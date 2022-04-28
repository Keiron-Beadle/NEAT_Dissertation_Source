using System;

namespace Base_NEAT
{
    enum NodeType
    {
        INPUT,
        HIDDEN,
        OUTPUT
    }
    enum ACTIVATION_FUNCTION
    {
        SIGMOID,
    }

    class NodeGene
    {
        //private static int _ClassSize = Marshal.SizeOf(typeof(NodeGene));
        private static int _ClassSize = 24;
        private int _id;
        private NodeType _type;
        private ACTIVATION_FUNCTION _activationFunc;
        private float _value = 0.0f;

        public int ID { get { return _id; } }
        public NodeType Type { get { return _type; } }
        public ACTIVATION_FUNCTION ActivationFunction { get { return _activationFunc; } }
        public float Value { get { return _value; } set { _value = value; } }

        public NodeGene(int pID, NodeType pType, ACTIVATION_FUNCTION pFunc)
        {
            _id = pID;
            _type = pType;
            _activationFunc = pFunc;
        }

        public static NodeGene Copy(NodeGene pBase)
        {
            return new NodeGene(pBase.ID, pBase.Type, pBase.ActivationFunction);
        }

        public void AddValue(float pValue)
        {
            _value += pValue;
        }

        private void SigmoidActivation()
        {
            _value = 1 / (1 + (float)Math.Pow(Math.E, -_value) );
        }

        public void ActivateFunction()
        {
            switch (_activationFunc)
            {
                case ACTIVATION_FUNCTION.SIGMOID:
                    SigmoidActivation();
                    break;
                default:
                    Console.WriteLine("Error: No valid activation function set for Node: " + _id);
                    break;
            }
        }

        public int GetMemBytes() { return _ClassSize; }

    }
}
