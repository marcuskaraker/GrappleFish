using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateTowardsCamera : MonoBehaviour
{
    Vector3 lookPos;
    Camera mainCamera;

    [SerializeField] bool skipY = true;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        float y = skipY ? transform.position.y : mainCamera.transform.position.y;
        lookPos = new Vector3(mainCamera.transform.position.x, y, mainCamera.transform.position.z);

        Vector3 lookDir = lookPos - transform.position;
        transform.rotation = Quaternion.LookRotation(-lookDir, Vector3.up);
    }
}
