using Base_NEAT;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using Unity.Profiling;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    GameObject _npcPrefab;
    [SerializeField]
    Trainer _trainer;
    [SerializeField]
    GameObject _placesOfInterest;
    private ProfilerRecorder systemMemoryRecorder;
    float elapsed = 0;
    public int Population = 0;

    List<RandController> _randControllers;

    private void OnEnable()
    {
        systemMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "System Used Memory");
    }

    private void OnDisable()
    {
        systemMemoryRecorder.Dispose();
    }

    // Start is called before the first frame update
    void Start()
    {
        _randControllers = new List<RandController>();
        for (int i = 0; i < Population; i++)
        {
            var newNpc = Instantiate(_npcPrefab, _trainer._spawnPoint, Quaternion.identity);
            _randControllers.Add(newNpc.AddComponent<RandController>());
            _randControllers[_randControllers.Count - 1].RandControllerConstructor(_placesOfInterest.transform);
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
            sWrite.WriteLine("OUT - Stat - {0}", systemMemoryRecorder.LastValue / (1024 * 1024));
        }
    }
}
