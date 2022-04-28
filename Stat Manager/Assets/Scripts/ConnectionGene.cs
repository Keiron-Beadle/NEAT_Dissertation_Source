using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base_NEAT
{
    class ConnectionGene
    {
        private int _inNode;
        private int _outNode;
        private float _weight;
        private bool _activated;
        private int _innovationNumber;

        public int InNode { get { return _inNode; } }
        public int OutNode { get { return _outNode; } }
        public int InnovationNumber { get { return _innovationNumber; } }

        public bool Activated { get { return _activated; }
            set
            {
                _activated = value;
            }
        
        }

        public float Weight {
            get { return _weight; }
            set 
            {
                if (value < 0) return;
                if (value > 1.0f) return;
                _weight = value;
            }
        }

        public ConnectionGene(int pInNode, int pOutNode, float pWeight, bool pActivated)
        {
            _inNode = pInNode;
            _outNode = pOutNode;
            _weight = pWeight;
            _activated = pActivated;

            //Tests if gene already exists if so we get that innovation number
            _innovationNumber = GenePool.ContainsConnection(_inNode, _outNode);
            if (_innovationNumber == -1)
            {
                //If not, we give it a unique innovation number add add to genepool as it's unique gene
                _innovationNumber = InnovationManager.GetConnectionInnovation();
                GenePool.AddConnection(this);
            }
        }

        public static ConnectionGene Copy(ConnectionGene pGene)
        {
            return new ConnectionGene(pGene.InNode, pGene.OutNode, pGene.Weight, pGene.Activated);
        }
    }
}
