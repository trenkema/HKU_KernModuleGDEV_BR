using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GunGDEV : MonoBehaviour
{
    ObjectPooler objectPool;

    [Header("Inventory Settings")]
    public string gunName;
    public Sprite icon;

    [Header("Gun Settings")]
    [SerializeField] private string gunPoolName;
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
    [SerializeField] private RaycastHit rayHit;
    [SerializeField] private LayerMask whatIsEnememy;
    [SerializeField] private LayerMask bulletHoleLayer;
    [SerializeField] private GameObject gunHolder;

    [Header("Graphics")]
    [SerializeField] private GameObject bulletHoleGraphic;
    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private TextMeshProUGUI bulletsInformation;

    [Header("Multiple Barrels")]
    [SerializeField] private bool hasMultipleBarrels = false;
    [SerializeField] private GameObject[] barrels;

    // Private Non-Inspector Variables
    private bool shooting, readyToShoot, reloading;
    private bool canShoot;
    private bool canHold = true;
    private int bulletsShot;

    private void Start()
    {
        for (int i = 0; i < GameControllerGDEV.instance.objectPoolers.Length; i++)
        {
            if (GameControllerGDEV.instance.objectPoolers[i].poolName == gunPoolName)
            {
                objectPool = GameControllerGDEV.instance.objectPoolers[i];
            }
        }

        bulletsLeft = magazineSize;
        readyToShoot = true;
        canShoot = true;
    }

    private void Update()
    {
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
        }
    }

    private void Shoot()
    {
        readyToShoot = false;
        audioSource.clip = gunShotSound;
        audioSource.Play();

        if (hasMultipleBarrels)
        {
            foreach (var item in barrels)
            {
                GameObject bullet = objectPool.RequestPooledObject();

                if (bullet != null)
                {
                    bullet.SetActive(true);
                    bullet.transform.position = barrelCenter.transform.position;
                    bullet.transform.rotation = barrelCenter.transform.rotation;
                }
            }

            bulletsLeft -= barrels.Length;

            EventSystem.RaiseEvent(Event_Type.UPDATE_GUN_UI);

            Invoke("ResetShot", timeBetweenShooting);

        }
        else
        {
            GameObject bullet = objectPool.RequestPooledObject();

            if (bullet != null)
            {
                bullet.SetActive(true);
                bullet.transform.position = barrelCenter.transform.position;
                bullet.transform.rotation = barrelCenter.transform.rotation;
            }

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
        reloading = true;
        Invoke("ReloadFinished", reloadTime);
    }

    private void ReloadFinished()
    {
        canHold = true;
        bulletsLeft = magazineSize;
        EventSystem<bool>.RaiseEvent(Event_Type.PLAYER_RELOADING, false);
        EventSystem.RaiseEvent(Event_Type.UPDATE_GUN_UI);
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
