using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnlitOnHover : MonoBehaviour
{
    public const float animDuration = 0.2f;
    public const float boundExtension = 0.16f;

    public MeshRenderer meshRenderer;

    float unlitness = 0f;

    public void Hover()
    {
        unlitness += 2 * Time.deltaTime / animDuration;
    }

    void Update()
    {
        unlitness -= Time.deltaTime / animDuration;
        unlitness = Mathf.Clamp(unlitness, -boundExtension, 1f + boundExtension);
        meshRenderer.material.SetFloat("_Unlitness", Mathf.Clamp01(unlitness));
    }
}
