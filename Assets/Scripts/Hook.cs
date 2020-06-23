using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hook : MonoBehaviour
{
    public float rayDistance = 1f;
    public LayerMask hitMask;
    public float baseDamage = 100f;
    public float returnForce = 20f;

    bool comingBack = false;

    Transform parentFirePoint;

    public bool IsStuck { get; private set; }
    public Rigidbody HookRigidBody { get; private set; }

    Collider lastHit;

    private void Awake()
    {
        HookRigidBody = GetComponent<Rigidbody>();
    }

    public void InitHook(Transform firePoint)
    {
        parentFirePoint = firePoint;
    }

    public void ReturnToGun()
    {
        comingBack = true;
    }

    public void Unstuck()
    {
        comingBack = true;
        IsStuck = false;
        SetRigidbodyActive(true);
    }

    private void Update()
    {
        if(!comingBack && !IsStuck)
        {
            // Raycast foward to extra check collision.
            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.right, out hit, rayDistance, hitMask))
            {
                if (hit.transform.tag == "Grabable")
                {
                    GetStuck();
                }
                else
                {
                    Destructible destructible = hit.transform.GetComponentInParent<Destructible>();
                    if (destructible != null && lastHit != hit.collider)
                    {
                        // Deal damage to destructible.
                        destructible.Hurt(baseDamage * Random.Range(1f, 2f));
                    }
                    else
                    {
                        // It hit a un-interactable wall, destroy.
                        Destroy(gameObject);
                    }
                }

                lastHit = hit.collider;
            }
        }

        if (comingBack && parentFirePoint && Vector3.Distance(transform.position, parentFirePoint.position) < 2f)
        {
            // The hook is back at the gun.
            Destroy(gameObject);
        }
    }

    private void FixedUpdate()
    {
        if (comingBack && parentFirePoint != null && !IsStuck)
        {
            HookRigidBody.AddForce((parentFirePoint.position - transform.position).normalized * returnForce);
        }
    }

    private void GetStuck()
    {
        IsStuck = true;
        SetRigidbodyActive(false);
    }

    private void SetRigidbodyActive(bool value)
    {
        if (value == false)
        {
            HookRigidBody.constraints = RigidbodyConstraints.FreezeAll;
        }
        else
        {
            HookRigidBody.constraints = RigidbodyConstraints.None;
            HookRigidBody.constraints = RigidbodyConstraints.FreezeRotationY;
            HookRigidBody.constraints = RigidbodyConstraints.FreezePositionZ;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other != lastHit && !IsStuck)
        {
            if (!comingBack && other.transform.tag == "Grabable")
            {
                GetStuck();
            }
            else
            {
                Destructible destructible = other.transform.GetComponentInParent<Destructible>();
                if (destructible != null && lastHit != other)
                {
                    destructible.Hurt(baseDamage * Random.Range(1f, 2f));
                }
                else
                {
                    Destroy(gameObject);
                }
            }

            lastHit = other;
        }        
    }
}
