using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Enemy : MonoBehaviourPunCallbacks, IDamageable
{
    [Header("Enemy Settings")]
    [SerializeField] float speed = 5f;
    [SerializeField] int health = 100;
    [SerializeField] int damage = 25;

    [Header("References")]
    [SerializeField] private Explosion explosionScript;
    [SerializeField] private List<DropLibrary> dropLibrary = new List<DropLibrary>();

    // Private Non-Inspector Variables
    private PhotonView PV;

    private GameObject target;

    private List<_Player> players = new List<_Player>();


    private void Start()
    {
        PV = GetComponent<PhotonView>();
        GetPlayers();
    }

    private void FixedUpdate()
    {
        Chase();
    }

    void Chase()
    {
        GameObject closestTarget = GetClosestTarget();

        if (closestTarget == null)
        {
            GetPlayers();
            target = null;
            return;
        }

        if (closestTarget != target)
        {
            target = closestTarget;
        }

        transform.Translate((target.transform.position - transform.position).normalized * Time.deltaTime * speed);
    }

    float GetDistanceFromTarget(GameObject player)
    {
        return Vector3.Distance(player.transform.position, transform.position);
    }

    GameObject GetClosestTarget()
    {
        GameObject closestTarget = null;
        float minDist = 999999999;

        for (int i = 0; i < players.Count; i++)
        {
            if (players[i] == null)
            {
                GetPlayers();
                return null;
            }
        }

        for (int i = 0; i < players.Count; i++)
        {
            float dist = GetDistanceFromTarget(players[i].gameObject);

            if (dist < minDist)
            {
                minDist = dist;
                closestTarget = players[i].gameObject;
            }
        }

        return closestTarget;
    }

    public void TakeDamage(int _damage, string _killerSource)
    {
        PV.RPC("RPC_TakeDamage", RpcTarget.All, _damage);
    }

    [PunRPC]
    void RPC_TakeDamage(int _damage)
    {
        health -= _damage;

        if (health <= 0)
        {
            Died(true);
        }
    }

    public void TakeDamageOverTime(int _damage, int _duration, int _timeBetweenDamage, string _killerSource)
    {
        throw new System.NotImplementedException();
    }

    public void Died(bool beenShot)
    {
        PV.RPC("RPC_Explode", RpcTarget.All);

        if (PV.IsMine)
        {
            int randomInt = Random.Range(1, 4);

            for (int i = 0; i < dropLibrary.Count; i++)
            {
                if (randomInt == i && beenShot)
                {
                    if (!dropLibrary[i].noDrop)
                    {
                        PhotonNetwork.Instantiate(dropLibrary[i].dropResourceName, new Vector3(transform.position.x, dropLibrary[i].dropHeight, transform.position.z), Quaternion.identity);
                    }
                }
            }

            PhotonNetwork.Destroy(gameObject);
        }
    }

    [PunRPC]
    private void RPC_Explode()
    {
        explosionScript.Explode();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<_Player>())
        {
            if (PV.IsMine)
            {
                other.gameObject.GetComponent<IDamageable>()?.TakeDamage(damage, "A MINION");
                Died(false);
            }
        }
    }

    private void GetPlayers()
    {
        players = GameController.instance.GetPlayers();
    }
}

[System.Serializable]
public class DropLibrary
{
    public string dropResourceName;
    public float dropHeight;
    public bool noDrop;
}
