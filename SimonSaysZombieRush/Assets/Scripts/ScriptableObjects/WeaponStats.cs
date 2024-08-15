using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]

public class WeaponStats : ScriptableObject
{
    public enum WeaponType { Gun, Melee, Grenade, Projectile }

    public GameObject weaponModel;
    public GameObject projectile;
    public WeaponType weaponType;
    public int damage;
    public int damageRange;
    public float damageDelay;
    public int ammoCurrent;
    public int ammoMax;

    public ParticleSystem hitEffect;
    public AudioClip shootSound;
    public float shootVol;

}
