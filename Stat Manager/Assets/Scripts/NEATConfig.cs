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
        public const float BIAS = 0.0f;
      
        //Trainer constants
        public const float ACTIVATION_REQ = 0.95f;
        public const float SCORE_PER_FRAME = 0.1f;
        public const float FINISH_SCORE = 1000000.0f;
        public const float SPECIES_IMPROVEMENT_CUTOFF = 20; // no improvement after X generations = cull species
        public const float GOAL_SCORE = 500.0f;
        public const float MAX_FEED_TIME = 0.004f;
        //Mutation constants
        public const float MAX_GENETIC_DIFF_SPECIES = 2.2f;
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
        public const float EXCESS_FACTOR = 3.0f;
        public const float DISJOINT_FACTOR = 2.5f;
        public const float MATCHING_FACTOR = 7.0f;

        public static float[] INCREASE_STATUS = { 12.5f, 24.5f, 19.5f, 13.0f, 5.6f }; 
        public static float[] DECREASE_STATUS = { 3.6f, 10.1f, 6.0f, 4.3f, 1.9f };
      //  public static float[] DECREASE_STATUS = { 2.6f, 36.1f, 43.0f, 40.3f, 36.9f };
    }
}
