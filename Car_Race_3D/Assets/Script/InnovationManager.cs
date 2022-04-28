using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base_NEAT
{
    class InnovationManager
    {
        private static int _innovationConnectionCounter = 0;

        /// <summary>
        /// Returns the innovation counter, then increments the counter
        /// </summary>
        /// <returns></returns>
        public static int GetConnectionInnovation()
        {
            return _innovationConnectionCounter++;
        }
    }
}
