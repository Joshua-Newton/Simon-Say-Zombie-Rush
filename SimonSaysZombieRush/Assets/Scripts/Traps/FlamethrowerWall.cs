using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FlamethrowerWall : MonoBehaviour
{
    [SerializeField] int emissionRest = 2;
    [SerializeField] float emissionTime = 1;
    [SerializeField] float flameParticleLifetime = 1;
    [SerializeField] float flameParticleSpeed = 5;
    [SerializeField] float sparkTime = .5f;
    [SerializeField] Collider flameHitBox;
    [SerializeField] ParticleSystem flameSparks;
    [SerializeField] ParticleSystem flamethrower;
    bool isFlaming;
    bool expanding;
    
    float farthesetDistance;
    Transform flameHitBoxTransform;
    BoxCollider flameHitBoxCollider;
    Vector3 initialHitBoxSize;
    private void Start()
    {
        ParticleSystem.MainModule mainSettings = flamethrower.main;
        mainSettings.startLifetime = flameParticleLifetime;
        mainSettings.startSpeed = flameParticleSpeed;

        farthesetDistance = flameParticleLifetime * flameParticleSpeed;
        flameHitBoxTransform = flameHitBox.gameObject.transform;
        flameHitBoxCollider = flameHitBox.gameObject.GetComponent<BoxCollider>();
        initialHitBoxSize = flameHitBoxCollider.size;
    }

    // Update is called once per frame
    void Update()
    {
        flamethowerEmission();
        expanding = flamethrower.isEmitting;
        

    }

    public void flamethowerEmission()
    {
        if (!isFlaming)
        {
            flameHitBox.gameObject.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            StartCoroutine(sparksTimer());
            StartCoroutine(flamehitBoxTimer());
        }
        else if(expanding)
        {
            flameHitBoxCollider.center = Vector3.Lerp(flameHitBoxCollider.center, new Vector3(0, 0, farthesetDistance / 2), flameParticleSpeed/2 * Time.deltaTime);
            flameHitBoxCollider.size = Vector3.Lerp(flameHitBoxCollider.size, new Vector3(initialHitBoxSize.x, initialHitBoxSize.y, farthesetDistance), flameParticleSpeed/2 * Time.deltaTime);
        }
        else
        {
            flameHitBoxCollider.center = Vector3.Lerp(flameHitBoxCollider.center, new Vector3(0, 0, farthesetDistance), flameParticleSpeed/2 * Time.deltaTime);
            flameHitBoxCollider.size = Vector3.Lerp(flameHitBoxCollider.size, new Vector3(initialHitBoxSize.x, initialHitBoxSize.y, 0), flameParticleSpeed/2 * Time.deltaTime);
            if (flamethrower.particleCount <= 0 || flameHitBoxCollider.size.z < 0.05f)
            {
                flameHitBox.gameObject.GetComponent<Damage>().DisableAndStopCoroutines();
            }
        }
    }

    IEnumerator flamehitBoxTimer()
    {
        flamethrower.Play(true);
        isFlaming = true;
        flameHitBox.gameObject.SetActive(true);

        yield return new WaitForSeconds(emissionTime);
        flamethrower.Stop();
        
        yield return new WaitForSeconds(emissionRest);
        flameHitBoxCollider.size = initialHitBoxSize;
        flameHitBoxCollider.center = Vector3.zero;
        isFlaming = false;
    }

    IEnumerator sparksTimer()
    {
        flameSparks.Play(true);
        yield return new WaitForSeconds((int)sparkTime);
        flameSparks.Stop();
    }
}
