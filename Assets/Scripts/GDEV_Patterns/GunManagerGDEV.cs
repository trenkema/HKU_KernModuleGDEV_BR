using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class GunManagerGDEV : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private List<GameObject> weaponInventory = new List<GameObject>();
    [SerializeField] private List<GunLibraryGDEV> gunSlotLibrary = new List<GunLibraryGDEV>();
    [SerializeField] private int inventorySize = 2;
    [SerializeField] private GameObject[] allWeapons;
    [SerializeField] private GameObject inventory;
    [SerializeField] private Image gunIcon;
    [SerializeField] private TextMeshProUGUI currentBulletsInformation;
    [SerializeField] private TextMeshProUGUI magazineInformation;

    // Private Non-Inspector Variables
    private int activeSlot = 0;
    private int freeWeaponSlot = 0;
    private bool hasStarted = false;
    private bool isReloading = false;

    private void Start()
    {
        inventory.SetActive(false);
        hasStarted = false;
        isReloading = false;

        GameControllerGDEV.instance.gunManager = this;

        GameControllerGDEV.instance.SetBaseWeapon(0);

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
        EventSystem<bool>.Unsubscribe(Event_Type.PLAYER_RELOADING, CheckPlayerReloading);
    }

    private void Update()
    {
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
            weaponInventory[activeSlot].SetActive(true);
            GetWeaponInfo(weaponInventory[activeSlot]).ResetAmmo();
            GetWeaponInfo(weaponInventory[activeSlot]).EquipWeapon();
        }
    }

    public void SetSpawnWeapon(int _index)
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

    public void SwitchSlot(int _upDown)
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

        weaponInventory[activeSlot].SetActive(true);
        GetWeaponInfo(weaponInventory[activeSlot]).EquipWeapon();
    }

    public GunGDEV GetWeaponInfo(GameObject _weapon)
    {
        return _weapon.GetComponent<GunGDEV>();
    }

    public void StartGame()
    {
        inventory.SetActive(true);
        weaponInventory[activeSlot].SetActive(true);
        weaponInventory[activeSlot].GetComponent<GunGDEV>().ResetAmmo();

        hasStarted = true;
    }

    public void GameOver()
    {
        weaponInventory[activeSlot].GetComponent<GunGDEV>().UnEquipWeapon();
        PlayerDied();

        hasStarted = false;
    }

    public void UpdateGunUI()
    {
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
        freeWeaponSlot = weaponInventory.Count;
    }

    public void PlayerDied()
    {
        EventSystem.Unsubscribe(Event_Type.START_GAME, StartGame);
        EventSystem.Unsubscribe(Event_Type.END_GAME, GameOver);
        EventSystem.Unsubscribe(Event_Type.PLAYER_DEATH, PlayerDied);
    }

    private void CheckPlayerReloading(bool _isReloading)
    {
        isReloading = _isReloading;
    }
}

[System.Serializable]
public class GunLibraryGDEV
{
    public GameObject[] WeaponToUse;
}
