using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;

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

        public List<RESPONSES> ActivatedOutputs;

        public Phenotype(Genome pGenome)
        {
            _genome = pGenome;
            ActivatedOutputs = new List<RESPONSES>();
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
            ActivatedOutputs = new List<RESPONSES>();

        }

        public void FeedForward(float[] pInputData)
        {
            ActivatedOutputs.Clear();
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
                        float value = tempNeuron.Value * conn.Weight;
                        value += NEATConfig.BIAS;
                        _outputNodes[i].AddValue(value);
                    }
                }
                _outputNodes[i].ActivateFunction(); //Use activation function to compress all data
              
                if (_outputNodes[i].Value > NEATConfig.ACTIVATION_REQ) 
                {
                    ActivatedOutputs.Add((RESPONSES)i);
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
        /// Used to fill connections. & Input, hidden and output nodes. 
        /// </summary>
        private void SetupNetwork()
        {
            List<NodeGene> inputTempList = new List<NodeGene>();
            List<NodeGene> hiddenTempList = new List<NodeGene>();
            List<NodeGene> outputTempList = new List<NodeGene>();
            ActivatedOutputs = new List<RESPONSES>();
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

        public int GetMemBytes()
        {
            return 108 + _genome.GetMemBytes() +
                ((_inputNodes.Length + _hiddenNodes.Length + _outputNodes.Length + _connections.Length + ActivatedOutputs.Count) * 4);
        }
    }
}
