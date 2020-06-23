using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Freezer : MonoBehaviour
{
    public static Freezer instance = null;

    public float defaultDuration = 1f;

    bool isFrozen = false;
    float pendingFreezeDuration = 0f;

    PlayerController focusedPlayer;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (pendingFreezeDuration > 0 && !isFrozen)
        {
            StartCoroutine(DoFreeze());
        }
    }

    public void Freeze()
    {
        pendingFreezeDuration = defaultDuration;
    }

    public void Freeze(float duration)
    {
        defaultDuration = duration;
        Freeze();
    }

    public void Freeze(PlayerController player)
    {
        focusedPlayer = player;
        Freeze();
    }

    public void Freeze(PlayerController player, float duration)
    {
        defaultDuration = duration;
        focusedPlayer = player;
        Freeze();
    }

    IEnumerator DoFreeze()
    {
        isFrozen = true;
        float original = Time.timeScale;
        Time.timeScale = 0;
        
        yield return new WaitForSecondsRealtime(defaultDuration);
        
        Time.timeScale = original;
        pendingFreezeDuration = 0f;
        isFrozen = false;
    }
}
