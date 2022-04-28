using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base_NEAT
{
    class Trainer
    {
        private int PopulationSize = 0;
        private List<Phenotype> _networks;
        private List<Phenotype> _nextGeneration;
        private List<Species> _species;
        private Dictionary<Phenotype, Species> _speciesDict;
        private Random random;

        public Phenotype FittestNetwork;
        public float HighestScore = 0.0f;
        public int NumOfSpecies;

        public Trainer(int pPopulationSize, Genome pFundamentalGenome)
        {
            PopulationSize = pPopulationSize;
            _networks = new List<Phenotype>();
            _nextGeneration = new List<Phenotype>();
            _species = new List<Species>();
            _speciesDict = new Dictionary<Phenotype, Species>();
            random = new Random();
            for (int i = 0; i < PopulationSize; i++)
            {
                _networks.Add(new Phenotype(pFundamentalGenome));
            }
        }

        public void Evaluate()
        {
            AssignGenomesToSpecies();
            CheckForDormantSpecies();
            RunSimulation();
            AddTopSpeciesGenomesToNextGen();
            BreedNextGen();
            CleanupForNextGen();
            NumOfSpecies = _species.Count;
        }

        private void RunSimulation()
        {
            for (int i = 0; i < _networks.Count; i++)
            {
                Species currentSpecies = _speciesDict[_networks[i]];
                float score = EvaluateNetwork(_networks[i]);
                float adjustedScore = score / _speciesDict[_networks[i]].Members.Count;
                currentSpecies.AddAdjustedFitness(adjustedScore);
                _networks[i].Genome.Fitness = adjustedScore;
                if (score > HighestScore)
                {
                    HighestScore = score;
                    FittestNetwork = _networks[i];
                }
            }
        }

        private void CleanupForNextGen()
        {
            for (int i = 0; i < _species.Count; i++) { _species[i].ResetSpecies(random); }
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
                Genome child = Genome.Crossover(parent1, parent2, random);
                child.Mutate(random);
                _nextGeneration.Add(new Phenotype(child));
            }
        }

        private Genome GetGenomeWeighted(Species pSpecies)
        {
            float sumFitness = 0.0f;
            for (int i = 0; i < pSpecies.Members.Count; i++)
                sumFitness += pSpecies.Members[i].Fitness;
            double roulettePicker = random.NextDouble() * sumFitness;
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
            double rouletterPicker = random.NextDouble() * sumFitness;
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
                var sortedMembers = _species[i].Members.OrderByDescending(entry => entry.Fitness).ToArray();
                _nextGeneration.Add(new Phenotype(sortedMembers[0]));
            }
        }

        private float EvaluateNetwork(Phenotype pNetwork)
        { //Trial test is to reward high node count networks
            return pNetwork.Genome.Connections.Count;
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
        }
    }
}
