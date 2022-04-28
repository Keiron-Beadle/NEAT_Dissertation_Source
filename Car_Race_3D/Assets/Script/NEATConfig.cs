using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base_NEAT
{
    class NEATConfig
    {
        //NEAT Constants
        public const float BIAS = 0f;
      
        //Trainer constants
        public const float ACTIVATION_REQ = 0.95f;
        public const float HIT_COST = 80.0f;
        public const float SPECIES_IMPROVEMENT_CUTOFF = 20; // no improvement after X generations = cull species
        public const float GOAL_SCORE = 500.0f;
        public const float DISTANCE_FACTOR = 3.0f;
        public const float SCORE_PER_FRAME = 0.01f;
        //Game constants
        public const float TARGET_DELAY = 1280; //delay between targets changing 
        public const int DEATH_COUNTER = 2400; // Can't hit target within this time, it dies
        //Mutation constants
        public const float MAX_GENETIC_DIFF_SPECIES = 3.0f;
        public const float MUTATION_RATE = 0.85f;
        public const float SINGLE_WEIGHT_MUTATE_RATE = 0.75f;
        public const float ALL_WEIGHT_MUTATE_RATE = 0.05f;
        public const float ADD_CONNECTION_RATE = 0.05f;
        public const float ADD_NODE_RATE = 0.03f;
        //public const float REMOVE_CONNECTION_RATE = 0.02f;
        //public const float REMOVE_NODE_RATE = 0.01f;
        public const int ADD_CONNECTION_ATTEMPTS = 15;
        public const int REMOVE_NODE_ATTEMPTS = 20;

        //Crossover constants
        public const float EXCESS_FACTOR = 1.1f;
        public const float DISJOINT_FACTOR = 1.0f;
        public const float MATCHING_FACTOR = 1.0f;

    }
}
