using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Bullet : MonoBehaviourPun
{
    [Header("Bullet Settings")]
    [SerializeField] private int damage = 10;
    [SerializeField] private float speed = 25f;
    [SerializeField] private float destroyTime = 5f;
    [SerializeField] private LayerMask layersToInteract;

    // Private Non-Inspector Variables
    private PhotonView PV;

    private void Awake()
    {
        PV = GetComponent<PhotonView>();
    }

    private void Start()
    {
        if (!PV.IsMine)
            return;

        Invoke("DestroyBullet", destroyTime);
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
            if (PV.IsMine)
            {
                hitInfo.collider.gameObject.GetComponent<IDamageable>()?.TakeDamage(damage, PV.Controller.NickName);
                PhotonNetwork.Destroy(gameObject);
            }
        }
        else
        {
            transform.Translate(transform.right * speed * Time.deltaTime, Space.World);
        }
    }

    private void DestroyBullet()
    {
        if (PV.IsMine)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }
}
