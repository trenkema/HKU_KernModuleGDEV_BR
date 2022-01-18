using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradePickupGDEV : MonoBehaviour
{
    [Header("Upgrade Pickup Settings")]
    [SerializeField] private SpellType spellType;
    [SerializeField] private float destroyTime = 7.5f;

    private void Start()
    {
        Invoke("DestroyPickup", destroyTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<GunManagerGDEV>())
        {
            other.GetComponent<GunManagerGDEV>().AddSpell(spellType);
        }
    }

    private void DestroyPickup()
    {
        Destroy(gameObject);
    }
}
