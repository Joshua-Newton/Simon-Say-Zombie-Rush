using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMelee : EnemyAI
{
    [SerializeField] float meleeHitTime;
    [SerializeField] float meleeDelay;
    [SerializeField] float meleeTriggerRange;
    [SerializeField] Collider meleeHitBox;
    bool isMeleeing;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        if (meleeHitBox != null)
        {
            meleeHitBox.enabled = false;
        }
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }

    protected override void Attack()
    {
        if(!isMeleeing && playerDir.magnitude < meleeTriggerRange)
        {
            StartCoroutine(Melee());
        }
    }

    IEnumerator Melee()
    {
        isMeleeing = true;
        meleeHitBox.enabled = true;
        yield return new WaitForSeconds(meleeHitTime);
        meleeHitBox.enabled = false;
        yield return new WaitForSeconds(meleeDelay);
        isMeleeing = false;
    }

}
