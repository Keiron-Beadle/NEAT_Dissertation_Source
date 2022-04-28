using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Profiling;


namespace Base_NEAT
{
    public enum RESPONSES
    {
        LEFT,
        RIGHT,
        FORWARD,
        BACKWARD
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
        private GameObject obstacles;

        public int GenerationCount { private set; get; }
        private ProfilerRecorder systemMemoryRecorder;
        private bool _spawnOnlyFittest = false;
        private int _frameCount = 0;
        public int FrameCount
        {
            get { return ++_frameCount; }
        }
        private float _generationTimer = 60.0f; //60 seconds for each generation, if elapsed, kill generation
        private int _lastOutput = 0;
        private List<AIController> NPCs; 
        private float _elapsedTimeInTrial = 0.0f; //Used for UI 
        private List<Phenotype> _networks;
        private List<Phenotype> _nextGeneration;
        private List<Species> _species;
        private Dictionary<Phenotype, Species> _speciesDict;
        private System.Random _random;
        [HideInInspector]
        public Vector3 _spawnPoint = new Vector3(-11.5f, 0.5f, 1.3f);
        private int _deadCount;
        public int DeadCount = 0;
        public float ElapsedTime { get { return _elapsedTimeInTrial; } }
        private Phenotype fittestNetworkPhenotype;
        [HideInInspector]
        public AIController FittestNetwork;
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

        private void WriteFramesToFile()
        {
            using (StreamWriter sWrite = new StreamWriter("frames.txt", true))
            {
                sWrite.WriteLine("{0}", _frameCount);
            }
        }

        private static void RestartApplication()
        {
            //Restart game to take new Results
            //using (StreamWriter s = new StreamWriter("frames.txt", true))
            //{
            //    s.WriteLine("..........");
            //}
            //EditorApplication.ExecuteMenuItem("Edit/Play");
            System.Diagnostics.Process.Start(Application.dataPath.Replace("_Data", ".exe"));
            Application.Quit(0);
        }

        private void WriteMemSizeToFile()
        {
            GC.Collect(); //Required, otherwise build up of dead objects
            GC.WaitForPendingFinalizers(); //Wait for GC to finish before continuing
            using (StreamWriter sWrite = new StreamWriter("neatMemory.txt", true))
            {
                int totalBytes = 0;
                foreach (var pheno in _networks)
                {
                    totalBytes += pheno.GetMemBytes();
                }
                //kilobytes
                sWrite.WriteLine("{0}", totalBytes / 1024);
            }
            using (StreamWriter sWrite = new StreamWriter("usedMemoryTotal.txt", true))
            {
                //megabytes
                sWrite.WriteLine("{0}", systemMemoryRecorder.LastValue / (1024 * 1024));
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

        private void Start()
        {
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
                var newNPC = Instantiate(npcPrefab, _spawnPoint, Quaternion.Euler(0, -3.2f, 0));
                AIController followComponent = newNPC.AddComponent<AIController>();
                followComponent.AIControllerConstructor(_spawnPoint, obstacles.transform);
                followComponent.SetPhenotype(phenotype);
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
                fittestNetworkPhenotype = NPCs[0].GetPhenotype();
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
            //WriteMemSizeToFile();
            GenerationCount++;
            if (GenerationCount == 100)
            {
                SerializeFittestNetworkTxt();
                Application.Quit(0);
            }
            if (!_spawnOnlyFittest)
            {
                //CheckForNoImprovedSpecies();
                AddTopSpeciesGenomesToNextGen();
                BreedNextGen();
                CleanupForNextGen();
            }
            NumOfSpecies = _species.Count;
            _generationTimer = 60.0f;
        }

        private void Update()
        {
            _generationTimer -= Time.deltaTime;
            _elapsedTimeInTrial += Time.deltaTime;
            var flooredElapsed = Math.Floor(_elapsedTimeInTrial);
            //if (flooredElapsed % 15 == 0 && _lastOutput != flooredElapsed)
            //{
            //    _lastOutput = (int)flooredElapsed;
            //    WriteFramesToFile();
            //}
            //if (_elapsedTimeInTrial > 121)
            //{
            //    RestartApplication();
            //}
            if (_generationTimer <= 0)
            {
                DeadCount = int.MaxValue;
                CheckIfGenerationFinished();
                return;
            }
            if (_networks.Count == 0) { return; }
            for (int i = 0; i < _networks.Count; i++)
            {
                if (_networks[i].IsDead) 
                {
                    _deadCount++; 
                    continue; 
                }
                Species currentSpecies = _speciesDict[_networks[i]];
                EvaluateNetwork(i);
                NPCs[i].UpdateBasedOnNetwork();
                if (_networks[i].IsDead) { continue; }
                if (_networks[i].Genome.Fitness > fittestNetworkPhenotype.Genome.Fitness)
                {
                    fittestNetworkPhenotype = _networks[i];
                }
                float adjustedScore = _networks[i].Score / _speciesDict[_networks[i]].Members.Count;
                currentSpecies.AddAdjustedFitness(adjustedScore);
                _networks[i].Genome.Fitness = adjustedScore;
                if (_networks[i].Score > HighestScore)
                {
                    HighestScore = _networks[i].Score;
                    FittestNetwork = NPCs[i];
                }
            }

            if (Input.GetKeyUp(KeyCode.K))
            {
                _spawnOnlyFittest = !_spawnOnlyFittest;
                Debug.Log("Spawn fittest: " + _spawnOnlyFittest);
                _deadCount = _networks.Count; //Invokes a new generation
                SerializeFittestNetworkTxt();
            }

            CheckIfGenerationFinished();
        }

        private void SerializeFittestNetworkTxt()
        {
            using (StreamWriter sWrite = new StreamWriter("Fittest.txt"))
            {
                var nodes = fittestNetworkPhenotype.Genome.Nodes;
                foreach (var node in nodes)
                {
                    sWrite.WriteLine("N," + node.ID + "," + Enum.GetName(typeof(NodeType), node.Type) + ","
                        + node.Value);
                }
                sWrite.WriteLine();
                var connections = fittestNetworkPhenotype.Genome.Connections;
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
            if (_deadCount >= _networks.Count)
            {
                if (!_spawnOnlyFittest)
                    FittestNetwork = null;
                HighestScore = 0.0f;
                PostGeneration();
                PreGeneration();
            }
            _deadCount = 0;
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
            float[] inputData = NPCs[index].GatherInput(); 
            _networks[index].FeedForward(inputData); //Feed data to network
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
