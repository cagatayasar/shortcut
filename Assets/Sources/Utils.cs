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

    public static float EaseOutQuint(float x)
    {
        return 1 - Mathf.Pow(1f - x, 5f);
    }

    public static Vector3 GetPointOnBezier(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        t = Mathf.Clamp01(t);
        float oneMinusT = 1f - t;
        return
        oneMinusT * oneMinusT * p0 +
        2f * oneMinusT * t * p1 +
        t * t * p2;
    }
}
