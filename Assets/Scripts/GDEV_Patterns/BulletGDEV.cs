using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletGDEV : MonoBehaviour
{
    [Header("Bullet Settings")]
    [SerializeField] private int damage = 10;
    [SerializeField] private float speed = 25f;
    [SerializeField] private float destroyTime = 5f;
    [SerializeField] private LayerMask layersToInteract;

    private void OnEnable()
    {
        Invoke("DestroyBullet", destroyTime);
    }

    private void OnDisable()
    {
        CancelInvoke();
    }

    private void Update()
    {
        MoveBullet();
    }

    private void MoveBullet()
    {
        RaycastHit hitInfo;
        if (Physics.Raycast(transform.position, transform.right, out hitInfo, speed * Time.deltaTime, layersToInteract))
        {
            hitInfo.collider.gameObject.GetComponent<IDamageable>()?.TakeDamage(damage, "Enemy");
            gameObject.SetActive(false);
        }
        else
        {
            transform.Translate(transform.right * speed * Time.deltaTime, Space.World);
        }
    }

    private void DestroyBullet()
    {
        gameObject.SetActive(false);
    }
}
