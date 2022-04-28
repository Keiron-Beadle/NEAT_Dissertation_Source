using Base_NEAT;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GenerationCountUpdater : MonoBehaviour
{
    [SerializeField]
    Trainer trainer;
    Text genCount;
    double counter = 0;
    void Start()
    {
        genCount = GetComponent<Text>();
        //Time.timeScale = 0.0f;
    }

    void FixedUpdate()
    {
        //genCount.text = counter.ToString();
        //counter++;
        genCount.text = trainer.GenerationCount.ToString();        
    }

    private void Update()
    {
        //genCount.text = counter.ToString();
        //counter++;
    }
}
