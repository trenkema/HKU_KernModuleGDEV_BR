using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class GunPickUp : MonoBehaviourPunCallbacks
{
    PhotonView PV;

    [Header("Gun Pickup Settings")]
    [SerializeField] private int weaponIndex;
    [SerializeField] private float destroyTime = 7.5f;

    private void Awake()
    {
        PV = GetComponent<PhotonView>();
    }

    private void Start()
    {
        if (!PV.IsMine)
            return;

        Invoke("DestroyPickup", destroyTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<GunManager>())
        {
            if (PV.IsMine)
            {
                other.GetComponent<GunManager>().AddWeapon(weaponIndex);
                PhotonNetwork.Destroy(gameObject);
            }
        }
    }

    private void DestroyPickup()
    {
        if (PV.IsMine)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }
}
