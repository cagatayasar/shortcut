using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[ExecuteInEditMode]
public class Player : MonoBehaviour
{
    public enum VerticalState
    {
        Ground,
        Jump,
    }

    public enum LeanState
    {
        Center,
        LeaningWithInput,
        ReturningToCenter,
    }

    public Camera cam;
    public Transform headTransform;
    public float headRadius;
    public float leanPosDistance;
    public float leanRotAngleMax;
    public float leanSpeed;
    [Min(1f)]
    public float leanEasePower;
    float leanProgress;
    Vector3 headCenterLocalPos;

    [Space(10)]
    public float movementSpeed;
    public float sprintSpeed;
    public float jumpSpeed;
    public float sensitivity;
    const float rotationXMin = -90f;
    const float rotationXMax =  90f;

    VerticalState verticalState = VerticalState.Ground;
    LeanState leanState = LeanState.Center;
    CharacterController cc;
    Vector3 movementDelta;

    void Move(Vector3 movement, bool isSprint)
    {
        var movementMultiplier = isSprint ? sprintSpeed : movementSpeed;
        movementDelta += movement * movementMultiplier * Time.deltaTime;
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

    void Lean(float leanAxis)
    {
        // Update lean state
        if (leanState == LeanState.Center) {
            if (Mathf.Abs(leanAxis) > 0.1f) {
                leanState = LeanState.LeaningWithInput;
            }
        } else if (leanState == LeanState.ReturningToCenter) {
            if (Mathf.Abs(leanAxis) > 0.1f) {
                leanState = LeanState.LeaningWithInput;
                leanProgress = Utils.EaseOutPowerInverted(Utils.EaseInPower(Mathf.Abs(leanProgress), leanEasePower), leanEasePower) * Mathf.Sign(leanProgress);
            }
        } else if (leanState == LeanState.LeaningWithInput) {
            if (Mathf.Abs(leanAxis) < 0.1f) {
                leanState = LeanState.ReturningToCenter;
                leanProgress = Utils.EaseInPowerInverted(Utils.EaseOutPower(Mathf.Abs(leanProgress), leanEasePower), leanEasePower) * Mathf.Sign(leanProgress);
            }
        }

        // Update lean progress & set mapped value
        var leanProgressMapped = 0f;
        if (leanState == LeanState.ReturningToCenter) {
            leanProgress -= Mathf.Sign(leanProgress) * leanSpeed * Time.deltaTime;
            if (Mathf.Abs(leanProgress) <= leanSpeed * Time.deltaTime) {
                leanProgress = 0f;
                leanState = LeanState.Center;
            }
            leanProgressMapped = Utils.EaseInPower(Mathf.Abs(leanProgress), leanEasePower);
        } else if (leanState == LeanState.LeaningWithInput) {
            leanProgress += Mathf.Sign(leanAxis) * leanSpeed * Time.deltaTime;
            if (Mathf.Abs(leanProgress) > 1f) {
                leanProgress = Mathf.Sign(leanProgress) * 1f;
            }
            leanProgressMapped = Utils.EaseOutPower(Mathf.Abs(leanProgress), leanEasePower);
        }

        // Check collision, then set head position, rotation
        var leanSign = Mathf.Sign(leanProgress);
        var didHitWhileLeaning = Physics.SphereCast(transform.position + headCenterLocalPos, headRadius, transform.right * leanSign,
            out var hitInfo, leanProgressMapped * leanPosDistance, ~LayerMask.GetMask("Player"));
        if (didHitWhileLeaning) {
            headTransform.localPosition = headCenterLocalPos + Vector3.right * hitInfo.distance * leanSign;
            var rotAngle = Mathf.Lerp(-leanRotAngleMax, leanRotAngleMax, 0.5f + leanSign * hitInfo.distance / (2f * leanPosDistance));
            headTransform.localRotation = Quaternion.Euler(0f, 0f, -rotAngle);
            if (leanState == LeanState.ReturningToCenter) {
                leanProgress = Mathf.Min(Mathf.Abs(leanProgress), Utils.EaseInPowerInverted(hitInfo.distance / leanPosDistance, leanEasePower)) * leanSign;
            } else if (leanState == LeanState.LeaningWithInput) {
                leanProgress = Mathf.Min(Mathf.Abs(leanProgress), Utils.EaseOutPowerInverted(hitInfo.distance / leanPosDistance, leanEasePower)) * leanSign;
            }
        } else {
            headTransform.localPosition = headCenterLocalPos + Vector3.right * leanPosDistance * leanProgressMapped * leanSign;
            var rotAngle = Mathf.Lerp(-leanRotAngleMax, leanRotAngleMax, 0.5f + leanProgressMapped * leanSign / 2f);
            headTransform.localRotation = Quaternion.Euler(0f, 0f, -rotAngle);
        }
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
        Gizmos.DrawLine(
            transform.position + headCenterLocalPos - transform.right * leanPosDistance,
            transform.position + headCenterLocalPos + transform.right * leanPosDistance
        );

        var leanProgressMapped = Utils.EaseOutPower(Mathf.Abs(leanProgress), leanEasePower);
        var isHit = Physics.SphereCast(transform.position + headCenterLocalPos, headRadius, transform.right * Mathf.Sign(leanProgress),
            out var hitInfo, leanProgressMapped * leanPosDistance, ~LayerMask.GetMask("Player"));
        if (isHit) {
            // Gizmos.DrawCube(transform.position + headCenterLocalPos + transform.right * curveValue * leanPosDistance, Vector3.one * 0.2f);
            Gizmos.DrawCube(hitInfo.point, Vector3.one * 0.2f);
        }
        // Gizmos.DrawCube(transform.position + headCenterLocalPos + headTransform.right * Mathf.Sign(leanProgress) * curveValue * leanPosDistance, Vector3.one * 0.2f);
    }

    void Awake()
    {
        cc = GetComponent<CharacterController>();
    }

    void Start()
    {
        // headCenterLocalPos = headTransform.localPosition;
        leanProgress = 0f;
        headCenterLocalPos = new Vector3(0f, 0.7f, 0f);

        var x = 0.4f;
        var y = Utils.EaseOutPower(x, leanEasePower);
        var xfound = Utils.EaseOutPowerInverted(y, leanEasePower);
        Debug.Log("x: " + x + ", y: " + y + ", x found: " + xfound);
        x = 0.2f;
        y = Utils.EaseOutPower(x, leanEasePower);
        xfound = Utils.EaseOutPowerInverted(y, leanEasePower);
        Debug.Log("x: " + x + ", y: " + y + ", x found: " + xfound);
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

        if (Input.GetKeyDown(KeyCode.Space)) {
            if (verticalState == VerticalState.Ground) {
                StartCoroutine(Jump_Coroutine());
            }
        }
        var isSprint = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        Move(movementVector, isSprint);
        Rotate(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        Lean(Input.GetAxis("Lean"));
        if (verticalState == VerticalState.Ground) {
            movementDelta += Vector3.down * Time.deltaTime;
        }

        cc.Move(movementDelta);
        movementDelta = Vector3.zero;
    }
}
