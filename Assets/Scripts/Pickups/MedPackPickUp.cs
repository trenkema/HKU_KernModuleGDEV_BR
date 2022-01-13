using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class MedPackPickUp : MonoBehaviour
{
    [Header("MedPack Pickup Settings")]
    [SerializeField] int healAmount;
    [SerializeField] private float destroyTime = 7.5f;

    // Private Non-Inspector Variables
    PhotonView PV;

    private void Start()
    {
        PV = GetComponent<PhotonView>();

        if (!PV.IsMine)
            return;

        Invoke("DestroyPickup", destroyTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<_Player>())
        {
            if (PV.IsMine)
            {
                other.GetComponent<_Player>().HealHealth(healAmount);
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
