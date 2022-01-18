using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SpellType { Normal, Ice, Fire }

public class BulletSpell : IBulletSpell
{
    public int damage { get; set; }
    public List<int> particleIndexes { get; set; }
    public SpellType spellTypes { get; set; } = SpellType.Normal;

    public BulletSpell(int _damage)
    {
        damage = _damage;
        particleIndexes = new List<int>();
    }

    public void Cast()
    {
        Debug.Log("Damage: " + damage + " " + spellTypes);
    }

    public void TakeDamage(IDamageable _target)
    {
        if (_target != null)
        {
            Debug.Log("Damage: " + damage + " " + spellTypes);

            _target.TakeDamage(damage, "");
        }
    }
}
