using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnlitOnHover : MonoBehaviour
{
    public const float animDuration = 0.2f;
    public const float boundExtension = 0.16f;

    public MeshRenderer meshRenderer;

    float unlitness = 0f;
    float baseSmoothness;
    float baseSpecularHighlights;

    void Start()
    {
        meshRenderer.material = new Material(meshRenderer.material);
        baseSmoothness = meshRenderer.material.GetFloat("_Smoothness");
        baseSpecularHighlights = meshRenderer.material.GetFloat("_SpecularHighlights");
    }

    void Update()
    {
        unlitness -= Time.deltaTime / animDuration;
        unlitness = Mathf.Clamp(unlitness, -boundExtension, 1f + boundExtension);
        meshRenderer.material.SetColor("_BaseColor", Color.Lerp(Color.white, Color.black, unlitness));
        meshRenderer.material.SetColor("_EmissionColor", Color.Lerp(Color.black, Color.white, unlitness));
        meshRenderer.material.SetFloat("_Smoothness", unlitness > 0.1f ? 0f : baseSmoothness);
        meshRenderer.material.SetFloat("_SpecularHighlights", unlitness > 0.1f ? 0 : baseSpecularHighlights);
    }

    public void Hover()
    {
        unlitness += 2 * Time.deltaTime / animDuration;
    }
}
