using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] Renderer model;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && GameManager.instance.playerSpawnPos.transform.position != transform.position)
        {
            Debug.Log("Player CheckPoint");
            GameManager.instance.playerSpawnPos.transform.position = transform.position;
            StartCoroutine(FlashModel());
        }
    }

    IEnumerator FlashModel()
    {
        // Replace with cached color and SerializeField color
        model.material.color = Color.red;
        GameManager.instance.checkpointPopup.SetActive(true);
        yield return new WaitForSeconds(0.3f);
        GameManager.instance.checkpointPopup.SetActive(false);
        model.material.color = Color.white;
    }
}
