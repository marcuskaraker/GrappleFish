using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialLerper : MonoBehaviour
{
    public float lerpSpeed = 7f;

    Renderer renderer;

    Color defaultColor;

    private void Awake()
    {
        renderer = GetComponent<Renderer>();
        defaultColor = renderer.material.color;
    }

    private void Update()
    {
        renderer.material.color = Color.Lerp(renderer.material.color, defaultColor, Time.deltaTime * lerpSpeed);
    }

    public void SetColor(Color color)
    {
        renderer.material.color = color;
    }

    public void Flash()
    {
        renderer.material.color = Color.white;
    }
}
