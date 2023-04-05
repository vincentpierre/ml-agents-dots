using Unity.Collections;
using Unity.Transforms;
using Unity.Physics;
using Unity.Mathematics;

namespace Unity.AI.MLAgents
{
    public static class RaycastObservationUtility
    {
        /// <summary>
        /// Generates a NativeArray of floats corresponding to a plannar raycast.
        /// The Array will be of total size ( num raycast )
        /// </summary>
        /// <param name="numRaycasts"> The number of raycasts </param>
        /// <param name="position"> The position of the raycast origin </param>
        /// <param name="rotation"> The rotation of the raycast origin </param>
        /// <param name="maxDistance"> The maximum distance for each raycast </param>
        /// <param name="angle"> The Angle of the raycasts.</param>
        /// <param name="verticalOffset"> The vertical offset for the raycast origin</param>
        /// <param name="collisionFilter"> the Collisiofilter for the raycasts </param>
        /// <param name="collisionWorld"> the CollisionWorld for the raycasts </param>
        /// <param name="allocator"> the Allocator for the Native array </param>
        /// <returns> A native array of floats containing the image data from the camera </returns>
        public static NativeArray<float> GetRayObs(int numRaycasts, Translation position, Rotation rotation, float maxDistance, float verticalOffset, float angle, CollisionFilter collisionFilter, CollisionWorld collisionWorld, Allocator allocator = Allocator.Temp)
        {
            var Rinputs = new NativeArray<Unity.Physics.RaycastInput>(numRaycasts, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            var hit = new NativeList<Unity.Physics.RaycastHit>(8, Allocator.Temp);
            var allRayData = new NativeArray<float>(numRaycasts, allocator, NativeArrayOptions.ClearMemory);

            PrepareRayCastInputs(ref Rinputs, ref position, ref rotation, ref collisionFilter, verticalOffset, angle);

            for (int i = 0; i < numRaycasts; i++)
            {
                hit.Clear();
                bool haveHit = collisionWorld.CastRay(Rinputs[i], ref hit);
                float minDistance = 1.0f;

                if (haveHit)
                {
                    for (int j = 0; j < hit.Length; j++)
                    {
                        if (hit[j].Fraction > 0.001f) // Seems when ray cast starts in the collider, fraction is 0.0f;
                        {
                            minDistance = math.min(minDistance, hit[j].Fraction);
                        }
                    }
                }
                allRayData[i] = minDistance;
            }
            Rinputs.Dispose();
            hit.Dispose();
            return allRayData;
        }

        private static void PrepareRayCastInputs(ref NativeArray<Unity.Physics.RaycastInput> inputs, ref Translation pos, ref Rotation rot, ref CollisionFilter collisionFilter, float verticalOffset, float fanAngle)
        {
            float3 forward = math.forward(rot.Value);
            float3 right = math.mul(rot.Value, new float3(1, 0, 0));//math.right(rot.Value);
            float3 up = math.mul(rot.Value, new float3(0, 1, 0));//math.right(rot.Value);math.up(rot.Value);
            float3 upOffset = verticalOffset * new float3(0, 1, 0);
            float RaycastCarRadius = 0f; //1.0f;
            float ObstacleSightDistance = 20f;

            int numRays = inputs.Length;

            for (int i = 0; i < numRays; i++)
            {
                var angle = ((fanAngle / (numRays - 1)) * i - fanAngle / 2f) * math.PI / fanAngle;
                var vec = forward * math.cos(angle) + right * math.sin(angle);
                inputs[i] = new RaycastInput
                {
                    Filter = collisionFilter,
                    Start = pos.Value + upOffset + vec * RaycastCarRadius,
                    End = pos.Value + upOffset + vec * ObstacleSightDistance
                };
            }
        }
    }
}
