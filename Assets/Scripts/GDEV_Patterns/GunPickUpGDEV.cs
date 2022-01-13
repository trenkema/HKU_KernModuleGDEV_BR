using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunPickUpGDEV : MonoBehaviour
{
    [Header("Gun Pickup Settings")]
    [SerializeField] private int weaponIndex;
    [SerializeField] private float destroyTime = 7.5f;

    private void Start()
    {
        Invoke("DestroyPickup", destroyTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<GunManagerGDEV>())
        {
            other.GetComponent<GunManagerGDEV>().AddWeapon(weaponIndex);
        }
    }

    private void DestroyPickup()
    {
        Destroy(gameObject);
    }
}
