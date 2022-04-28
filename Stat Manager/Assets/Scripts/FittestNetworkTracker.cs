using Base_NEAT;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FittestNetworkTracker : MonoBehaviour
{
    [SerializeField]
    private Trainer trainer;
    [SerializeField]
    public Text happinessCounter;
    [SerializeField]
    public Text hungerCounter;
    [SerializeField]
    public Text thirstCounter;
    [SerializeField]
    public Text sleepCounter;
    [SerializeField]
    public Text socialCounter;

    void Update()
    {
        if (trainer.FittestNetwork == null) { return; }
        happinessCounter.text = trainer.FittestNetwork._status[(int)RESPONSES.Happiness].ToString();
        hungerCounter.text = trainer.FittestNetwork._status[(int)RESPONSES.Hunger].ToString();
        thirstCounter.text = trainer.FittestNetwork._status[(int)RESPONSES.Thirst].ToString();
        sleepCounter.text = trainer.FittestNetwork._status[(int)RESPONSES.Sleep].ToString();
        socialCounter.text = trainer.FittestNetwork._status[(int)RESPONSES.Social].ToString();
    }
}
