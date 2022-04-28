using Base_NEAT;
using System;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Unity.Profiling;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    GameObject _npcPrefab;
    [SerializeField]
    Trainer _trainer; //Used to get population number
    private ProfilerRecorder systemMemoryRecorder;
    float elapsed = 0;

    List<RandController> _randControllers;

    private void OnEnable()
    {
        systemMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "System Used Memory");
    }

    private void OnDisable()
    {
        systemMemoryRecorder.Dispose();
    }

    void Start()
    {
        _randControllers = new List<RandController>();
        for (int i = 0; i < 100; i++)
        {
            var newNPC = Instantiate(_npcPrefab, _trainer.spawnPoint, Quaternion.Euler(0, -3.2f, 0));
            _randControllers.Add(newNPC.AddComponent<RandController>());
        }
    }

    void Update()
    {
        elapsed += Time.deltaTime;
        if (elapsed >= 10.0f)
        {
            WriteMemSizeToFile();
            Application.Quit(0);
        }
    }

    private void WriteMemSizeToFile()
    {
        GC.Collect(); //Required, otherwise build up of dead objects
        GC.WaitForPendingFinalizers(); //Wait for GC to finish before continuing
        using (StreamWriter sWrite = new StreamWriter("usedMemoryTotal.txt", true))
        {
            //megabytes
            sWrite.WriteLine("OUT - Car - {0}", systemMemoryRecorder.LastValue / (1024 * 1024));
        }
    }
}
