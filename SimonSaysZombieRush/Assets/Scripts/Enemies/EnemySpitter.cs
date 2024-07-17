using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpitter : EnemyAI
{
    [SerializeField] GameObject spitProjectile;
    [SerializeField] Transform shootPos;
    [SerializeField] float spitDelay;
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
        Instantiate(spitProjectile, shootPos.position, transform.rotation);
        yield return new WaitForSeconds(spitDelay);
        isSpitting = false;
    }

}
