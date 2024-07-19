using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]

public class WeaponStats : ScriptableObject
{
    public GameObject weaponModel;
    public int damage;
    public int damageRange;
    public float damageDelay;
    public int ammoCurrent;
    public int ammoMax;

    public ParticleSystem hitEffect;
    public AudioClip shootSound;
    public AudioClip meleeSound;
    public float shootVol;
    public float meleeVol;

}
