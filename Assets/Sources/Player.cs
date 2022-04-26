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
        Gizmos.color = new Color(0f, 1f, 0.5f, 1f);
        // Gizmos.DrawCube(transform.position, new Vector3(1f, 1f, 1f));


        if (cc != null) {
            if ((cc.collisionFlags & CollisionFlags.Above) != 0) {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(transform.position, 0.5f);
                return;
            }
        }


        var cc_ = GetComponent<CharacterController>();
        Gizmos.DrawCube(transform.position + cc_.center, Vector3.one * 0.15f);
        Gizmos.DrawCube(transform.position + cc_.center - transform.up *  (cc_.height / 2f - cc_.radius), Vector3.one * 0.15f);
        var origin = transform.position + cc_.center;
        var bottom = origin - transform.up * (cc_.height / 2f - cc_.radius);
        var up     = origin + transform.up * (cc_.height / 2f - cc_.radius) + Vector3.up * 0.1f;
        var colliders = Physics.OverlapCapsule(origin, up, cc_.radius, ~LayerMask.GetMask("Player"));
        if (colliders.Length > 0) {
            
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
    }
}
