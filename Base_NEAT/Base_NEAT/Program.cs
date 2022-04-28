using System;

namespace Base_NEAT
{
    class Program
    {
        static void Main(string[] args)
        {
            NodeGene input = new NodeGene(0, NodeType.INPUT, ACTIVATION_FUNCTION.SIGMOID);
            NodeGene output = new NodeGene(1, NodeType.OUTPUT, ACTIVATION_FUNCTION.SIGMOID);
            Genome fundamentalGenome = new Genome();
            fundamentalGenome.AddNode(input);
            fundamentalGenome.AddNode(output);
            fundamentalGenome.AddConnection(new ConnectionGene(0,1,0.5f,true));
            Trainer trainer = new Trainer(100, fundamentalGenome); // Params (Population size , Base Genome)
            for (int i = 0; i < 101; i++) // 101 generations
            {
                trainer.Evaluate();
                if (i % 10 == 0)
                {
                    Console.WriteLine($"Gen: {i}, Fittest Genome Node: {trainer.FittestNetwork.Genome.Nodes.Count} , Fittest Genome Connections: {trainer.FittestNetwork.Genome.Connections.Count}");
                    Console.WriteLine();
                }
            }
            Console.ReadLine();
        }
    }
}
