using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanHarpooner : MonoBehaviour
{
    Destructible destructible;
    HarpoonGun harpoonGun;

    PlayerController player;

    private void Start()
    {
        destructible = GetComponent<Destructible>();
        destructible.OnDeath.AddListener(delegate { GameController.instance.DecreaseSpawnCount(); });
        destructible.OnDeath.AddListener(delegate { GameController.instance.AddKillCount(); });

        destructible.OnHurt.AddListener(delegate { EZCameraShake.CameraShaker.Instance.ShakeOnce(5, 4, 0, 0.2f); });
        destructible.OnHurt.AddListener(delegate { GameController.instance.FreezeFrame(); });

        harpoonGun = GetComponentInChildren<HarpoonGun>();

        player = FindObjectOfType<PlayerController>();
    }

    private void Update()
    {
        // Instant rotate towards the player.
        Vector3 playerDir = (player.transform.position - transform.position).normalized;
        harpoonGun.transform.right = playerDir;

        harpoonGun.Fire();
    }
}
