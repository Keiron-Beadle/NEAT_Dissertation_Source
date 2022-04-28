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
    Transform objects;

    //private Vector3 spawnPoint = new Vector3(24, 0, 4);
    private Vector3 spawnPoint = new Vector3(-11.5f, 0.5f, 1.3f);
    private Phenotype phenotype;
    private AIController controller;
    private List<AIController> NPCs;
    private float _elapsedTime = 0.0f;
    private float _lastTimeOutput = 0.0f;
    int framecount = 0;
    // Start is called before the first frame update
    void Start()
    {
        NPCs = new List<AIController>();
        Genome genome = new Genome();
        var lines = networkFile.text.Split(new string[] { "\r\n" }, System.StringSplitOptions.None);
        foreach (var s in lines)
        {
            if (string.IsNullOrEmpty(s))
            {
                continue;
            }
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
            controller.AIControllerConstructor(spawnPoint, objects);
            controller.SetPhenotype(phenotype);
            NPCs.Add(controller);
        }
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
            EvaluateNetwork(i);
            NPCs[i].UpdateBasedOnNetwork();
        }
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
        ConnectionGene cg = new ConnectionGene(inNode, outNode, weight, activated, innovation);
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

        float[] inputData = NPCs[index].GatherInput();
        NPCs[index].GetPhenotype().FeedForward(inputData); //Feed data to network
    }
}
