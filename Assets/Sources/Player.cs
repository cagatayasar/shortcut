using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class Player : MonoBehaviour
{
    public enum VerticalState
    {
        Ground,
        Jump,
    }

    public Camera cam;
    public Transform headTransform;
    public float headRadius;
    public Vector2 leanPosScale;
    public float leanPosAngleMax;
    public float leanPosRadius;
    public float leanRotAngleMax;
    public float leanSpeed;
    public AnimationCurve leanCurve;
    float leanProgress;
    Vector3 leanPos0, leanPos1, leanPos2;

    [Space(10)]
    public float movementSpeed;
    public float sprintSpeed;
    public float jumpSpeed;
    public float sensitivity;
    const float rotationXMin = -90f;
    const float rotationXMax =  90f;

    VerticalState verticalState = VerticalState.Ground;
    CharacterController cc;
    Vector3 movementDelta;
    bool isGrounded;

    static Vector3 GetScaledLeanPos(Vector3 pos, Vector3 headPos, Vector2 scale) {
        var offset = (pos - headPos);
        offset = (new Vector3(offset.x, 0f, offset.z)) * scale.x + Vector3.up * offset.y * scale.y;
        return headPos + offset;
    }

    void Rotate(float leftRightDelta, float upDownDelta)
    {
        var newEulers = new Vector3(
            cam.transform.localEulerAngles.x - upDownDelta    * sensitivity,
            transform.localEulerAngles.y     + leftRightDelta * sensitivity,
            cam.transform.localEulerAngles.z
        );
        // Mathf.Clamp does not work here, instead below is done
        float range = (rotationXMax - rotationXMin) / 2f;
        float offset = rotationXMax - range;
        newEulers.x = ((newEulers.x + 540f) % 360f) - 180f - offset;
        if (Mathf.Abs(newEulers.x) > range) {
            newEulers.x = range * Mathf.Sign(newEulers.x) + offset;
        }

        var playerEulers = new Vector3(0f, newEulers.y, 0f);
        var camEulers = new Vector3(newEulers.x, 0f, newEulers.z);
        transform.localRotation = Quaternion.Euler(playerEulers);
        cam.transform.localRotation = Quaternion.Euler(camEulers);
    }

    IEnumerator Jump_Coroutine()
    {
        if (verticalState != VerticalState.Ground)
            yield break;

        verticalState = VerticalState.Jump;
        var verticalSpeed = jumpSpeed;
        do {
            movementDelta += Vector3.up * verticalSpeed * Time.deltaTime;
            verticalSpeed -= 9.81f * Time.deltaTime;

            if ((cc.collisionFlags & CollisionFlags.Above) != 0 && verticalSpeed > 0f) {
                verticalSpeed = 0f;
            }
            yield return null;
        }
        while (!cc.isGrounded);
        verticalState = VerticalState.Ground;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
        Gizmos.DrawCube(transform.position, Vector3.one * 0.5f);
        Gizmos.color = new Color(0f, 0f, 1f, 0.3f);
        Gizmos.DrawSphere(headTransform.position, headRadius);

        if (cc != null) {
            if ((cc.collisionFlags & CollisionFlags.Above) != 0) {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(transform.position, 0.5f);
                return;
            }
        }

        Gizmos.color = Color.red;
        // leanPos0 = headTransform.position + transform.rotation * new Vector3(-headLeanPositionBounds.x, -headLeanPositionBounds.y, 0f);
        // leanPos1 = headTransform.position;
        // leanPos2 = headTransform.position + transform.rotation * new Vector3( headLeanPositionBounds.x, -headLeanPositionBounds.y, 0f);

        int editorPrecision = 20;
        for (int i = 0; i < editorPrecision; i++) {
            var center = transform.position;
            var angleStart = Mathf.Lerp(-leanPosAngleMax, leanPosAngleMax, (float) i / editorPrecision);
            var angleEnd = Mathf.Lerp(-leanPosAngleMax, leanPosAngleMax, (float) (i+1) / editorPrecision);

            var p1 = center + Quaternion.AngleAxis(angleStart, -transform.forward) * Vector3.up * leanPosRadius;
            var p2 = center + Quaternion.AngleAxis(angleEnd,   -transform.forward) * Vector3.up * leanPosRadius;

            Gizmos.DrawLine(
                GetScaledLeanPos(p1, center + leanPosRadius * transform.up, leanPosScale),
                GetScaledLeanPos(p2, center + leanPosRadius * transform.up, leanPosScale)
                // center + Quaternion.AngleAxis(angleStart, -transform.forward) * Vector3.up * headLeanRadius,
                // center + Quaternion.AngleAxis(angleEnd,   -transform.forward) * Vector3.up * headLeanRadius
            );
        }
    }

    void Awake()
    {
        cc = GetComponent<CharacterController>();
    }

    void Update()
    {
        var movementVector = Vector3.zero;
        if (Input.GetKey(KeyCode.W))
            movementVector += Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0) * Vector3.forward;
        if (Input.GetKey(KeyCode.S))
            movementVector += Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0) * Vector3.back;
        if (Input.GetKey(KeyCode.A))
            movementVector += Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0) * Vector3.left;
        if (Input.GetKey(KeyCode.D))
            movementVector += Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0) * Vector3.right;
        var movementMultiplier = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : movementSpeed;
        movementDelta += movementVector * movementMultiplier * Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Space)) {
            if (verticalState == VerticalState.Ground) {
                StartCoroutine(Jump_Coroutine());
            }
        }
        if (verticalState == VerticalState.Ground) {
            movementDelta += Vector3.down * Time.deltaTime;
        }
        cc.Move(movementDelta);
        movementDelta = Vector3.zero;

        Rotate(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

        
        //  ( Input.GetAxis("Lean")) * Time.deltaTime
        if (Mathf.Abs(Input.GetAxis("Lean")) > 0.1f) {
            leanProgress += Input.GetAxis("Lean") * leanSpeed * Time.deltaTime;
            if (leanProgress >  1f) {
                leanProgress =  1f;
            } else if (leanProgress < -1f) {
                leanProgress = -1f;
            }
        } else if (Mathf.Abs(leanProgress) > leanSpeed * Time.deltaTime) {
            leanProgress -= Mathf.Sign(leanProgress) * leanSpeed * Time.deltaTime;
            if (Mathf.Abs(leanProgress) <= leanSpeed * Time.deltaTime)
                leanProgress = 0f;
        }

        // Physics.SphereCast(headTransform.transform.position, headRadius, headTransform.right * Mathf.Sign(leanProgress), out var hitInfo, 1f, )

        var curveValue = leanCurve.Evaluate(Mathf.Abs(leanProgress)) * Mathf.Sign(leanProgress);
        var center = transform.position;
        var angle = Mathf.Lerp(-leanPosAngleMax, leanPosAngleMax, 0.5f + curveValue / 2f);
        var p = center + Quaternion.AngleAxis(angle, -transform.forward) * Vector3.up * leanPosRadius;
        headTransform.position = GetScaledLeanPos(p, center + leanPosRadius * transform.up, leanPosScale);
        // var zAngle = -Input.GetAxis("Lean") * Vector3.Angle(headTransform.position - transform.position, transform.up);
        var rotAngle = Mathf.Lerp(-leanRotAngleMax, leanRotAngleMax, 0.5f + curveValue / 2f);
        headTransform.localRotation = Quaternion.Euler(0f, 0f, -rotAngle);
    }

    bool IsLeanAllowed(float leanProgress) {
        var curveValue = leanCurve.Evaluate(Mathf.Abs(leanProgress)) * Mathf.Sign(leanProgress);
        var center = transform.position;
        var angle = Mathf.Lerp(-leanPosAngleMax, leanPosAngleMax, 0.5f + curveValue / 2f);
        var p = center + Quaternion.AngleAxis(angle, -transform.forward) * Vector3.up * leanPosRadius;
        var projectedPosition = GetScaledLeanPos(p, center + leanPosRadius * transform.up, leanPosScale);
        var nonPlayerMask = ~(LayerMask.GetMask("Player"));
        var colliders = Physics.OverlapSphere(projectedPosition, headRadius, nonPlayerMask);
        return colliders.Length == 0;
    }
}
