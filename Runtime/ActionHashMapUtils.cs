using Unity.Collections;
using Unity.Entities;
using Unity.Collections.LowLevel.Unsafe;

namespace Unity.AI.MLAgents
{
    // TODO : A potential API to retrieve the actions on the main thread for projects allergic to jobs ?
    // TODO : Make faster and parallel
    public static class ActionHashMapUtils
    {
        /// <summary>
        /// Retrieves the action data for a Policy in puts it into a HashMap.
        /// This action deletes the action data from the Policy.
        /// </summary>
        /// <param name="policy"> The Policy the data will be retrieved from.</param>
        /// <param name="allocator"> The memory allocator of the create NativeHashMap.</param>
        /// <typeparam name="T"> The type of the Action struct. It must match the Action Size
        /// and Action Type of the Policy.</typeparam>
        /// <returns> A NativeHashMap from Entities to Actions with type T.</returns>
        public static NativeHashMap<Entity, T> GenerateActionHashMap<T>(this Policy policy, Allocator allocator) where T : struct
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (policy.ActionSize != UnsafeUtility.SizeOf<T>() / 4)
            {
                var receivedSize = UnsafeUtility.SizeOf<T>() / 4;
                throw new MLAgentsException($"Action space size does not match for action. Expected {policy.ActionSize} but received {receivedSize}");
            }
#endif
            Academy.Instance.UpdatePolicy(policy);
            int actionCount = policy.ActionCounter.Count;
            var result = new NativeHashMap<Entity, T>(actionCount, allocator);
            int size = policy.ActionSize;
            for (int i = 0; i < actionCount; i++)
            {
                if (policy.ActionType == ActionType.DISCRETE)
                {
                    result.TryAdd(policy.ActionAgentEntityIds[i], policy.DiscreteActuators.Slice(i * size, size).SliceConvert<T>()[0]);
                }
                else
                {
                    result.TryAdd(policy.ActionAgentEntityIds[i], policy.ContinuousActuators.Slice(i * size, size).SliceConvert<T>()[0]);
                }
            }
            policy.ResetActionsCounter();
            return result;
        }
    }
}
