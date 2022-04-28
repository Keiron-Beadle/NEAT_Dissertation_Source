using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetTargetFrameRate : MonoBehaviour
{
    void Start()
    {
        Application.targetFrameRate = 99999;
    }
}
