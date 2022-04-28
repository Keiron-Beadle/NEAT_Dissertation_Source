using Base_NEAT;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LoadAI : MonoBehaviour
{
    [SerializeField]
    private GameObject npcPrefab;
    [SerializeField]
    TextAsset networkFile;
    [SerializeField]
    Transform placesOfInterest;
    private Vector2 spawnPoint = new Vector2(-0.15f, 2.21f);
    //private Vector3 spawnPoint = new Vector3(11.5f, 0.0f, 0.0f);
    private Phenotype phenotype;
    private AIController controller;
    private List<AIController> NPCs;
    private Phenotype fittest;
    int framecount = 0;
    float _elapsedTime = 0, _lastTimeOutput = 0;

    // Start is called before the first frame update
    void Start()
    {
        NPCs = new List<AIController>();
        Genome genome = new Genome();
        var line = networkFile.text.Split(new string[] { "\r\n" }, System.StringSplitOptions.None);
        foreach (var s in line)
        {
            if (string.IsNullOrEmpty(s)) { continue; }
            var entry = s.Split(',');
            if (entry[0] == "N")
            {
                ParseNode(entry, ref genome);
            }
            else
            {
                ParseConnection(entry, ref genome);
            }
        }
        for (int i = 0; i < 100; i++)
        {
            phenotype = new Phenotype(genome);
            var spawn = Instantiate(npcPrefab, spawnPoint, Quaternion.identity);
            controller = spawn.AddComponent<AIController>();
            controller.AIControllerConstructor(placesOfInterest);
            controller.SetPhenotype(phenotype);
            NPCs.Add(controller);
        }
        fittest = NPCs[0].GetPhenotype();
    }

    // Update is called once per frame
    void Update()
    {
        framecount++;
        _elapsedTime += Time.deltaTime;
        var flooredElapsed = Math.Floor(_elapsedTime);
        if (flooredElapsed % 15 == 0 && _lastTimeOutput != flooredElapsed)
        {
            _lastTimeOutput = (int)flooredElapsed;
            WriteFramesToFile();
        }
        if (_elapsedTime >= 121)
        {
            RestartApplication();
        }

        for (int i = 0; i < NPCs.Count; i++)
        {
            if (fittest.Genome.Fitness < NPCs[i].GetPhenotype().Genome.Fitness)
            {
                fittest = NPCs[i].GetPhenotype();
            }
            EvaluateNetwork(i);
            NPCs[i].UpdateBasedOnNetwork();
        }
        //phenotype.Score += NEATConfig.SCORE_PER_FRAME;

    }


    private static void RestartApplication()
    {
        //Restart game to take new Results
        using (StreamWriter s = new StreamWriter("fittestFrames.txt", true))
        {
            s.WriteLine("..........");
        }
        //EditorApplication.ExecuteMenuItem("Edit/Play");
        System.Diagnostics.Process.Start(Application.dataPath.Replace("_Data", ".exe"));
        Application.Quit(0);
    }

    private void WriteFramesToFile()
    {
        using (StreamWriter sWrite = new StreamWriter("fittestFrames.txt", true))
        {
            sWrite.WriteLine("{0}", framecount);
        }
    }

    private void ParseConnection(string[] entry, ref Genome genome)
    {
        int innovation = int.Parse(entry[1]);
        int inNode = int.Parse(entry[2]);
        int outNode = int.Parse(entry[3]);
        bool activated = bool.Parse(entry[4]);
        float weight = float.Parse(entry[5]);
        ConnectionGene cg = new ConnectionGene(inNode, outNode, weight, activated);
        genome.AddConnection(cg);
    }

    private void ParseNode(string[] entry, ref Genome genome)
    {
        int id = int.Parse(entry[1]);
        NodeType type = (NodeType)Enum.Parse(typeof(NodeType), entry[2]);
        NodeGene n = new NodeGene(id, type, ACTIVATION_FUNCTION.SIGMOID);
        n.Value = float.Parse(entry[3]);
        genome.AddNode(n);
    }

    private void EvaluateNetwork(int index) {

        DecreaseStats(index);
        float[] inputData = NPCs[index].GatherInput();
        NPCs[index].GetPhenotype().FeedForward(inputData); //Feed data to network
    }

    private void DecreaseStats(int index)
    {
        NPCs[index]._status[(int)RESPONSES.Happiness] -= NEATConfig.DECREASE_STATUS[(int)RESPONSES.Happiness] * Time.deltaTime;
        NPCs[index]._status[(int)RESPONSES.Hunger] -= NEATConfig.DECREASE_STATUS[(int)RESPONSES.Hunger] * Time.deltaTime;
        NPCs[index]._status[(int)RESPONSES.Thirst] -= NEATConfig.DECREASE_STATUS[(int)RESPONSES.Thirst] * Time.deltaTime;
        NPCs[index]._status[(int)RESPONSES.Sleep] -= NEATConfig.DECREASE_STATUS[(int)RESPONSES.Sleep] * Time.deltaTime;
        NPCs[index]._status[(int)RESPONSES.Social] -= NEATConfig.DECREASE_STATUS[(int)RESPONSES.Social] * Time.deltaTime;
    }
}
