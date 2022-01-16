using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGDEV : MonoBehaviour, IDamageable
{
    [Header("Enemy Settings")]
    [SerializeField] float speed = 5f;
    [SerializeField] int health = 100;
    [SerializeField] int damage = 25;

    [Header("References")]
    [SerializeField] private Explosion explosionScript;
    [SerializeField] private List<DropLibraryGDEV> dropLibrary = new List<DropLibraryGDEV>();

    // Private Non-Inspector Variables
    private GameObject target;

    private List<_PlayerGDEV> players = new List<_PlayerGDEV>();

    private void Start()
    {
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
        health -= _damage;

        if (health <= 0)
        {
            Died(true);
        }
    }

    public void TakeDamageOverTime(int _damage, int _duration, int _timeBetweenDamage, string _killerSource)
    {
        StartCoroutine(DamageOverTime(_damage, _duration, _timeBetweenDamage));
    }

    private IEnumerator DamageOverTime(int _damage, int _duration, int _timeBetweenDamage)
    {
        for (int i = 0; i < _duration; i++)
        {
            yield return new WaitForSeconds(_timeBetweenDamage);

            health -= _damage;
        }
    }

    public void Died(bool beenShot)
    {
        RPC_Explode();

        int randomInt = Random.Range(1, 4);

        for (int i = 0; i < dropLibrary.Count; i++)
        {
            if (randomInt == i && beenShot)
            {
                if (!dropLibrary[i].noDrop)
                {
                    Instantiate(dropLibrary[i].dropPrefab, new Vector3(transform.position.x, dropLibrary[i].dropHeight, transform.position.z), Quaternion.identity);
                }
            }
        }
    }

    private void RPC_Explode()
    {
        explosionScript.Explode();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<_PlayerGDEV>())
        {
            other.gameObject.GetComponent<IDamageable>()?.TakeDamage(damage, "A MINION");
            Died(false);
        }
    }

    private void GetPlayers()
    {
        players = GameControllerGDEV.instance.GetPlayers();
    }
}

[System.Serializable]
public class DropLibraryGDEV
{
    public GameObject dropPrefab;
    public float dropHeight;
    public bool noDrop;
}
