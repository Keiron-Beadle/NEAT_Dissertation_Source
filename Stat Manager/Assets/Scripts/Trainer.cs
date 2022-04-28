using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Profiling;
using UnityEngine;

namespace Base_NEAT
{
    public enum RESPONSES
    {
        Happiness,
        Hunger,
        Sleep,
        Thirst,
        Social
    }

    class Trainer : MonoBehaviour
    {
        [SerializeField]
        public int PopulationSize = 0;
        [SerializeField]
        private int InitialInputs = 0;
        [SerializeField]
        private int InitialOutputs = 0;
        [SerializeField]
        private GameObject npcPrefab;
        [SerializeField]
        private GameObject placesOfInterest;
        [HideInInspector]
        public Vector2 _spawnPoint = new Vector2(-0.15f, 2.21f);
        public int GenerationCount { private set; get; }
        private bool _spawnOnlyFittest = false;
        private List<AIController> NPCs;
        private ProfilerRecorder systemMemoryRecorder;
        private List<Phenotype> _networks;
        private List<Phenotype> _nextGeneration;
        private List<Species> _species;
        private Dictionary<Phenotype, Species> _speciesDict;
        private System.Random _random;
        [HideInInspector]
        public int DeadCount = 0;
        private int _frameCount = 0;
        public int FrameCount { get { return ++_frameCount; } }
        private int _lastOutput = 0;
        private float _elapsedTimeInTrial = 0.0f; //Used for UI 
        public float ElapsedTime { get { return _elapsedTimeInTrial; } }
        Stopwatch feedForwardTimer;

        [HideInInspector]
        public AIController FittestNetwork;
        private Phenotype fittestPhenotype;
        [HideInInspector]
        public float HighestScore = 0.0f;
        [HideInInspector]
        public int NumOfSpecies;

        public Trainer()
        {
            _networks = new List<Phenotype>();
            _nextGeneration = new List<Phenotype>();
            _species = new List<Species>();
            _speciesDict = new Dictionary<Phenotype, Species>();
            _random = new System.Random();
            NPCs = new List<AIController>();
        }

        private void OnEnable()
        {
            systemMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "System Used Memory");
        }

        private void OnDisable()
        {
            systemMemoryRecorder.Dispose();
        }

        private void Start()
        {
            feedForwardTimer = new Stopwatch();
            Genome fundamentalGenome = new Genome();
            NodeGene[] initialInputs = new NodeGene[InitialInputs];
            NodeGene[] initialOutputs = new NodeGene[InitialOutputs];
            int counter = 0;
            for (int i = 0; i < InitialInputs; i++)
            {
                initialInputs[i] = new NodeGene(counter, NodeType.INPUT, ACTIVATION_FUNCTION.SIGMOID);
                fundamentalGenome.AddNode(initialInputs[i]);
                counter++;
            }
            for (int i = 0; i < InitialOutputs; i++)
            {
                initialOutputs[i] = new NodeGene(counter, NodeType.OUTPUT, ACTIVATION_FUNCTION.SIGMOID);
                fundamentalGenome.AddNode(initialOutputs[i]);
                counter++;
            }
            foreach (var output in initialOutputs)
            {
                foreach (var input in initialInputs)
                {
                    fundamentalGenome.AddConnection(new ConnectionGene(input.ID, output.ID, 0.5f, true));
                }
            }
            for (int i = 0; i < PopulationSize; i++)
            {
                var phenotype = new Phenotype(fundamentalGenome);
                var newNPC = Instantiate(npcPrefab, _spawnPoint, Quaternion.identity);
                AIController followComponent = newNPC.AddComponent<AIController>();
                followComponent.AIControllerConstructor(placesOfInterest.GetComponent<Transform>());
                NPCs.Add(followComponent);
                _networks.Add(phenotype);
            }
            PreGeneration();
        }

        public void PreGeneration()
        {
            if (!_spawnOnlyFittest)
            {
                AssignGenomesToSpecies();
                CheckForDormantSpecies();
                for (int i = 0; i < NPCs.Count; i++)
                {
                    NPCs[i].SetPhenotype(_networks[i]);
                    if (GenerationCount > 0)
                        NPCs[i].ResetTransform();
                }
                return;
            }

            //This will spawn only the fittest network, and many of them
            //Basically measures performance of the Fittest Network thus far
            for (int i  = 0; i < NPCs.Count; i++)
            {
                NPCs[i].SetPhenotype(FittestNetwork.GetPhenotype());
                if (GenerationCount > 0)
                    NPCs[i].ResetTransform();
            }
        }

        public void PostGeneration()
        {
            //Debug.LogError("Species Count: " + _species.Count);
            //WriteMemoryToFile();
            //if (GenerationCount > 50)
            //{
            //    RestartApplication();       
            //}
            GenerationCount++;
            if (GenerationCount >= 100)
            {
                SerializeFittestNetworkTxt();
                Application.Quit(0);
            }
            if (!_spawnOnlyFittest)
            {
                AddTopSpeciesGenomesToNextGen();
                BreedNextGen();
                CleanupForNextGen();
            }
            NumOfSpecies = _species.Count;
        }

        private void WriteMemoryToFile()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            using (StreamWriter s = new StreamWriter("neatMemory.txt", true))
            {
                int totalBytes = 0;
                foreach (var pheno in _networks)
                {
                    totalBytes += pheno.GetMemBytes();
                }
                s.WriteLine("{0}", totalBytes / 1024);
            }
            using (StreamWriter s = new StreamWriter("usedMemoryTotal.txt", true))
            {
                s.WriteLine("{0}", systemMemoryRecorder.LastValue / (1024 * 1024));
            }
        }

        private void WriteFramesToFile()
        {
            using (StreamWriter sWrite = new StreamWriter("frames.txt", true))
            {
                sWrite.WriteLine("{0}", _frameCount);
            }
        }

        private bool CheckForNoImprovedSpecies(in Species s)
        {
            if (s.NoImprovementCounter > NEATConfig.SPECIES_IMPROVEMENT_CUTOFF)
            {
                return true;
            }
            return false;
        }

        private void Update()
        {
            _elapsedTimeInTrial += Time.deltaTime;
            var flooredElapsed = Math.Floor(_elapsedTimeInTrial);
            if (flooredElapsed % 15 == 0 && _lastOutput != flooredElapsed)
            {
                _lastOutput = (int)flooredElapsed;
                WriteFramesToFile();
            }
            if (_elapsedTimeInTrial > 121)
            {
                RestartApplication();
            }

            if (_networks.Count == 0) { return; }
            for (int i = 0; i < _networks.Count; i++)
            {
                if (_networks[i].IsDead)
                {
                    DeadCount++;
                    continue;
                }
                Species currentSpecies = _speciesDict[_networks[i]];
                _networks[i].Score += NEATConfig.SCORE_PER_FRAME;
                EvaluateNetwork(i);
                NPCs[i].UpdateBasedOnNetwork();
                if (_networks[i].IsDead) { continue; }

                float adjustedScore = _networks[i].Score / _speciesDict[_networks[i]].Members.Count;
                currentSpecies.AddAdjustedFitness(adjustedScore);
                _networks[i].Genome.Fitness = adjustedScore;
                if (_networks[i].Score > HighestScore)
                {
                    HighestScore = _networks[i].Score;
                    FittestNetwork = NPCs[i];
                    fittestPhenotype = FittestNetwork.GetPhenotype();
                }
            }
            CheckIfGenerationFinished();
            //This will only spawn the fittest genome for the next generation
            //if (Input.GetKeyUp(KeyCode.K))
            //{
            //    _spawnOnlyFittest = !_spawnOnlyFittest;
            //}
        }

        private void RestartApplication()
        {
            //Restart game to take new Results
            using (StreamWriter s = new StreamWriter("frames.txt", true))
            {
                s.Write("..........");
                s.WriteLine(fittestPhenotype.Genome.Fitness);
            }
            //EditorApplication.ExecuteMenuItem("Edit/Play");
            System.Diagnostics.Process.Start(Application.dataPath.Replace("_Data", ".exe"));
            Application.Quit(0);
        }

        private void SerializeFittestNetworkTxt()
        {
            using (StreamWriter sWrite = new StreamWriter("FittestManager.txt"))
            {
                var nodes = fittestPhenotype.Genome.Nodes;
                foreach (var node in nodes)
                {
                    sWrite.WriteLine("N," + node.ID + "," + Enum.GetName(typeof(NodeType), node.Type) + ","
                        + node.Value);
                }
                sWrite.WriteLine();
                var connections = fittestPhenotype.Genome.Connections;
                foreach (var connection in connections)
                {
                    sWrite.WriteLine("W,{0},{1},{2},{3},{4}", connection.InnovationNumber,
                                connection.InNode, connection.OutNode, connection.Activated,
                                connection.Weight);
                }
            }
        }

        private void CheckIfGenerationFinished()
        {
            if (DeadCount >= _networks.Count)
            {
                //if (!_spawnOnlyFittest)
                //    FittestNetwork = null;
                HighestScore = 0.0f;
                PostGeneration();
                PreGeneration();
            }
            DeadCount = 0;
        }

        private void CleanupForNextGen()
        {
            for (int i = 0; i < _species.Count; i++) { _species[i].ResetSpecies(_random); }
            _speciesDict.Clear();
            _networks = _nextGeneration;
            _nextGeneration = new List<Phenotype>();
        }

        private void BreedNextGen()
        {
            while (_nextGeneration.Count < PopulationSize)
            {
                Species species1 = GetSpeciesWeighted();
                Species species2 = GetSpeciesWeighted();
                Genome parent1 = GetGenomeWeighted(species1);
                Genome parent2 = GetGenomeWeighted(species2);
                Genome child = Genome.Crossover(parent1, parent2, _random);
                child.Mutate(_random);
                _nextGeneration.Add(new Phenotype(child));
            }
        }

        private Genome GetGenomeWeighted(Species pSpecies)
        {
            float sumFitness = 0.0f;
            for (int i = 0; i < pSpecies.Members.Count; i++)
                sumFitness += pSpecies.Members[i].Fitness;
            double roulettePicker = _random.NextDouble() * sumFitness;
            float count = 0.0f;
            for (int i = 0; i < pSpecies.Members.Count; i++)
            {
                count += pSpecies.Members[i].Fitness;
                if (count >= roulettePicker)
                    return pSpecies.Members[i];
            }
            throw new Exception("Error picking random genome by weight, Roulette: " + roulettePicker);
        }

        private Species GetSpeciesWeighted()
        {
            float sumFitness = 0.0f;
            for (int i = 0; i < _species.Count; i++)
                sumFitness += _species[i].AdjustedFitness;
            double rouletterPicker = _random.NextDouble() * sumFitness;
            float count = 0.0f;
            for (int i = 0; i < _species.Count; i++)
            {
                count += _species[i].AdjustedFitness;
                if (count >= rouletterPicker)
                    return _species[i];
            }
            throw new Exception("Error getting random species by roulette: Float:" + rouletterPicker + " Species count: " + _species.Count);
        }

        private void AddTopSpeciesGenomesToNextGen()
        {
            for (int i = 0; i < _species.Count; i++)
            {
                if (_species.Count > 1 && CheckForNoImprovedSpecies(_species[i]))
                {
                    _species.RemoveAt(i);
                    i--;
                    continue;
                }
                var sortedMembers = _species[i].Members.OrderByDescending(entry => entry.Fitness).ToArray();
                _nextGeneration.Add(new Phenotype(sortedMembers[0]));
            }
        }

        private void EvaluateNetwork(int index)
        {
            DecreaseStats(index);
            float[] inputData = NPCs[index].GatherInput(); //Get ordinal distances from NPC
            feedForwardTimer.Start();
            _networks[index].FeedForward(inputData); //Feed data to network
            feedForwardTimer.Stop();
            if (feedForwardTimer.ElapsedMilliseconds > NEATConfig.MAX_FEED_TIME)
            {
                _networks[index].IsDead = true;
                _networks[index].Genome.Fitness = 0;
            }
            feedForwardTimer.Reset();
        }

        private void DecreaseStats(int index)
        {
            NPCs[index]._status[(int)RESPONSES.Happiness] -= NEATConfig.DECREASE_STATUS[(int)RESPONSES.Happiness] * Time.deltaTime;
            NPCs[index]._status[(int)RESPONSES.Hunger] -= NEATConfig.DECREASE_STATUS[(int)RESPONSES.Hunger] * Time.deltaTime;
            NPCs[index]._status[(int)RESPONSES.Thirst] -= NEATConfig.DECREASE_STATUS[(int)RESPONSES.Thirst] * Time.deltaTime;
            NPCs[index]._status[(int)RESPONSES.Sleep] -= NEATConfig.DECREASE_STATUS[(int)RESPONSES.Sleep] * Time.deltaTime;
            NPCs[index]._status[(int)RESPONSES.Social] -= NEATConfig.DECREASE_STATUS[(int)RESPONSES.Social] * Time.deltaTime;
        }

        private void CheckForDormantSpecies()
        {
            for (int i = 0; i < _species.Count; i++)
            {
                if (_species[i].Members.Count == 0)
                {
                    _species.RemoveAt(i);
                    i--;
                }
            }
        }

        private void AssignGenomesToSpecies()
        {
            for (int i = 0; i < _networks.Count; i++)
            {
                bool foundSpecies = false;
                for (int j = 0; j < _species.Count; j++)
                {
                    float geneticComparison = Genome.Compare(_networks[i].Genome, _species[j].Mascot);
                    if (geneticComparison < NEATConfig.MAX_GENETIC_DIFF_SPECIES)
                    {
                        foundSpecies = true;
                        _species[j].Members.Add(_networks[i].Genome);
                        _speciesDict.Add(_networks[i], _species[j]);
                        break;
                    }
                }
                if (!foundSpecies)
                {
                    Species newSpecies = new Species(_networks[i].Genome);
                    _species.Add(newSpecies);
                    _speciesDict.Add(_networks[i], newSpecies);
                }
            }

            //Debug.Log("Num of species: " + _species.Count);
        }
    }
}
