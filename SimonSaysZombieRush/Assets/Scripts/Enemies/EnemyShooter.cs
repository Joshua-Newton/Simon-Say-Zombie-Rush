using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyShooter : EnemyAI
{
    [Header("----- Shooting -----")]
    [SerializeField] GameObject bullet;
    [SerializeField] Transform shootPos;
    [SerializeField] float shootDelay;
    bool isShooting;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }

    protected override void Attack()
    {
        if(!isShooting)
        {
            StartCoroutine(Shoot());
        }
    }
    IEnumerator Shoot()
    {
        isShooting = true;
        Instantiate(bullet, shootPos.position, transform.rotation);
        yield return new WaitForSeconds(shootDelay);
        isShooting = false;
    }

}
