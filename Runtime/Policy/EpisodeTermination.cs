using Unity.Collections;
using Unity.Mathematics;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using System;

namespace Unity.AI.MLAgents
{
    /// <summary>
    /// A EpisodeTermination is a struct used to provide data about an Agent to a Policy.
    /// This data will be used to notify of the end of the episode of an Agent.
    /// Adding data is done through a builder pattern.
    /// </summary>
    public struct EpisodeTermination
    {
        private int m_Index;
        private Policy m_Policy;
        private int m_ObservationIndex;

        internal EpisodeTermination(int index, Policy policy)
        {
            this.m_Index = index;
            this.m_Policy = policy;
            this.m_ObservationIndex = 0;
        }

        /// <summary>
        /// Sets the reward that the Agent has accumulated since the last decision request.
        /// Add any "end of episode" reward.
        /// </summary>
        /// <param name="r"> The reward value </param>
        /// <returns> The EpisodeTermination struct </returns>
        public EpisodeTermination SetReward(float r)
        {
            m_Policy.TerminationRewards[m_Index] = r;
            return this;
        }

        /// <summary>
        /// Sets the observation for of the end of the Episode.
        /// </summary>
        /// <param name="sensor"> A struct strictly containing floats used as observation data </param>
        /// <returns> The EpisodeTermination struct </returns>
        public EpisodeTermination SetObservation<T>(T sensor) where T : struct
        {
            m_ObservationIndex += 1;
            return this.SetObservation(m_ObservationIndex - 1, sensor);
        }

        /// <summary>
        /// Sets the observation for of the end of the Episode.
        /// </summary>
        /// <param name="sensorNumber"> The index of the observation as provided when creating the associated Policy </param>
        /// <param name="sensor"> A struct strictly containing floats used as observation data </param>
        /// <returns> The EpisodeTermination struct </returns>
        public EpisodeTermination SetObservation<T>(int sensorNumber, T sensor) where T : struct
        {
            int inputSize = UnsafeUtility.SizeOf<T>() / sizeof(float);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            int3 s = m_Policy.SensorShapes[sensorNumber];
            int expectedInputSize = s.x * math.max(1, s.y) * math.max(1, s.z);
            if (inputSize != expectedInputSize)
            {
                throw new MLAgentsException(
                    $"Cannot set observation {sensorNumber} due to incompatible size of the input. Expected size : { expectedInputSize }, received size : { inputSize}");
            }
#endif
            int start = m_Policy.ObservationOffsets[sensorNumber];
            start += inputSize * m_Index;
            var tmp = m_Policy.TerminationObs.Slice(start, inputSize).SliceConvert<T>();
            tmp[0] = sensor;
            return this;
        }

        /// <summary>
        /// Sets the observation for a termination request using a categorical value.
        /// </summary>
        /// <param name="sensor"> An integer containing the index of the categorical observation </param>
        /// <returns> The EpisodeTermination struct </returns>
        public EpisodeTermination SetObservation(int sensor)
        {
            m_ObservationIndex += 1;
            return this.SetObservation(m_ObservationIndex - 1, sensor);
        }

        /// <summary>
        /// Sets the observation for a termination request using a categorical value.
        /// </summary>
        /// <param name="sensorNumber"> The index of the observation as provided when creating the associated Policy </param>
        /// <param name="sensor"> An integer containing the index of the categorical observation </param>
        /// <returns> The EpisodeTermination struct </returns>
        public EpisodeTermination SetObservation(int sensorNumber, int sensor)
        {
            int3 s = m_Policy.SensorShapes[sensorNumber];
            int maxValue = s.x;
#if ENABLE_UNITY_COLLECTIONS_CHECKS

            if (s.y != 0 || s.z != 0)
            {
                throw new MLAgentsException(
                    $"Categorical observation must have a shape (max_category, 0, 0)");
            }
            if (sensor > maxValue)
            {
                throw new MLAgentsException(
                    $"Categorical observation is out of bound for observation {sensorNumber} with maximum {maxValue} (received {sensor}.");
            }
#endif
            int start = m_Policy.ObservationOffsets[sensorNumber];
            start += maxValue * m_Index;
            for (int i = 0; i < maxValue; i++)
            {
                m_Policy.TerminationObs[start + i] = 0.0f;
            }
            m_Policy.TerminationObs[start + sensor] = 1.0f;
            return this;
        }

        /// <summary>
        /// Sets the last observation the Agent perceives before ending the episode.
        /// </summary>
        /// <param name="obs"> A NativeSlice of floats containing the observation data </param>
        /// <returns> The EpisodeTermination struct </returns>
        public EpisodeTermination SetObservationFromSlice([ReadOnly] NativeSlice<float> obs)
        {
            m_ObservationIndex += 1;
            return this.SetObservationFromSlice(m_ObservationIndex - 1, obs);
        }

        /// <summary>
        /// Sets the last observation the Agent perceives before ending the episode.
        /// </summary>
        /// <param name="sensorNumber"> The index of the observation as provided when creating the associated Policy </param>
        /// <param name="obs"> A NativeSlice of floats containing the observation data </param>
        /// <returns> The EpisodeTermination struct </returns>
        public EpisodeTermination SetObservationFromSlice(int sensorNumber, [ReadOnly] NativeSlice<float> obs)
        {
            int inputSize = obs.Length;
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            int3 s = m_Policy.SensorShapes[sensorNumber];
            int expectedInputSize = s.x * math.max(1, s.y) * math.max(1, s.z);
            if (inputSize != expectedInputSize)
            {
                throw new MLAgentsException(
                    $"Cannot set observation {sensorNumber} due to incompatible size of the input. Expected size : {expectedInputSize}, received size : { inputSize}");
            }
#endif
            int start = m_Policy.ObservationOffsets[sensorNumber];
            start += inputSize * m_Index;
            m_Policy.TerminationObs.Slice(start, inputSize).CopyFrom(obs);
            return this;
        }
    }
}
