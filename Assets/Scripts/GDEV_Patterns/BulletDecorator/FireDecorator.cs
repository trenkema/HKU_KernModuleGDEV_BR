using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireDecorator : BulletDecorator
{
    public FireDecorator(int _damage, int _particleIndex) : base(_damage, _particleIndex) { }

    public override IBulletSpell Decorate(IBulletSpell spell)
    {
        Debug.Log("Add Some Fire To It!");
        spell.spellTypes |= SpellType.Fire;
        spell.damage += damage;

        if (particleIndex != -1)
        {
            spell.particleIndexes.Add(particleIndex);
        }

        return spell;
    }

    public override IBulletSpell UnDecorate(IBulletSpell spell)
    {
        if (spell.spellTypes.HasFlag(SpellType.Fire))
        {
            Debug.Log("Removed The Fire From It!");
            spell.spellTypes &= ~SpellType.Fire;
            spell.damage -= damage;

            if (particleIndex != -1)
            {
                spell.particleIndexes.Remove(particleIndex);
            }
        }

        return spell;
    }
}
