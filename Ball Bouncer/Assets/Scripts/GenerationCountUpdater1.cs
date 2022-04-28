using Base_NEAT;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GenerationCountUpdater1 : MonoBehaviour
{
    [SerializeField]
    Trainer trainer;
    Text genCount;
    double counter = 0;

    // Start is called before the first frame update
    void Start()
    {
        genCount = GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        genCount.text = trainer.GenerationCount.ToString();        
    }
}
