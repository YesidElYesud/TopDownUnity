using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageInfo
{
    public float amount;
    public GameObject source;
    public string damageType;

    public DamageInfo(float amount, GameObject source, string damageType = "default")
    {
        this.amount = amount;
        this.source = source;
        this.damageType = damageType.ToLower();
    }
}
