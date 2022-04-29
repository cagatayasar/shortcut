using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class Utils
{
    public static bool isNull(this System.Object obj) => obj == null || obj.Equals(null);

    public static bool isNotNull(this System.Object obj) => !obj.isNull();

    public static bool isInBetween(this float num, float boundary1, float boundary2) {
        return (num >= boundary1 && num <= boundary2) || (num >= boundary2 && num <= boundary1);
    }

    public static void reset(this LineRenderer renderer) {
        renderer.positionCount = 2;
        renderer.SetPosition(0, Vector3.zero);
        renderer.SetPosition(1, Vector3.zero);
    }

    public static void setPoints(this LineRenderer renderer, Vector3[] points)
    {
        renderer.positionCount = points.Length;
        renderer.SetPositions(points);
    }

    // About 30 times faster than Enumerable.Contains()
    public static bool contains<T>(this T[] array, T value) where T : class
    {
        for (int i = 0; i < array.Length; i++) {
            if (array[i] == value) {
                return true;
            }
        }
        return false;
    }

    public static void GizmosDrawCube(Vector3 position, Quaternion rotation, Vector3 scale)
    {
        Matrix4x4 cubeTransform = Matrix4x4.TRS(position, rotation, scale);
        Matrix4x4 oldGizmosMatrix = Gizmos.matrix;

        Gizmos.matrix *= cubeTransform;

        Gizmos.DrawCube(Vector3.zero, Vector3.one);

        Gizmos.matrix = oldGizmosMatrix;
    }

    public static float EaseOutPower(float x, float p)
    {
        x = Mathf.Clamp01(x);
        return 1f - Mathf.Pow(1f - x, p);
    }

    public static float EaseOutPowerInverted(float x, float p)
    {
        x = Mathf.Clamp01(x);
        return 1f - Mathf.Pow(1f - x, 1f / p);
    }

    public static float EaseInPower(float x, float p)
    {
        x = Mathf.Clamp01(x);
        return Mathf.Pow(x, p);
    }

    public static float EaseInPowerInverted(float x, float p)
    {
        x = Mathf.Clamp01(x);
        return Mathf.Pow(x, 1f / p);
    }
}
