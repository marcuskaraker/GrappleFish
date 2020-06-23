using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float damage = 0.05f;

    Rigidbody rb;
    Collider col;
    TrailRenderer tr;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        tr = GetComponent<TrailRenderer>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            GameController.instance.AddOxygen(-damage);
            GameController.instance.HurtFlash();

            if (Random.Range(0f, 1f) < 0.6f)
            {
                GameController.instance.ShowHitWarning();
            }          

            // Set new parent.
            transform.parent = other.GetComponent<PlayerController>().ProjectileParent;

            // Give a random offset from new parent.
            Vector3 newLocalPos = Random.onUnitSphere * Random.Range(0f, 1f);
            newLocalPos.z = 0;
            transform.localPosition = newLocalPos;

            // Spawn damage floating text.
            FloatingTextManager.instance.SpawnFloatingText(other.transform, "-" + Mathf.RoundToInt(damage * 100) + "%", Color.cyan);
        }

        // Destroy rigidbody and collider component (and trail component after a little while).
        Destroy(rb);
        Destroy(col);
        Destroy(tr, 1.5f);
    }
}
