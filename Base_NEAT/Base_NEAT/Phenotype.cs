using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base_NEAT
{
    class Phenotype
    {
        private Genome _genome;

        private NodeGene[] _inputNodes;
        private NodeGene[] _hiddenNodes;
        private NodeGene[] _outputNodes;
        private ConnectionGene[] _connections;

        public Genome Genome { get { return _genome; } }

        public Phenotype(Genome pGenome)
        {
            _genome = pGenome;
            SetupNetwork();
        }

        public void FeedForward(float[] pInputData)
        {
            for (int i = 0; i < _inputNodes.Length; i++)
            {
                _inputNodes[i].Value = pInputData[i];
            }

            //Feed hidden neurons
            List<NodeGene> visitedNodes = new List<NodeGene>();
            for (int i = 0; i < _hiddenNodes.Length; i++)
            {
                visitedNodes.Add(_hiddenNodes[i]);
                FeedHiddenNode(_hiddenNodes[i], visitedNodes);
            }
            
            for (int i = 0; i < _outputNodes.Length; i++)
            {
                foreach (var conn in _connections)
                {
                    if (conn.OutNode == _outputNodes[i].ID)
                    {
                        NodeGene tempNeuron = _genome.FindNodeByID(conn.InNode);
                        _outputNodes[i].AddValue(tempNeuron.Value * conn.Weight);
                    }
                }
                _outputNodes[i].ActivateFunction(); //Use activation function to compress all data
            }
        }

        private void FeedHiddenNode(NodeGene pCurrent, List<NodeGene> pVisitedNodes)
        {
            foreach (var connection in _connections)
            {
                if (connection.OutNode == pCurrent.ID)
                {
                    NodeGene tempNeuron = _genome.FindNodeByID(connection.InNode);
                    if (tempNeuron.Type == NodeType.INPUT || pVisitedNodes.Contains(tempNeuron)) //Base cases
                    {
                        pCurrent.AddValue(tempNeuron.Value * connection.Weight);
                        continue;
                    }
                    else
                    {
                        pVisitedNodes.Add(tempNeuron);
                        FeedHiddenNode(tempNeuron, pVisitedNodes);
                    }
                }
            }
            pCurrent.ActivateFunction(); //Use activation function to compress all data
        }

        /// <summary>
        /// Used to fill connections. & Input, hidden and output nodes. 
        /// </summary>
        private void SetupNetwork()
        {
            List<NodeGene> inputTempList = new List<NodeGene>();
            List<NodeGene> hiddenTempList = new List<NodeGene>();
            List<NodeGene> outputTempList = new List<NodeGene>();
            //Get number of each type of node
            for (int i = 0; i < _genome.Nodes.Count; i++)
            { 
                switch (_genome.Nodes[i].Type)
                {
                    case NodeType.INPUT:
                        inputTempList.Add(_genome.Nodes[i]);
                        continue;
                    case NodeType.HIDDEN:
                        hiddenTempList.Add(_genome.Nodes[i]);
                        continue;
                    case NodeType.OUTPUT:
                        outputTempList.Add(_genome.Nodes[i]);
                        continue;
                }
            }
            _inputNodes = inputTempList.ToArray();
            _hiddenNodes = hiddenTempList.ToArray();
            _outputNodes = outputTempList.ToArray();
            _connections = _genome.Connections.ToArray();
        }
    }
}
