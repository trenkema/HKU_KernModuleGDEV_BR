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

    private ISpell someSpell;

    [SerializeField] GameObject[] bulletParticles;

    private void OnEnable()
    {
        foreach (var particle in bulletParticles)
        {
            particle.SetActive(false);
        }

        someSpell = new Spell(damage);
        someSpell.Cast();

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
            //hitInfo.collider.gameObject.GetComponent<IDamageable>()?.TakeDamage(damage, "Enemy");
            someSpell.TakeDamage(hitInfo.collider.gameObject.GetComponent<IDamageable>());
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

    public ISpell GetSpell()
    {
        return someSpell;
    }

    public void SetSpell(ISpell _spell)
    {
        someSpell = _spell;

        SetDecoration();
    }

    private void SetDecoration()
    {
        foreach (var particle in someSpell.particleIndexes)
        {
            if (particle != -1)
            {
                bulletParticles[particle].SetActive(true);
            }
        }
    }
}
