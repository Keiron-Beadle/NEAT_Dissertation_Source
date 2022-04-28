using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base_NEAT
{
    class Genome
    {
        private List<NodeGene> _nodes;
        private List<ConnectionGene> _connections;
        private float _fitness;

        public List<NodeGene> Nodes { get { return _nodes; } }
        public List<ConnectionGene> Connections { get { return _connections; } }
        public float Fitness { get { return _fitness; } set { _fitness = value; } }

        public Genome()
        {
            _nodes = new List<NodeGene>();
            _connections = new List<ConnectionGene>();
            _fitness = 0.0f;
        }

        public Genome(Genome pCopy)
        {
            _nodes = new List<NodeGene>();
            _connections = new List<ConnectionGene>();
            _fitness = 0.0f;
            for (int i = 0; i < pCopy._nodes.Count; i++)
            {
                _nodes.Add(new NodeGene(pCopy._nodes[i].ID,
                                        pCopy._nodes[i].Type,
                                        pCopy._nodes[i].ActivationFunction));
            }

            for (int i = 0; i < pCopy._connections.Count; i++)
            {
                _connections.Add(new ConnectionGene(pCopy._connections[i].InNode,
                                                    pCopy._connections[i].OutNode,
                                                    pCopy._connections[i].Weight,
                                                    pCopy._connections[i].Activated));
            }
        }

        public int GetMemBytes()
        {
            //   base  +   each node and connection in genome          +  references inside of Lists
            return 60 + (_nodes.Count * 24) + (_connections.Count * 28) + ((_nodes.Count + _connections.Count) * 4);
        }

        public void AddNode(NodeGene pNode)
        {
            _nodes.Add(pNode);
        }

        public void AddConnection(ConnectionGene pConnection)
        {
            _connections.Add(pConnection);
        }

        public static Genome Crossover(Genome parent1, Genome pParent2, Random pRandom)
        {
            Genome child = new Genome();
            Genome mostFit = parent1._fitness > pParent2._fitness ? parent1 : pParent2;
            Genome leastFit = parent1._fitness > pParent2._fitness ? pParent2 : parent1;
            bool sameFitness = parent1._fitness == pParent2._fitness;
            AddNodesToChild(child, leastFit, mostFit, sameFitness);
            AddConnectionsToChild(child, leastFit, mostFit, pRandom, sameFitness);
            return child;
        }

        private static void AddConnectionsToChild(Genome pChild, Genome pLeastFit, Genome pMostFit, Random pRandom, bool pSameFitness)
        {
            int mostPointer = 0, leastPointer = 0;
            while (true)
            {
                ConnectionGene conn1 = pMostFit._connections[mostPointer];
                ConnectionGene conn2 = pLeastFit._connections[leastPointer];

                if (conn2.InnovationNumber < conn1.InnovationNumber)
                {
                    if (pSameFitness) //We're adding disjoint genes if same fitness
                    {
                        pChild.AddConnection(ConnectionGene.Copy(conn2));
                        leastPointer++;
                    }
                    else //We don't if not same fitness
                    {
                        leastPointer++;
                    }
                }
                else if (conn1.InnovationNumber == conn2.InnovationNumber)
                {
                    //Inherit randomly if both matching genes
                    int randInt = pRandom.Next(0, 2);
                    ConnectionGene successorGene = randInt == 1 ? conn1 : conn2;
                    pChild.AddConnection(ConnectionGene.Copy(successorGene));
                    mostPointer++;
                    leastPointer++;
                }
                else
                {
                    //Conn1 is disjoint, we're adding fittest no matter what
                    mostPointer++;
                    pChild.AddConnection(ConnectionGene.Copy(conn1));
                }

                //Check if same fitness and reached end of one parent, means we add all excess genes from other parent
                //since they're same fitness
                if (pSameFitness)
                {
                    if (pLeastFit._connections.Count <= leastPointer)
                    {
                        for (; mostPointer < pMostFit._connections.Count; mostPointer++)
                        {
                            if (!pChild.Connections.Contains(pMostFit._connections[mostPointer]))
                                pChild.AddConnection(ConnectionGene.Copy(pMostFit._connections[mostPointer]));
                        }
                        return;
                    }
                    else if (pMostFit._connections.Count <= mostPointer)
                    {
                        for (; leastPointer < pLeastFit._connections.Count; leastPointer ++)
                        {
                            if (!pChild.Connections.Contains(pLeastFit._connections[leastPointer]))
                                pChild.AddConnection(ConnectionGene.Copy(pLeastFit._connections[leastPointer]));
                        }
                        return;
                    }
                }
                //Check if we reached the end of least fit parent, this means we add all excess genes from most fit
                if (pLeastFit._connections.Count <= leastPointer)
                {
                    for (;mostPointer < pMostFit._connections.Count; mostPointer++)
                    {
                        if (!pChild.Connections.Contains(pMostFit._connections[mostPointer]))
                            pChild.AddConnection(ConnectionGene.Copy(pMostFit._connections[mostPointer]));
                    }
                    return;
                }
                if (pMostFit._connections.Count <= mostPointer)
                    return; //Not same fitness, and added all fitter parent connections
            }
        }

        private static void AddNodesToChild(Genome pChild, Genome pLeastFit, Genome pMostFit, bool pSameFitness)
        {
            //If same fitness, add both nodes from both parents
            //else only add nodes from fittest parent and leave the improvements to mutations
            foreach (var node in pMostFit._nodes)
            {
                pChild.AddNode(NodeGene.Copy(node)); //Add deep copy
            }

            if (!pSameFitness) return; //We're done, no need to add other parent nodes

            foreach (var node in pLeastFit._nodes)
            {
                if (pChild.ContainsNode(node)) { continue; }
                pChild.AddNode(NodeGene.Copy(node));
            }
        }

        public void Mutate(Random pRandom)
        {
            float doMutate = (float)pRandom.NextDouble();
            if (doMutate < NEATConfig.MUTATION_RATE)
            {
                float doWeightMutate = (float)pRandom.NextDouble();
                float doSingleWeightMutate = (float)pRandom.NextDouble();
                float doAddConnection = (float)pRandom.NextDouble();
                float doAddNode = (float)pRandom.NextDouble();
                if (doWeightMutate < NEATConfig.ALL_WEIGHT_MUTATE_RATE)
                {
                    WeightMutation(pRandom);
                }
                if (doSingleWeightMutate < NEATConfig.SINGLE_WEIGHT_MUTATE_RATE)
                {
                    SingleWeightMutate(pRandom);
                }
                if (doAddConnection < NEATConfig.ADD_CONNECTION_RATE)
                {
                    AddConnectionMutation(pRandom);
                }
                if (doAddNode < NEATConfig.ADD_NODE_RATE)
                {
                    AddNodeMutation(pRandom);
                }
            }
        }

        private void SingleWeightMutate(Random pRandom)
        {
            int index = pRandom.Next(0, _connections.Count);
            _connections[index].Weight = (float)pRandom.NextDouble();
        }

        private void WeightMutation(Random pRandom)
        {
            for (int i = 0; i < _connections.Count; i++)
            {
                _connections[i].Weight = (float)pRandom.NextDouble();
            }
        }

        private void AddConnectionMutation(Random pRandom)
        {
            for (int i = 0; i < NEATConfig.ADD_CONNECTION_ATTEMPTS; i++)
            {
                bool needsReverse = false; //This bool will be true if the random node1 -> node2 needs a reverse connection
                NodeGene node1 = _nodes[pRandom.Next(0,_nodes.Count)];
                NodeGene node2 = _nodes[pRandom.Next(0, _nodes.Count)];
                
                //Check if we already have this connection in our connection list
                foreach (var conn in _connections)
                {
                    if (node1.ID == conn.InNode && node2.ID == conn.OutNode ||
                        node2.ID == conn.InNode && node1.ID == conn.OutNode)
                    {
                        continue;
                    }
                }

                //Check if both nodes are input or equal nodes, if so, we need to retry
                if (node1 == node2 || node1.Type == NodeType.INPUT && node2.Type == NodeType.INPUT) { continue; }
                if (node1.Type == NodeType.HIDDEN && node2.Type == NodeType.INPUT ||
                    node1.Type == NodeType.OUTPUT && node2.Type == NodeType.HIDDEN ||
                    node1.Type == NodeType.OUTPUT && node2.Type== NodeType.INPUT)
                {
                    needsReverse = true;
                }

                if (needsReverse)
                {
                    NodeGene temp = node1;
                    node1 = node2;
                    node2 = temp;
                }

                float randomWeight = (float)pRandom.NextDouble();
                ConnectionGene connection = new ConnectionGene(node1.ID, node2.ID, randomWeight, true);
                _connections.Add(connection);
                return;
            }
        }

        private void AddNodeMutation(Random pRandom)
        {
            ConnectionGene existingConnection = _connections[pRandom.Next(0, _connections.Count)];
            NodeGene inNode = FindNodeByID(existingConnection.InNode);
            NodeGene outNode = FindNodeByID(existingConnection.OutNode);
            existingConnection.Activated = false;
            NodeGene temp = new NodeGene(_nodes.Count, NodeType.HIDDEN, ACTIVATION_FUNCTION.SIGMOID);
            AddConnection(new ConnectionGene(inNode.ID, temp.ID, 1.0f, true));
            AddConnection(new ConnectionGene(temp.ID, outNode.ID, existingConnection.Weight, true));
            AddNode(temp);
        }

        public NodeGene FindNodeByID(int pNodeID)
        {
            for (int i = 0; i < _nodes.Count; i++)
            {
                if (_nodes[i].ID == pNodeID)
                {
                    return _nodes[i];
                }
            }
            return null;
        }

        private bool ContainsNode(NodeGene pNode)
        {
            for (int i = 0; i < _nodes.Count; i++)
            {
                if (_nodes[i].ID == pNode.ID)
                    return true;
            }
            return false;
        }

        public static float Compare(Genome pParent1, Genome pParent2)
        {
            int parent1GenomeSize = pParent1.Connections.Count + pParent1.Nodes.Count;
            int parent2GenomeSize = pParent2.Connections.Count + pParent2.Nodes.Count;
            int maxGenomeSize = parent1GenomeSize > parent2GenomeSize ? parent1GenomeSize : parent2GenomeSize;

            List<ConnectionGene> parent1Conn = pParent1._connections.OrderBy(entry => entry.InnovationNumber).ToList();
            List<ConnectionGene> parent2Conn = pParent2._connections.OrderBy(entry => entry.InnovationNumber).ToList();

            int disjointCount = 0, excessCount = 0, matchingCount = 0;
            float matchingDifferenceInWeights = 0;
            int parent1Counter = 0, parent2Counter = 0;
            while (true)
            {
                if (parent1Conn.Count == parent1Counter)
                {
                    for (; parent2Counter < parent2Conn.Count; parent2Counter++)
                    {
                        excessCount++;
                    }
                    break;
                }
                else if (parent2Conn.Count == parent2Counter)
                {
                    for (; parent1Counter < parent1Conn.Count; parent1Counter++)
                    {
                        excessCount++;
                    }
                    break;
                }

                if (parent1Conn[parent1Counter].InnovationNumber < parent2Conn[parent2Counter].InnovationNumber)
                {
                    disjointCount++;
                    parent1Counter++;
                    continue;
                }
                else if (parent2Conn[parent2Counter].InnovationNumber < parent1Conn[parent1Counter].InnovationNumber)
                {
                    disjointCount++;
                    parent2Counter++;
                    continue;
                }

                if (parent1Conn[parent1Counter].InnovationNumber == parent2Conn[parent2Counter].InnovationNumber)
                {
                    matchingCount++;
                    matchingDifferenceInWeights += (float)Math.Abs(parent1Conn[parent1Counter].Weight - parent2Conn[parent2Counter].Weight);
                    parent1Counter++;
                    parent2Counter++;
                    continue;
                }

            }

            float totalExcess = (NEATConfig.EXCESS_FACTOR * excessCount) / maxGenomeSize;
            float totalDisjoint = (NEATConfig.DISJOINT_FACTOR * disjointCount) / maxGenomeSize;
            float avgDiffMatching = matchingDifferenceInWeights / matchingCount;
            if (float.IsNaN(avgDiffMatching)) { avgDiffMatching = 0f; }
            float totalMatching = NEATConfig.MATCHING_FACTOR * avgDiffMatching;
            float result = totalExcess + totalDisjoint + totalMatching;
            return result;
        }
    }
}
