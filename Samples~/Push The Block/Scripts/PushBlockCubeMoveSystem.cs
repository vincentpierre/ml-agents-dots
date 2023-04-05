using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Entities;
using Unity.Physics;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using Unity.AI.MLAgents;



[UpdateBefore(typeof(BlockCollisionSystem))]
partial class PushBlockCubeMoveSystem : SystemBase
{
    public Policy PushBlockPolicy;
    Unity.Physics.Systems.BuildPhysicsWorld physicsWorldSystem;
    int counter = 0;

    private struct UpdatePushBlockAction : IActuatorJob
    {
        public ComponentDataFromEntity<PushBlockAction> ComponentDataFromEntity;
        public void Execute(ActuatorEvent ev)
        {
            var a = ev.GetDiscreteAction<PushBlockAction>();
            ComponentDataFromEntity[ev.Entity] = a;
        }
    }

    protected override void OnCreate()
    {
        physicsWorldSystem = World.GetExistingSystem<Unity.Physics.Systems.BuildPhysicsWorld>();
    }

    protected override void OnUpdate()
    {
        if (!PushBlockPolicy.IsCreated) { return; }

        var positionData = GetComponentDataFromEntity<Translation>(isReadOnly: false);
        var policy = PushBlockPolicy;


        var collisionWorld = physicsWorldSystem.PhysicsWorld.CollisionWorld;

        int NumOfRayCasts = 9;

        counter++;
        if (counter % 8 == 0)
            Entities.WithReadOnly(collisionWorld).WithNativeDisableContainerSafetyRestriction(positionData).ForEach((Entity entity, ref PushBlockCube cube, ref Rotation rot) =>
            {

                var pos = positionData[entity];




                //////////////////////////////////////////////////////////

                /* RAYCAST SECTION*/
                var Rinputs = new NativeArray<Unity.Physics.RaycastInput>(NumOfRayCasts, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
                var hit = new NativeList<Unity.Physics.RaycastHit>(8, Allocator.Temp);
                var allRayData = new NativeArray<float>(NumOfRayCasts * 2 * 4, Allocator.Temp, NativeArrayOptions.ClearMemory);


                // Collide with all level 0
                var collisionFilter = new CollisionFilter
                {
                    BelongsTo = ~0u, // all 1s, so all layers, collide with everything
                    CollidesWith = ~0u,
                    GroupIndex = 0
                };
                PrepareRayCastInputs(ref Rinputs, ref pos, ref rot, ref collisionFilter, 0f);
                for (int i = 0; i < NumOfRayCasts; i++)
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
                    allRayData[i * 8] = minDistance;
                }
                // Collide with all level 1
                collisionFilter = new CollisionFilter
                {
                    BelongsTo = ~0u, // all 1s, so all layers, collide with everything
                    CollidesWith = ~0u,
                    GroupIndex = 0
                };
                PrepareRayCastInputs(ref Rinputs, ref pos, ref rot, ref collisionFilter, 1f);
                for (int i = 0; i < NumOfRayCasts; i++)
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
                    allRayData[i * 8 + 1] = minDistance;
                }

                // Collide with block level 0
                collisionFilter = new CollisionFilter
                {
                    BelongsTo = ~0u,
                    CollidesWith = 1u << 1,
                    GroupIndex = 0
                };
                PrepareRayCastInputs(ref Rinputs, ref pos, ref rot, ref collisionFilter, 0f);
                for (int i = 0; i < NumOfRayCasts; i++)
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
                    allRayData[i * 8 + 2] = minDistance;
                }
                // Collide with block level 1
                collisionFilter = new CollisionFilter
                {
                    BelongsTo = ~0u,
                    CollidesWith = 1u << 1,
                    GroupIndex = 0
                };
                PrepareRayCastInputs(ref Rinputs, ref pos, ref rot, ref collisionFilter, 1f);
                for (int i = 0; i < NumOfRayCasts; i++)
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
                    allRayData[i * 8 + 3] = minDistance;
                }
                // Collide with goal level 0
                collisionFilter = new CollisionFilter
                {
                    BelongsTo = ~0u,
                    CollidesWith = 1u << 2,
                    GroupIndex = 0
                };
                PrepareRayCastInputs(ref Rinputs, ref pos, ref rot, ref collisionFilter, 0f);
                for (int i = 0; i < NumOfRayCasts; i++)
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
                    allRayData[i * 8 + 4] = minDistance;
                }
                // Collide with goal level 1
                collisionFilter = new CollisionFilter
                {
                    BelongsTo = ~0u,
                    CollidesWith = 1u << 2,
                    GroupIndex = 0
                };
                PrepareRayCastInputs(ref Rinputs, ref pos, ref rot, ref collisionFilter, 1f);
                for (int i = 0; i < NumOfRayCasts; i++)
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
                    allRayData[i * 8 + 5] = minDistance;
                }

                //////////////////////////////////////////////////////////


                var collisionFilterAll = new CollisionFilter
                {
                    BelongsTo = ~0u, // all 1s, so all layers, collide with everything
                    CollidesWith = ~0u,
                    GroupIndex = 0
                };

                var collisionFilterBlock = new CollisionFilter
                {
                    BelongsTo = ~0u,
                    CollidesWith = 1u << 1,
                    GroupIndex = 0
                };
                var collisionFilterGoal = new CollisionFilter
                {
                    BelongsTo = ~0u,
                    CollidesWith = 1u << 2,
                    GroupIndex = 0
                };




                var tmp0 = RaycastObservationUtility.GetRayObs(9, pos, rot, 20f, 0, 180f, collisionFilterAll, collisionWorld);
                var tmp1 = RaycastObservationUtility.GetRayObs(9, pos, rot, 20f, 1f, 180f, collisionFilterAll, collisionWorld);
                var tmp2 = RaycastObservationUtility.GetRayObs(9, pos, rot, 20f, 0f, 180f, collisionFilterBlock, collisionWorld);
                var tmp3 = RaycastObservationUtility.GetRayObs(9, pos, rot, 20f, 1f, 180f, collisionFilterBlock, collisionWorld);
                var tmp4 = RaycastObservationUtility.GetRayObs(9, pos, rot, 20f, 0f, 180f, collisionFilterGoal, collisionWorld);
                var tmp5 = RaycastObservationUtility.GetRayObs(9, pos, rot, 20f, 1f, 180f, collisionFilterGoal, collisionWorld);



                for (int i = 0; i < 8; i++)
                {
                    if (math.abs(allRayData[i * 8] - tmp0[i]) > 0.0001)
                    {
                        UnityEngine.Debug.LogError(i + $" was {tmp0[i]} but expected {allRayData[i * 8]}");
                    }
                    if (math.abs(allRayData[i * 8 + 1] - tmp1[i]) > 0.0001)
                    {
                        UnityEngine.Debug.LogError(i + $" was {tmp1[i]} but expected {allRayData[i * 8 + 1]}");
                    }
                    if (math.abs(allRayData[i * 8 + 2] - tmp2[i]) > 0.0001)
                    {
                        UnityEngine.Debug.LogError(i + $" was {tmp2[i]} but expected {allRayData[i * 8 + 2]}");
                    }
                    if (math.abs(allRayData[i * 8 + 3] - tmp3[i]) > 0.0001)
                    {
                        UnityEngine.Debug.LogError(i + $" was {tmp3[i]} but expected {allRayData[i * 8 + 3]}");
                    }
                    if (math.abs(allRayData[i * 8 + 4] - tmp4[i]) > 0.0001)
                    {
                        UnityEngine.Debug.LogError(i + $" was {tmp4[i]} but expected {allRayData[i * 8 + 4]}");
                    }
                    if (math.abs(allRayData[i * 8 + 5] - tmp5[i]) > 0.0001)
                    {
                        UnityEngine.Debug.LogError(i + $" was {tmp5[i]} but expected {allRayData[i * 8 + 5]}");
                    }
                }


                // If uninitialized, set the reset position
                if (cube.status == PushBlockStatus.UnInitialized)
                {
                    cube.resetPosition = pos.Value;
                    cube.status = PushBlockStatus.Ongoing;
                }

                if (cube.status == PushBlockStatus.Ongoing && cube.stepCount < 200)
                {
                    // policy.RequestDecision(entity).SetObservationFromSlice(0, allRayData.Slice(0, 72)).SetReward(-0.005f);
                    policy.RequestDecision(entity)
                        .SetObservationFromSlice(RaycastObservationUtility.GetRayObs(9, pos, rot, 20f, 0, 180f, collisionFilterAll, collisionWorld))
                        .SetObservationFromSlice(RaycastObservationUtility.GetRayObs(9, pos, rot, 20f, 1f, 180f, collisionFilterAll, collisionWorld))
                        .SetObservationFromSlice(RaycastObservationUtility.GetRayObs(9, pos, rot, 20f, 0f, 180f, collisionFilterBlock, collisionWorld))
                        .SetObservationFromSlice(RaycastObservationUtility.GetRayObs(9, pos, rot, 20f, 1f, 180f, collisionFilterBlock, collisionWorld))
                        .SetObservationFromSlice(RaycastObservationUtility.GetRayObs(9, pos, rot, 20f, 0f, 180f, collisionFilterGoal, collisionWorld))
                        .SetObservationFromSlice(RaycastObservationUtility.GetRayObs(9, pos, rot, 20f, 1f, 180f, collisionFilterGoal, collisionWorld))
                        .SetReward(-0.005f);
                }
                else if (cube.status == PushBlockStatus.Success)
                {
                    // policy.EndEpisode(entity)
                    //     .SetObservationFromSlice(0, allRayData.Slice(0, 72))
                    //     .SetReward(1f);
                    policy.EndEpisode(entity)
                        .SetObservationFromSlice(RaycastObservationUtility.GetRayObs(9, pos, rot, 20f, 0, 180f, collisionFilterAll, collisionWorld))
                        .SetObservationFromSlice(RaycastObservationUtility.GetRayObs(9, pos, rot, 20f, 1f, 180f, collisionFilterAll, collisionWorld))
                        .SetObservationFromSlice(RaycastObservationUtility.GetRayObs(9, pos, rot, 20f, 0f, 180f, collisionFilterBlock, collisionWorld))
                        .SetObservationFromSlice(RaycastObservationUtility.GetRayObs(9, pos, rot, 20f, 1f, 180f, collisionFilterBlock, collisionWorld))
                        .SetObservationFromSlice(RaycastObservationUtility.GetRayObs(9, pos, rot, 20f, 0f, 180f, collisionFilterGoal, collisionWorld))
                        .SetObservationFromSlice(RaycastObservationUtility.GetRayObs(9, pos, rot, 20f, 1f, 180f, collisionFilterGoal, collisionWorld))
                        .SetReward(1f);
                    cube.status = PushBlockStatus.Ongoing;
                    pos.Value = cube.resetPosition;
                    positionData[entity] = pos;
                    cube.stepCount = 0;
                    rot.Value = quaternion.AxisAngle(new float3(0, 1, 0), 3.14f);
                    var blockPosition = new Translation();
                    blockPosition.Value = cube.resetPosition + new float3(6, 0, -3);
                    positionData[cube.block] = blockPosition;
                }
                else if (cube.status == PushBlockStatus.Ongoing && cube.stepCount >= 200)
                {
                    // policy.InterruptEpisode(entity).SetObservationFromSlice(0, allRayData.Slice(0, 72));
                    policy.InterruptEpisode(entity)
                        .SetObservationFromSlice(RaycastObservationUtility.GetRayObs(9, pos, rot, 20f, 0, 180f, collisionFilterAll, collisionWorld))
                        .SetObservationFromSlice(RaycastObservationUtility.GetRayObs(9, pos, rot, 20f, 1f, 180f, collisionFilterAll, collisionWorld))
                        .SetObservationFromSlice(RaycastObservationUtility.GetRayObs(9, pos, rot, 20f, 0f, 180f, collisionFilterBlock, collisionWorld))
                        .SetObservationFromSlice(RaycastObservationUtility.GetRayObs(9, pos, rot, 20f, 1f, 180f, collisionFilterBlock, collisionWorld))
                        .SetObservationFromSlice(RaycastObservationUtility.GetRayObs(9, pos, rot, 20f, 0f, 180f, collisionFilterGoal, collisionWorld))
                        .SetObservationFromSlice(RaycastObservationUtility.GetRayObs(9, pos, rot, 20f, 1f, 180f, collisionFilterGoal, collisionWorld));
                    cube.status = PushBlockStatus.Ongoing;
                    pos.Value = cube.resetPosition;
                    positionData[entity] = pos;
                    cube.stepCount = 0;
                    rot.Value = quaternion.AxisAngle(new float3(0, 1, 0), 3.14f);
                    var blockPosition = new Translation();
                    blockPosition.Value = cube.resetPosition + new float3(6, 0, -3);
                    positionData[cube.block] = blockPosition;
                }
                cube.stepCount += 1;
                // allRayData.Dispose();
            }).ScheduleParallel();

        var updateActionJob = new UpdatePushBlockAction
        {
            ComponentDataFromEntity = GetComponentDataFromEntity<PushBlockAction>(isReadOnly: false)
        };
        Dependency = updateActionJob.Schedule(policy, Dependency);

        Entities.WithAll<PushBlockCube>().ForEach((ref Translation pos, ref PhysicsVelocity vel, ref PushBlockAction action, ref Rotation rot) =>
        {

            vel.Angular = 3 * new float3(0, action.Rotate - 1, 0); // action (0,1,2) -> ( -1, 0, 1)
            float3 forward = math.forward(rot.Value);
            vel.Linear = 3 * forward * (action.Forward - 1);
        }).ScheduleParallel();
        // inputDeps.Complete();
        return;
    }



    static void PrepareRayCastInputs(ref NativeArray<Unity.Physics.RaycastInput> inputs, ref Translation pos, ref Rotation rot, ref CollisionFilter collisionFilter, float verticalOffset)
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
            var angle = ((180f / (numRays - 1)) * i - 90f) * math.PI / 180f;
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
