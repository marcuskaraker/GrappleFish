using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class GrapplingGun : MonoBehaviour
{
    [SerializeField] Transform firePoint;
    [SerializeField] Hook hookPrefab;
    [SerializeField] GameObject hookOnWeapon;
    [SerializeField] float fireForce = 5f;
    [SerializeField] float maxDistance = 10f;

    Hook spawnedHook;

    LineRenderer lineRenderer;

    PlayerController parentPlayer;

    public bool HookOut { get { return (spawnedHook != null); } }

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        parentPlayer = GetComponentInParent<PlayerController>();
    }

    private void Update()
    {
        if (spawnedHook)
        {
            Vector3[] linePositions = new Vector3[] { firePoint.transform.position, spawnedHook.transform.position };

            lineRenderer.enabled = true;
            lineRenderer.positionCount = 2;
            lineRenderer.SetPositions(linePositions);
            hookOnWeapon.SetActive(false);

            if (spawnedHook.IsStuck)
            {
                parentPlayer.SetJoint(spawnedHook.HookRigidBody);
            }
            
            if (Vector3.Distance(transform.position, spawnedHook.transform.position) > maxDistance)
            {
                Destroy(spawnedHook.gameObject);
            }
        }
        else
        {
            lineRenderer.enabled = false;
            hookOnWeapon.SetActive(true);
        }
    }

    public bool Fire(Vector3 target)
    {
        if (spawnedHook != null)
        {
            return false;
        }

        spawnedHook = Instantiate(hookPrefab, firePoint.position + firePoint.right, Quaternion.identity);
        spawnedHook.transform.right = firePoint.transform.right;

        spawnedHook.HookRigidBody.AddForce(spawnedHook.transform.right * fireForce, ForceMode.Impulse);
        spawnedHook.InitHook(firePoint);

        return true;
    }

    public void ReturnHook()
    {
        if (spawnedHook == null)
        {
            return;
        }

        spawnedHook.ReturnToGun();

        if (spawnedHook.IsStuck) // The hook is stuck in terrain, move player towards hook.
        {
            parentPlayer.MoveToHook(spawnedHook);
        }
    }

    public void UnstuckHook()
    {
        if (spawnedHook != null)
        {
            spawnedHook.Unstuck();
            parentPlayer.SetJoint(null);
        }
    }
}
