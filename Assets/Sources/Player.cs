using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class Player : MonoBehaviour
{
    public float movementSpeed;
    public float sprintSpeed;
    public float sensitivity;
    CharacterController controller;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        float leftRightDelta = Input.GetAxis("Mouse X");
        float upDownDelta = Input.GetAxis("Mouse Y");
        var newEulers = transform.rotation.eulerAngles + new Vector3(-upDownDelta, leftRightDelta, 0) * sensitivity;
        transform.rotation = Quaternion.Euler(newEulers);

        float movementMultiplier = movementSpeed;
        if (Input.GetKey(KeyCode.LeftShift))
            movementMultiplier = sprintSpeed;

        var movementVector = Vector3.zero;
        if (Input.GetKey(KeyCode.W))
            movementVector += Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0) * Vector3.forward;
        if (Input.GetKey(KeyCode.S))
            movementVector += Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0) * Vector3.back;
        if (Input.GetKey(KeyCode.A))
            movementVector += Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0) * Vector3.left;
        if (Input.GetKey(KeyCode.D))
            movementVector += Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0) * Vector3.right;

        controller.Move(movementVector * Time.deltaTime * movementMultiplier);
    }
}
