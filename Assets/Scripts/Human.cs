using UnityEngine;

public class Human : MonoBehaviour
{
    [SerializeField] float speed = 1f;
    [SerializeField] float distanceToTarget = 0.2f;
    [SerializeField] float pollutionAdd = 0.01f;
    [SerializeField] GameObject bucketObject;

    float xDir;

    Transform spawnPoint;
    Transform targetPoint;

    bool holdingBucket;

    Destructible destructible;

    private void Start()
    {
        destructible = GetComponent<Destructible>();
        destructible.OnDeath.AddListener(delegate { GameController.instance.DecreaseSpawnCount(); });
        destructible.OnDeath.AddListener(delegate { GameController.instance.AddKillCount(); });

        destructible.OnHurt.AddListener(delegate { EZCameraShake.CameraShaker.Instance.ShakeOnce(5, 4, 0, 0.2f); });
        destructible.OnHurt.AddListener(delegate { GameController.instance.FreezeFrame(); });

    }

    public void InitHuman(Transform spawnPoint, Transform targetPoint)
    {
        this.spawnPoint = spawnPoint;
        this.targetPoint = targetPoint;
        holdingBucket = true;
    }

    private void Update()
    {
        if (holdingBucket)
        {
            // If holding a bucket, walk towards target point (water ledge).
            Vector3 dirToTarget = (targetPoint.position - transform.position).normalized;
            xDir = Vector3.Dot(dirToTarget, Vector3.right);
            transform.position += new Vector3(xDir * speed, 0, 0) * Time.deltaTime;

            if (Vector3.Distance(transform.position, targetPoint.position) < distanceToTarget)
            {
                // IF arrived at target point, empty bucket
                holdingBucket = !holdingBucket;
                GameController.instance.AddPollution(pollutionAdd);
                bucketObject.SetActive(false);
            }
        }
        else
        {
            // If NOT holding a bucket, walk towards spawn point (bucket pickup).

            Vector3 dirToTarget = (spawnPoint.position - transform.position).normalized;
            xDir = Vector3.Dot(dirToTarget, Vector3.right);
            transform.position += new Vector3(xDir * speed, 0, 0) * Time.deltaTime;

            if (Vector3.Distance(transform.position, spawnPoint.position) < distanceToTarget)
            {
                // If arrived at spawn point, fill bucket
                holdingBucket = !holdingBucket;
                bucketObject.SetActive(true);
            }
        }
    }
}
