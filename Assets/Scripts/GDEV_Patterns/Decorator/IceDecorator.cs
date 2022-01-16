using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceDecorator : BulletDecorator
{
    public IceDecorator(int _damage, int _particleIndex) : base(_damage, _particleIndex) { }

    public override ISpell Decorate(ISpell spell)
    {
        Debug.Log("Add Some Ice To It!");
        spell.spellTypes |= SpellType.Ice;
        spell.damage += damage;

        if (particleIndex != -1)
        {
            spell.particleIndexes.Add(particleIndex);
        }

        return spell;
    }

    public override ISpell UnDecorate(ISpell spell)
    {
        if (spell.spellTypes.HasFlag(SpellType.Ice))
        {
            Debug.Log("Removed The Ice From It!");
            spell.spellTypes &= ~SpellType.Ice;
            spell.damage -= damage;

            if (particleIndex != -1)
            {
                spell.particleIndexes.Remove(particleIndex);
            }
        }

        return spell;
    }
}
