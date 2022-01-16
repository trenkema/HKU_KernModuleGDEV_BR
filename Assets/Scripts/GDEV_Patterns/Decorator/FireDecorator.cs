using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireDecorator : BulletDecorator
{
    public FireDecorator(int _damage, int _particleIndex) : base(_damage, _particleIndex) { }

    public override ISpell Decorate(ISpell spell)
    {
        Debug.Log("Add Some Fire To It!");
        spell.spellTypes |= SpellType.Fire;
        spell.damage += damage;
        return spell;
    }

    public override ISpell UnDecorate(ISpell spell)
    {
        if (spell.spellTypes.HasFlag(SpellType.Fire))
        {
            Debug.Log("Removed The Fire From It!");
            spell.spellTypes &= ~SpellType.Fire;
            spell.damage -= damage;
        }

        return spell;
    }
}
