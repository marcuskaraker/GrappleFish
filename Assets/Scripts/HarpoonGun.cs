using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HarpoonGun : MonoBehaviour
{
    [SerializeField] GameObject harpoonPrefab;
    [SerializeField] float fireForce = 100f;
    [SerializeField] Vector2 fireCooldownInterval = new Vector2(1f, 3f);

    [Space]
    [SerializeField] GameObject harpoonWeaponVisuals;

    float currentFireCooldown;
    float fireTimer;

    bool canFire;

    private void Start()
    {
        currentFireCooldown = Random.Range(fireCooldownInterval.x, fireCooldownInterval.y);
    }

    private void Update()
    {
        fireTimer += Time.deltaTime;
        if (fireTimer > currentFireCooldown)
        {
            canFire = true;          
        }

        if (fireTimer > (currentFireCooldown / 2))
        {
            harpoonWeaponVisuals.SetActive(true);
        }
    }

    public void Fire()
    {
        if (!canFire)
        {
            return;
        }

        GameObject spawnedHarpoon = Instantiate(harpoonPrefab, transform.position, Quaternion.identity);
        spawnedHarpoon.transform.right = transform.right;

        spawnedHarpoon.GetComponent<Rigidbody>().AddForce(spawnedHarpoon.transform.right * fireForce, ForceMode.Impulse);

        fireTimer = 0;
        currentFireCooldown = Random.Range(fireCooldownInterval.x, fireCooldownInterval.y);
        canFire = false;

        harpoonWeaponVisuals.SetActive(false);
    }
}
