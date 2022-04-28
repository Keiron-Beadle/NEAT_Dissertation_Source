using Base_NEAT;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FrameRateUpdater : MonoBehaviour
{
    [SerializeField]
    Trainer _trainer; //used for counter
    Text _frameText;

    void Start()
    {
        _frameText = GetComponent<Text>();
    }

    void Update()
    {
        _frameText.text = _trainer.FrameCount.ToString();
    }
}
