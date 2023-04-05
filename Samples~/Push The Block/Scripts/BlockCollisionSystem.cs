using Unity.Entities;
using Unity.Jobs;
using Unity.Burst;
using Unity.Physics.Systems;
using Unity.Physics;
using UnityEngine;
using Unity.Collections;



partial class BlockCollisionSystem : SystemBase
{

    [ReadOnly] BuildPhysicsWorld buildPhysicsWorldSystem;
    [ReadOnly] StepPhysicsWorld stepPhysicsWorldSystem;

    protected override void OnCreate()
    {
        buildPhysicsWorldSystem = World.GetOrCreateSystem<BuildPhysicsWorld>();
        stepPhysicsWorldSystem = World.GetOrCreateSystem<StepPhysicsWorld>();
    }
    protected override void OnUpdate()
    {
        Dependency = JobHandle.CombineDependencies(
            Dependency,
            stepPhysicsWorldSystem.FinalSimulationJobHandle);

        // buildPhysicsWorldSystem.AddInputDependency(Dependency);

        var ecb = new EntityCommandBuffer(Allocator.TempJob);
        var triggerJob = new CollisionEventJob
        {
            entityCommandBuffer = ecb,
            BlockData = GetComponentDataFromEntity<Block>(isReadOnly: true),
            AgentData = GetComponentDataFromEntity<PushBlockCube>(isReadOnly: false),
        };
        Dependency = triggerJob.Schedule(
                stepPhysicsWorldSystem.Simulation, Dependency);
        Dependency.Complete();
        ecb.Playback(EntityManager);
        ecb.Dispose();

        return;
    }

    public struct CollisionEventJob : ITriggerEventsJob
    {

        // This looks at all of the collisions of the block. There is probably a better way to do this
        public EntityCommandBuffer entityCommandBuffer;
        [ReadOnly] public ComponentDataFromEntity<Block> BlockData;
        public ComponentDataFromEntity<PushBlockCube> AgentData;

        public void Execute(TriggerEvent triggerEvent)
        {
            Entity A = triggerEvent.EntityA;
            Entity B = triggerEvent.EntityB;

            if ((!BlockData.HasComponent(A)) && (!BlockData.HasComponent(B)))
            {
                // The collision does not involve a block
                return;
            }


            if (BlockData.HasComponent(A))
            {
                // Swap the two entities
                A = triggerEvent.EntityB;
                B = triggerEvent.EntityA;
            }

            var blockData = BlockData[B];

            if (blockData.TargetZone == A)
            {
                var pushAgent = AgentData[blockData.PushingAgent];
                pushAgent.status = PushBlockStatus.Success;
                AgentData[blockData.PushingAgent] = pushAgent;
            }

        }

    }
}


