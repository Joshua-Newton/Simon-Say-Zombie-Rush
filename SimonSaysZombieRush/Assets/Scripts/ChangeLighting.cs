using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeLighting : MonoBehaviour
{
    [SerializeField] Color lightColor;
    Color colorOriginal;

    // Start is called before the first frame update
    void Start()
    {
        colorOriginal = RenderSettings.ambientLight;
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            RenderSettings.ambientLight = lightColor;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            RenderSettings.ambientLight = colorOriginal;
        }
    }
}
