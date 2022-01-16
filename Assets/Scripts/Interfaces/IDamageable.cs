using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable
{
    void TakeDamage(int _damage, string _killerSource);

    void TakeDamageOverTime(int _damage, int _duration, int _timeBetweenDamage, string _killerSource);
}
