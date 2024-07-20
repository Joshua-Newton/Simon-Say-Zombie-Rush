using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMelee : EnemyAI
{
    [Header("----- Melee Settings -----")]
    [SerializeField] float meleeHitTime;
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
        anim.SetTrigger("Attack");
        yield return new WaitForSeconds(meleeHitTime);
        isMeleeing = false;
    }

    public void EnableMeleeHitbox()
    {
        meleeHitBox.enabled = true;
    }

    public void DisableMeleeHitbox()
    {
        meleeHitBox.enabled = false;
    }

}
