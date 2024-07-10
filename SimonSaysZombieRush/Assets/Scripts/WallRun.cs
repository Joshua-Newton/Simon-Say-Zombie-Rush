using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallRun : MonoBehaviour
{
    [SerializeField] Collider wallRunTrigger;
    [SerializeField] float wallRunCooldown;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.instance.player.GetComponent<Player>().InitiateWallRun(wallRunTrigger);


        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.instance.player.GetComponent<Player>().AbruptEndWallRun();
            StartCoroutine(Cooldown());
        }
    }

    IEnumerator Cooldown()
    {
        wallRunTrigger.enabled = false;
        yield return new WaitForSeconds(wallRunCooldown);
        wallRunTrigger.enabled = true;
    }

}
