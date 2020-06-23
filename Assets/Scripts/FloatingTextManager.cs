using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FloatingTextManager : MonoBehaviour
{
    public static FloatingTextManager instance = null;

    [SerializeField] GameObject floatingTextPrefab;

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

    public void SpawnFloatingText(Transform target, string text)
    {
        GameObject spawnedFT = Instantiate(floatingTextPrefab, target.position, Quaternion.identity, target);
        spawnedFT.GetComponentInChildren<Text>().text = text;
    }

    public void SpawnFloatingText(Transform target, string text, Color color)
    {
        GameObject spawnedFT = Instantiate(floatingTextPrefab, target.position, Quaternion.identity, target);

        Text uiText = spawnedFT.GetComponentInChildren<Text>();
        uiText.color = color;
        uiText.text = text;
    }
}
