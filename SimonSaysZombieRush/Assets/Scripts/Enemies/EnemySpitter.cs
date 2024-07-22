using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpitter : EnemyAI
{
    [Header("----- Spitting -----")]
    [SerializeField] GameObject spitProjectile;
    [SerializeField] Transform shootPos;
    [SerializeField] float spitDelay;
    [Range(0.1f, 10)] [SerializeField] float spitSpeed;
    bool isSpitting;


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
        if (!isSpitting)
        {
            StartCoroutine(Spit());
        }
    }
    IEnumerator Spit()
    {
        isSpitting = true;
        GameObject projectile = Instantiate(spitProjectile, shootPos.position, transform.rotation);
        Rigidbody spitRB = projectile.GetComponent<Rigidbody>();
        spitRB.velocity = playerDir.normalized * spitSpeed;
        yield return new WaitForSeconds(spitDelay);
        isSpitting = false;
    }

}
