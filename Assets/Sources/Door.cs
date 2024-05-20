using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pixelplacement;

public class Door : MonoBehaviour, IInteractable
{
    public enum DoorState
    {
        Closed,
        Open,
        Opening,
        Closing,
    }

    const float openingDurationPer90Deg = 0.5f;
    const float openingDelay = 0.1f;

    public float openAngle = 90f;
    public Transform parentRotator;

    public UnlitOnHover unlitOnHover { get; set; }

    DoorState state = DoorState.Closed;

    void Awake()
    {
        unlitOnHover = GetComponent<UnlitOnHover>();
    }

    void OnAnimEnd()
    {
        if (state == DoorState.Opening) {
            state = DoorState.Open;
        } else if (state == DoorState.Closing) {
            state = DoorState.Closed;
        }
    }

    public void Interact()
    {
        if (state == DoorState.Opening || state == DoorState.Closing)
            return;
        Debug.Log("Interacted with the door.");

        var finalParentRot = Vector3.zero;
        if (state == DoorState.Closed) {
            state = DoorState.Opening;
            finalParentRot = new Vector3(0f, openAngle, 0f);
        } else if (state == DoorState.Open) {
            state = DoorState.Closing;
            finalParentRot = Vector3.zero;
        }

        Tween.LocalRotation(parentRotator, finalParentRot, openAngle * openingDurationPer90Deg / 90f,
            openingDelay, Tween.EaseOut, Tween.LoopType.None, null, OnAnimEnd);
    }
}
