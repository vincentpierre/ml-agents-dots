using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.AI.MLAgents;
using Unity.Collections;
using Unity.Physics;


[GenerateAuthoringComponent]
public struct BalanceBallSettings : IComponentData
{
    public Entity Ball;
}

// This is the data each agent (platform holds)
public struct AgentData : IComponentData
{

    // Where to reset the ball after a success or a failure
    public float3 BallResetPosition;

    // A reference to the ball entity the platform is trying to balance
    public Entity BallRef;

    // For how many updates has the platform been balancing the ball
    public int StepCount;
}

public struct Actuator : IComponentData
{
    public float2 Value;
}

public partial class BallSystem : SystemBase
{
    // If the platform maintained the ball for this many steps, it will be reset
    private const int maxStep = 1000;

    // The platform will only update its action every `decisionPeriod` steps
    private const int decisionPeriod = 5;

    // Used to keep track of the decisions period
    private int stepIndex = 0;

    // This is an actuator job. It processes the decisions of the policy and updates
    // the rotation of the platform
    private struct RotateJob : IActuatorJob
    {
        public ComponentDataFromEntity<Actuator> ComponentDataFromEntity;
        public void Execute(ActuatorEvent ev)
        {
            // a is the continuous action provided by the policy
            var a = ev.GetContinuousAction<Actuator>();
            // Add the Actuator struct to each agent Entity as a Component
            ComponentDataFromEntity[ev.Entity] = a;
        }
    }

    public Policy BallPolicy;


    // Update is called once per frame
    protected override void OnUpdate()
    {
        if (!BallPolicy.IsCreated)
        {
            return;
        }
        stepIndex++;
        if (stepIndex % decisionPeriod != 0)
        {
            return;
        }

        var policy = BallPolicy;

        ComponentDataFromEntity<Translation> TranslationFromEntity = GetComponentDataFromEntity<Translation>(isReadOnly: false);
        ComponentDataFromEntity<PhysicsVelocity> VelFromEntity = GetComponentDataFromEntity<PhysicsVelocity>(isReadOnly: false);
        Entities
        .WithNativeDisableParallelForRestriction(TranslationFromEntity)
        .WithNativeDisableParallelForRestriction(VelFromEntity)
        .ForEach((Entity entity, ref Rotation rot, ref AgentData agentData) =>
        {

            var ballPos = TranslationFromEntity[agentData.BallRef].Value;
            var ballVel = VelFromEntity[agentData.BallRef].Linear;
            var platformVel = VelFromEntity[entity];
            bool taskFailed = false;
            bool interruption = false;
            if (ballPos.y - agentData.BallResetPosition.y < -0.7f)
            {
                taskFailed = true;
                agentData.StepCount = 0;
            }
            if (agentData.StepCount > maxStep)
            {
                interruption = true;
                agentData.StepCount = 0;
            }
            if (!interruption && !taskFailed)
            {
                policy.RequestDecision(entity)
                        .SetObservation(0, rot.Value)
                        .SetObservation(1, ballPos - agentData.BallResetPosition)
                        .SetObservation(2, ballVel)
                        .SetObservation(3, platformVel.Angular)
                        .SetReward((0.1f));
            }
            if (taskFailed)
            {
                policy.EndEpisode(entity)
                    .SetObservation(0, rot.Value)
                    .SetObservation(1, ballPos - agentData.BallResetPosition)
                    .SetObservation(2, ballVel)
                    .SetObservation(3, platformVel.Angular)
                    .SetReward(-1f);
            }
            else if (interruption)
            {
                policy.InterruptEpisode(entity)
                    .SetObservation(0, rot.Value)
                    .SetObservation(1, ballPos - agentData.BallResetPosition)
                    .SetObservation(2, ballVel)
                    .SetObservation(3, platformVel.Angular)
                    .SetReward((0.1f));
            }
            if (interruption || taskFailed)
            {
                VelFromEntity[agentData.BallRef] = new PhysicsVelocity();
                TranslationFromEntity[agentData.BallRef] = new Translation { Value = agentData.BallResetPosition };
                rot.Value = quaternion.identity;
            }
            agentData.StepCount++;

        }).ScheduleParallel();

        var reactiveJob = new RotateJob
        {
            ComponentDataFromEntity = GetComponentDataFromEntity<Actuator>(isReadOnly: false)
        };
        Dependency = reactiveJob.Schedule(policy, Dependency);

        Entities.ForEach((ref Actuator act, ref Rotation rotation) =>
        {
            var rot = math.mul(rotation.Value, quaternion.Euler(0.05f * new float3(act.Value.x, 0, act.Value.y)));
            rotation.Value = rot;
        }).ScheduleParallel();
    }

    protected override void OnDestroy()
    {
        BallPolicy.Dispose();
    }
}
