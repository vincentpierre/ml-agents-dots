using Unity.Collections;
using Unity.Mathematics;
using System.Text;

namespace Unity.AI.MLAgents
{
    public static class SensorShape
    {
        public static int3 Vector(int dim)
        {
            return new int3(dim, 0, 0);
        }

        public static int3 Matrix(int width, int height, int channel)
        {
            return new int3(width, height, channel);
        }
    }
}
