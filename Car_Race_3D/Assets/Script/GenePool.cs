using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base_NEAT
{
    class GenePool
    {
        //Static gene pool which holds all connections made within the generations
        //static List<ConnectionGene> _genePool = new List<ConnectionGene>();
        static Dictionary<Tuple<int,int>, int> _genePool = new Dictionary<Tuple<int,int>,int>();

        /// <summary>
        /// Adds a connection to the gene pool, assumes this connection has already
        /// been tested for existence in gene pool
        /// </summary>
        /// <param name="pConnection"></param>
        public static void AddConnection(ConnectionGene pConnection)
        {
            _genePool.Add(new Tuple<int,int>(pConnection.InNode, pConnection.OutNode), pConnection.InnovationNumber);
        }

        /// <summary>
        /// Tests if connection gene exists within the gene pool,
        /// if so return the innovation number so that we don't increment this
        /// innovation without there being a need to as the connection is NOT unique
        /// </summary>
        /// <param name="pConnection">The connection gene to test for</param>
        /// <returns></returns>
        public static int ContainsConnection(int pInNode, int pOutNode)
        {
            Tuple<int,int> key = new Tuple<int, int>(pInNode, pOutNode);
            if (_genePool.TryGetValue(key, out int result))
            {
                return result;
            }
            return -1; //Returns -1 to signify the pConnection is unique
        }
    }
}
