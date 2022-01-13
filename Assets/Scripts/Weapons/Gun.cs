using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
using UnityEngine.UI;

public class Gun : MonoBehaviour
{
    [Header("Inventory Settings")]
    public string gunName;
    public Sprite icon;

    [Header("Gun Settings")]
    [SerializeField] private bool allowButtonHold;
    [SerializeField] private float timeBetweenShooting, spread, range, reloadTime, timeBetweenMultipleBullets;
    [SerializeField] private int bulletsPerTap;

    public int magazineSize;
    public int weaponIndex;
    public int bulletsLeft { get; private set; }

    [Header("References")]
    [SerializeField] private GameObject barrelCenter;
    [SerializeField] private AudioClip gunShotSound;
    [SerializeField] private AudioClip gunReloadSound;
    [SerializeField] private AudioClip gunEmptySound;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private NetworkPlaySound networkPlaySound;
    [SerializeField] private RaycastHit rayHit;
    [SerializeField] private LayerMask whatIsEnememy;
    [SerializeField] private LayerMask bulletHoleLayer;
    [SerializeField] public string bulletResourceName;
    [SerializeField] private GameObject gunHolder;
    public string gunResourceName;

    [Header("Graphics")]
    [SerializeField] private GameObject bulletHoleGraphic;
    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private TextMeshProUGUI bulletsInformation;

    [Header("Multiple Barrels")]
    [SerializeField] private bool hasMultipleBarrels = false;
    [SerializeField] private GameObject[] barrels;

    // Private Non-Inspector Variables
    private PhotonView PV;

    private bool shooting, readyToShoot, reloading;
    private bool canShoot;
    private bool canHold = true;
    private int bulletsShot;

    private void Awake()
    {
        PV = GetComponent<PhotonView>();
    }

    private void Start()
    {
        if (!PV.IsMine)
            return;

        //networkPlaySound = GameController.instance.networkPlaySound;
        bulletsLeft = magazineSize;
        readyToShoot = true;
        canShoot = true;
    }

    private void Update()
    {
        if (!PV.IsMine)
            return;

        MyInput();
    }

    private void MyInput()
    {
        if (allowButtonHold && canHold)
        {
            shooting = Input.GetKey(KeyCode.Mouse0);
        }
        else
        {
            shooting = Input.GetKeyDown(KeyCode.Mouse0);
        }

        if (Input.GetKeyDown(KeyCode.R) && bulletsLeft < magazineSize && !reloading && canShoot)
        {
            Reload();
        }

        if (readyToShoot && shooting && !reloading && bulletsLeft > 0 && canShoot)
        {
            bulletsShot = bulletsPerTap;
            Shoot();
        }
        else if (readyToShoot && shooting && !reloading && bulletsLeft == 0 && canShoot)
        {
            canHold = false;
            audioSource.clip = gunEmptySound;
            audioSource.Play();
            networkPlaySound.PlaySoundAtOwnTransform(gunEmptySound);
        }
    }

    private void Shoot()
    {
        readyToShoot = false;
        audioSource.clip = gunShotSound;
        audioSource.Play();
        networkPlaySound.PlaySoundAtOwnTransform(gunShotSound);

        if (hasMultipleBarrels)
        {
            foreach (var item in barrels)
            {
                GameObject bullet = PhotonNetwork.Instantiate(bulletResourceName, item.transform.position, item.transform.rotation);
            }

            bulletsLeft -= barrels.Length;

            EventSystem.RaiseEvent(Event_Type.UPDATE_GUN_UI);

            Invoke("ResetShot", timeBetweenShooting);

        }
        else
        {
            GameObject bullet = PhotonNetwork.Instantiate(bulletResourceName, barrelCenter.transform.position, gunHolder.transform.localRotation);

            bulletsLeft--;
            bulletsShot--;

            EventSystem.RaiseEvent(Event_Type.UPDATE_GUN_UI);

            Invoke("ResetShot", timeBetweenShooting);

            if (bulletsShot > 0 && bulletsLeft > 0)
            {
                Invoke("Shoot", timeBetweenMultipleBullets);
            }
        }
    }

    private void ResetShot()
    {
        readyToShoot = true;
    }

    private void Reload()
    {
        EventSystem<bool>.RaiseEvent(Event_Type.PLAYER_RELOADING, true);
        audioSource.clip = gunReloadSound;
        audioSource.Play();
        networkPlaySound.PlaySoundAtOwnTransform(gunReloadSound);
        reloading = true;
        Invoke("ReloadFinished", reloadTime);
    }

    private void ReloadFinished()
    {
        canHold = true;
        bulletsLeft = magazineSize;
        EventSystem.RaiseEvent(Event_Type.UPDATE_GUN_UI);
        EventSystem<bool>.RaiseEvent(Event_Type.PLAYER_RELOADING, false);
        reloading = false;
    }

    public void EquipWeapon()
    {
        canShoot = true;
        EventSystem.RaiseEvent(Event_Type.UPDATE_GUN_UI);
    }

    public void UnEquipWeapon()
    {
        canShoot = false;
    }

    public void ResetAmmo()
    {
        bulletsLeft = magazineSize;
        EventSystem.RaiseEvent(Event_Type.UPDATE_GUN_UI);
    }
}
