using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DontDestroyUI : MonoBehaviour
{
    void Start()
    {
        DontDestroyOnLoad(gameObject); // Make the UI object a "dont destroy on load" object
    }
}
