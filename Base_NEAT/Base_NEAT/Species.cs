using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base_NEAT
{
    class Species
    {
        private Genome _mascot;
        private List<Genome> _members;
        private float _adjustedFitness;

        public Genome Mascot { get { return _mascot; } }
        public List<Genome> Members { get { return _members; } }
        public float AdjustedFitness { get { return _adjustedFitness; } }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pMascot">The mascot of species will be compared to when adding new genome to population</param>
        public Species(Genome pMascot)
        {
            _mascot = pMascot;
            _members = new List<Genome>();
            _members.Add(_mascot);
        }

        public void AddAdjustedFitness(float pFitness)
        {
            _adjustedFitness += pFitness;
        }

        public void ResetSpecies(Random pRandom)
        {
            int newMascot = pRandom.Next(0, _members.Count);
            _mascot = _members[newMascot];
            _members.Clear();
            _adjustedFitness = 0.0f;
        }
    }
}
