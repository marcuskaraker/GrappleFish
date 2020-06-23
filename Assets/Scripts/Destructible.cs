using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class DestructibleData
{
    public bool isDestructible = true;
    public bool destroyOnDeath = true;
    public float maxHealth = 100f;
    public float health = 100f;
    public float protection = 0f;
    public int factionID;
    public bool isAlive = true;

    public DestructibleData()
    {

    }
}

public class Destructible : MonoBehaviour
{
    [SerializeField]
    DestructibleData destructibleData;
    public DestructibleData DestructibleData { get { return destructibleData; } }

    public bool useFloatingText = true;

    public UnityEvent OnHurt;
    public UnityEvent OnProtect;
    public UnityEvent OnDeath;

    Vector2 lastHitDir = Vector2.right;

    public Vector2 LastHitDir { get { return lastHitDir; } }

    private void OnValidate()
    {
        if (destructibleData.health > destructibleData.maxHealth)
        {
            destructibleData.health = destructibleData.maxHealth;
        }
        else if (destructibleData.health < 0)
        {
            destructibleData.health = 0;
        }

        if (destructibleData.maxHealth < 0)
        {
            destructibleData.maxHealth = 0;
        }
    }

    public bool Hurt(float damage)
    {
        if (destructibleData.isDestructible)
        {
            bool protection = Random.Range(0f, 1f) < destructibleData.protection;
            if (!protection)
            {
                OnHurt.Invoke();

                if (destructibleData.health - damage > 0)
                {
                    destructibleData.health -= damage;
                }
                else
                {
                    destructibleData.health = 0;
                    Die();
                }

                if (useFloatingText)
                {
                    FloatingTextManager.instance.SpawnFloatingText(transform, Mathf.RoundToInt(damage).ToString(), Color.red);
                }

                return true;
            }
            else
            {
                if (useFloatingText)
                {
                    FloatingTextManager.instance.SpawnFloatingText(transform, Mathf.RoundToInt(damage).ToString(), Color.cyan);
                }

                OnProtect.Invoke();
            }
        }

        return false;
    }

    public bool Hurt(float damage, Vector2 dir)
    {
        lastHitDir = dir.normalized;
        return Hurt(damage);
    }

    public void Heal(float value)
    {
        if (destructibleData.health + value < destructibleData.maxHealth)
        {
            destructibleData.health += value;
        }
        else
        {
            destructibleData.health = destructibleData.maxHealth;
        }

        if (destructibleData.health > 0)
        {
            destructibleData.isAlive = true;
        }
    }

    protected virtual void Die()
    {
        OnDeath.Invoke();

        destructibleData.isAlive = false;

        if (useFloatingText)
        {
            FloatingTextManager.instance.SpawnFloatingText(transform, "+1", new Color(0.45f, 1f, 0.52f, 1f));
        }

        if (destructibleData.destroyOnDeath)
        {            
            Destroy(gameObject);
        }
    }

    public bool IsEnemy(int factionID)
    {
        return (factionID != destructibleData.factionID) 
            && (factionID != 0) 
            && (destructibleData.factionID != 0);
    }

    public void ApplyData(DestructibleData data)
    {
        destructibleData = data;
    }
}
