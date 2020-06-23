using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingText : MonoBehaviour
{
    public float destroyTime = 3f;
    public Vector3 baseOffset = new Vector3(0, 1f, 0);
    public Vector3 rndOffsetMagnitude = new Vector3(0.5f, 0.5f, 0);

    private void Start()
    {
        Destroy(gameObject, destroyTime);

        Vector3 rndOffset = new Vector3(
            Random.Range(-rndOffsetMagnitude.x, rndOffsetMagnitude.x),
            Random.Range(-rndOffsetMagnitude.y, rndOffsetMagnitude.y),
            Random.Range(-rndOffsetMagnitude.z, rndOffsetMagnitude.z)
        );

        transform.position += (baseOffset + rndOffset);
    }
}
