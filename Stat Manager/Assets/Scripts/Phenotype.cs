using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Threading.Tasks;
using System.IO;

namespace Base_NEAT
{
    class Phenotype
    {
        private Genome _genome;

        private NodeGene[] _inputNodes;
        private NodeGene[] _hiddenNodes;
        private NodeGene[] _outputNodes;
        private ConnectionGene[] _connections;

        public float Score = 0.0f;
        public bool IsDead = false;

        public Genome Genome { get { return _genome; } }

        public RESPONSES[] ActivatedOutputs;

        public Phenotype(Genome pGenome)
        {
            _genome = pGenome;
            ActivatedOutputs = new RESPONSES[1];
            SetupNetwork();
        }

        public Phenotype (Phenotype pPhenotype)
        {
            _genome = new Genome(pPhenotype.Genome);
            _inputNodes = new NodeGene[pPhenotype._inputNodes.Length];
            _hiddenNodes = new NodeGene[pPhenotype._hiddenNodes.Length];
            _outputNodes = new NodeGene[pPhenotype._outputNodes.Length];
            for (int i = 0; i < _inputNodes.Length; i++) { _inputNodes[i] = NodeGene.Copy(pPhenotype._inputNodes[i]); }
            for (int i = 0; i < _hiddenNodes.Length; i++) { _hiddenNodes[i] = NodeGene.Copy(pPhenotype._hiddenNodes[i]); }
            for (int i = 0; i < _outputNodes.Length; i++) { _outputNodes[i] = NodeGene.Copy(pPhenotype._outputNodes[i]); }
            _connections = new ConnectionGene[pPhenotype._connections.Length];
            for (int i = 0; i < _connections.Length; i++) { _connections[i] = ConnectionGene.Copy(pPhenotype._connections[i]); }
            Score = pPhenotype.Score;
            IsDead = pPhenotype.IsDead;
            ActivatedOutputs = new RESPONSES[1];

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

            float maxValue = 0;
            for (int i = 0; i < _outputNodes.Length; i++)
            {
                foreach (var conn in _connections)
                {
                    if (conn.OutNode == _outputNodes[i].ID)
                    {
                        NodeGene tempNeuron = _genome.FindNodeByID(conn.InNode);
                        float value = tempNeuron.Value * conn.Weight;
                        value += NEATConfig.BIAS;
                        _outputNodes[i].AddValue(value);
                    }
                }
                _outputNodes[i].ActivateFunction(); //Use activation function to compress all data
               
                if (_outputNodes[i].Value > maxValue)
                {
                    maxValue = _outputNodes[i].Value;
                    ActivatedOutputs[0] = (RESPONSES)i;
                }
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
                        float value = (tempNeuron.Value + NEATConfig.BIAS) * connection.Weight;
                        pCurrent.AddValue(value);
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
        /// Method used to write a network to a file 
        /// </summary>
        /// <param name="sWrite">Output stream to use</param>
        internal void WriteSelfToFile(StreamWriter sWrite)
        {
            sWrite.WriteLine("Number of Input Nodes: " + _inputNodes.Length);
            sWrite.WriteLine("Number of Hidden Nodes: " + _hiddenNodes.Length);
            sWrite.WriteLine("Number of Output Nodes: " + _outputNodes.Length);
            sWrite.WriteLine();
            sWrite.WriteLine("Number of connections: " + _connections.Length);
            sWrite.WriteLine("Number of nodes: " + _genome.Nodes.Count);
        }

        public int GetMemBytes()
        {
            return 108 + _genome.GetMemBytes()+
                ((_inputNodes.Length + _hiddenNodes.Length + _outputNodes.Length + _connections.Length + ActivatedOutputs.Length) * 4);
        }

        /// <summary>
        /// Used to fill connections. & Input, hidden and output nodes. 
        /// </summary>
        private void SetupNetwork()
        {
            List<NodeGene> inputTempList = new List<NodeGene>();
            List<NodeGene> hiddenTempList = new List<NodeGene>();
            List<NodeGene> outputTempList = new List<NodeGene>();
            ActivatedOutputs = new RESPONSES[1];
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
