using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    public static Game inst;

    public bool focusChangedThisFrame;

    //------------------------------------------------------------------------
    void Awake()
    {
        if (inst == null) {
           inst = this;
        } else {
            Debug.LogError("This is a singleton, and there are multiple instances of it.");
            Destroy(gameObject);
            return;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Application.focusChanged += (bool focused) => focusChangedThisFrame = true;
    }

    //------------------------------------------------------------------------
    void LateUpdate()
    {
        focusChangedThisFrame = false;
    }
}
