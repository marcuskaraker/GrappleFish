using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] Transform target;

    Vector3 offset;

    private void Awake()
    {
        offset = transform.position - target.position;
    }

    private void Update()
    {
        transform.position = target.position + offset;
    }
}
