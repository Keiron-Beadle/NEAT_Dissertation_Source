using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base_NEAT
{
    class NEATConfig
    {
        //Mutation constants
        public const float MAX_GENETIC_DIFF_SPECIES = 3.0f;
        public const float MUTATION_RATE = 0.8f;
        public const float SINGLE_WEIGHT_MUTATE_RATE = 0.9f;
        public const float ALL_WEIGHT_MUTATE_RATE = 0.03f;
        public const float ADD_CONNECTION_RATE = 0.05f;
        public const float ADD_NODE_RATE = 0.03f;
        public const float REMOVE_CONNECTION_RATE = 0.02f;
        public const float REMOVE_NODE_RATE = 0.01f;
        public const int ADD_CONNECTION_ATTEMPTS = 20;
        public const int REMOVE_NODE_ATTEMPTS = 20;

        //Crossover constants
        public const float EXCESS_FACTOR = 1.5f;
        public const float DISJOINT_FACTOR = 1.5f;
        public const float MATCHING_FACTOR = 1.0f;
    }
}
