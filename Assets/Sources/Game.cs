using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    public static Game instance;

    void Awake()
    {
        if (instance == null) {
           instance = this;
        } else {
            Debug.LogError("This is a singleton, and there are multiple instances of it.");
            Destroy(gameObject);
            return;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
