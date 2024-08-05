using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceMouse : MonoBehaviour
{
    float distanceToCamera;

    Vector3 mousePos;

    // Start is called before the first frame update
    void Start()
    {
        distanceToCamera = (Camera.main.transform.position - GameManager.instance.player.transform.position).magnitude;
    }

    // Update is called once per frame
    void Update()
    {
        RotateToFaceMouse();
    }

    void RotateToFaceMouse()
    {
        mousePos = Input.mousePosition;
        mousePos = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, distanceToCamera));
        mousePos.y = transform.position.y;

        transform.LookAt(mousePos);
    }

    public Vector3 GetCurrentMousePos()
    {
        return mousePos;
    }

}
