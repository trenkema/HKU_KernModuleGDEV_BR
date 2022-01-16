using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISpell
{
    int damage { get; set; }
    List<int> particleIndexes { get; set; }
    SpellType spellTypes { get; set; }
    void Cast();
    void TakeDamage(IDamageable _target);
}
