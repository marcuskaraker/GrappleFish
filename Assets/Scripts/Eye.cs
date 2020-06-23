using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Eye : MonoBehaviour
{
    [SerializeField] GameObject iris;
    [SerializeField] float lerpSpeed = 7f;

    Vector3 target;

    private void Update()
    {
        // Up, down
        Vector3 toTarget = (target - transform.position);
        float upDot = Vector3.Dot(transform.up, toTarget);

        Vector3 targetPos = new Vector3(
            iris.transform.localPosition.x,
            Mathf.Lerp(-0.1f, 0.22f, upDot),
            iris.transform.localPosition.z
        );

        iris.transform.localPosition = Vector3.Lerp(iris.transform.localPosition, targetPos, Time.deltaTime * lerpSpeed);


    }

    public void SetTargetPosition(Vector3 target)
    {
        this.target = target;
    }
}
