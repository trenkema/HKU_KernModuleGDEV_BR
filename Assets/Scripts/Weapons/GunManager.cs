using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using TMPro;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Photon.Realtime;

public class GunManager : MonoBehaviourPunCallbacks
{
    [Header("References")]
    [SerializeField] private List<GameObject> weaponInventory = new List<GameObject>();
    [SerializeField] private List<GunLibrary> gunSlotLibrary = new List<GunLibrary>();
    [SerializeField] private int inventorySize = 2;
    [SerializeField] private GameObject[] allWeapons;
    [SerializeField] private GameObject inventory;
    [SerializeField] private Image gunIcon;
    [SerializeField] private TextMeshProUGUI currentBulletsInformation;
    [SerializeField] private TextMeshProUGUI magazineInformation;

    // Private Non-Inspector Variables
    PhotonView PV;

    private int activeSlot = 0;
    private int freeWeaponSlot = 0;
    private bool hasStarted = false;
    private bool isReloading = false;


    private void Awake()
    {
        PV = GetComponent<PhotonView>();
    }

    private void Start()
    {
        if (!PV.IsMine)
            return;

        inventory.SetActive(false);
        hasStarted = false;
        isReloading = false;

        GameController.instance.gunManager = this;

        GameController.instance.SetBaseWeapon(0);

        EventSystem.Subscribe(Event_Type.START_GAME, StartGame);

        EventSystem.Subscribe(Event_Type.END_GAME, GameOver);

        EventSystem.Subscribe(Event_Type.PLAYER_DEATH, PlayerDied);

        EventSystem.Subscribe(Event_Type.UPDATE_GUN_UI, UpdateGunUI);

        EventSystem<bool>.Subscribe(Event_Type.PLAYER_RELOADING, CheckPlayerReloading);
    }

    private void OnDisable()
    {
        EventSystem.Unsubscribe(Event_Type.START_GAME, StartGame);
        EventSystem.Unsubscribe(Event_Type.END_GAME, GameOver);
        EventSystem.Unsubscribe(Event_Type.PLAYER_DEATH, PlayerDied);
        EventSystem.Unsubscribe(Event_Type.UPDATE_GUN_UI, UpdateGunUI);
        EventSystem<bool>.Unsubscribe(Event_Type.PLAYER_RELOADING, CheckPlayerReloading);
    }

    private void Update()
    {
        if (!PV.IsMine)
            return;

        if (hasStarted)
        {
            if (Input.GetAxis("Mouse ScrollWheel") > 0f)
            {
                SwitchSlot(1);
            }
            else if (Input.GetAxis("Mouse ScrollWheel") < 0f)
            {
                SwitchSlot(-1);
            }
        }
    }

    public void AddWeapon(int _index)
    {
        PV.RPC("RPC_AddWeapon", RpcTarget.All, _index);
    }

    [PunRPC]
    public void RPC_AddWeapon(int _index)
    {
        if (!PV.IsMine)
            return;

        if (weaponInventory.Count < inventorySize)
        {
            GetFreeWeaponSlot();
            weaponInventory.Add(gunSlotLibrary[freeWeaponSlot].WeaponToUse[_index]);
            GetWeaponInfo(weaponInventory[freeWeaponSlot]).ResetAmmo();
            GetWeaponInfo(weaponInventory[freeWeaponSlot]).EquipWeapon();
        }
        else
        {
            // Drop Current Weapon On Floor
            //string gunResourceName = GetWeaponInfo(weaponInventory[activeSlot]).gunResourceName;
            //PhotonNetwork.Instantiate(gunResourceName, transform.position, Quaternion.identity);
            weaponInventory[activeSlot].SetActive(false);

            weaponInventory[activeSlot] = gunSlotLibrary[activeSlot].WeaponToUse[_index];
            UpdateWeaponNetwork();
            weaponInventory[activeSlot].SetActive(true);
            GetWeaponInfo(weaponInventory[activeSlot]).ResetAmmo();
            GetWeaponInfo(weaponInventory[activeSlot]).EquipWeapon();
        }
    }

    public void SetSpawnWeapon(int _index)
    {
        if (PV.IsMine)
        {
            if (weaponInventory.Count >= 1)
            {
                weaponInventory.RemoveAt(activeSlot);

                weaponInventory.Insert(activeSlot, gunSlotLibrary[activeSlot].WeaponToUse[_index]);
            }
            else
            {
                GetFreeWeaponSlot();
                weaponInventory.Add(gunSlotLibrary[freeWeaponSlot].WeaponToUse[_index]);
                freeWeaponSlot++;
            }
        }
    }

    public void SwitchSlot(int _upDown)
    {
        if (PV.IsMine)
        {
            if (isReloading)
                return;

            weaponInventory[activeSlot].SetActive(false);

            activeSlot += _upDown;

            if (activeSlot > weaponInventory.Count - 1)
            {
                activeSlot = 0;
            }
            else if (activeSlot < 0)
            {
                activeSlot = weaponInventory.Count - 1;
            }

            UpdateWeaponNetwork();

            weaponInventory[activeSlot].SetActive(true);
            GetWeaponInfo(weaponInventory[activeSlot]).EquipWeapon();
        }
    }

    public void UpdateWeaponNetwork()
    {
        if (PV.IsMine)
        {
            Hashtable hash = new Hashtable();
            hash.Add("weaponIndex", GetWeaponInfo(weaponInventory[activeSlot]).weaponIndex);
            PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (!PV.IsMine && targetPlayer == PV.Owner)
        {
            if (targetPlayer.CustomProperties.ContainsKey("weaponIndex"))
            {
                string test = targetPlayer.CustomProperties["weaponIndex"].ToString();
                int index = int.Parse(test);

                foreach (var item in allWeapons)
                {
                    item.SetActive(false);
                }

                allWeapons[index].SetActive(true);
            }
        }
    }

    public Gun GetWeaponInfo(GameObject _weapon)
    {
        if (!PV.IsMine)
            return null;
        
        return _weapon.GetComponent<Gun>();
    }

    public void StartGame()
    {
        if (PV.IsMine)
        {
            inventory.SetActive(true);
            weaponInventory[activeSlot].SetActive(true);
            weaponInventory[activeSlot].GetComponent<Gun>().ResetAmmo();

            UpdateWeaponNetwork();
        }

        hasStarted = true;
    }

    public void GameOver()
    {
        if (PV.IsMine)
        {
            weaponInventory[activeSlot].GetComponent<Gun>().UnEquipWeapon();
            PlayerDied();
        }

        hasStarted = false;
    }

    public void UpdateGunUI()
    {
        if (!PV.IsMine)
            return;

        gunIcon.sprite = GetWeaponInfo(weaponInventory[activeSlot]).icon;
        currentBulletsInformation.SetText(GetWeaponInfo(weaponInventory[activeSlot]).bulletsLeft.ToString());
        magazineInformation.SetText(GetWeaponInfo(weaponInventory[activeSlot]).magazineSize.ToString());

        if (GetWeaponInfo(weaponInventory[activeSlot]).bulletsLeft == 0)
        {
            currentBulletsInformation.color = Color.red;
        }
        else
        {
            currentBulletsInformation.color = Color.white;
        }
    }

    public void GetFreeWeaponSlot()
    {
        if (!PV.IsMine)
            return;

        freeWeaponSlot = weaponInventory.Count;
    }

    public void PlayerDied()
    {
        EventSystem.Unsubscribe(Event_Type.START_GAME, StartGame);
        EventSystem.Unsubscribe(Event_Type.END_GAME, GameOver);
        EventSystem.Unsubscribe(Event_Type.PLAYER_DEATH, PlayerDied);
        EventSystem.Unsubscribe(Event_Type.UPDATE_GUN_UI, UpdateGunUI);
    }

    private void CheckPlayerReloading(bool _isReloading)
    {
        isReloading = _isReloading;
    }
}

[System.Serializable]
public class GunLibrary
{
    public GameObject[] WeaponToUse;
}
