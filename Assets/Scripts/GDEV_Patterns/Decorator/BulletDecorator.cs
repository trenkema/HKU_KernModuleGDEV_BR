using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BulletDecorator
{
    public int damage { get; set; }
    public int particleIndex { get; set; }

    public BulletDecorator(int _damage, int _particleIndex)
    {
        damage = _damage;
        particleIndex = _particleIndex;
    }

    public abstract ISpell Decorate(ISpell spell);

    public abstract ISpell UnDecorate(ISpell spell);
}