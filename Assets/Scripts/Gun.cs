using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public string GunName;
    public float range;
    public float accuracy;
    public float firerate;
    public float reloadtime;
    public float bulletspeed;

    public int damage;

    public int reloadBulletCount;
    public int currentBulletCount;
    public int MaxBulletCount;
    public int carryBulletCount;

    public AudioClip FireSound;
}
