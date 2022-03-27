using Game.Ecs.Components.Tags;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

namespace Game.Ecs.Systems {
    public partial class MoveEnemySystem : SystemBase {
        protected override void OnUpdate() {
            Entities.WithAll<Tag_Enemy>().ForEach((ref PhysicsVelocity velocity, ref PhysicsMass mass, ref Rotation rotation) => {
                float3 currentVelocityLinear = velocity.Linear;
                float4 currentRotation = rotation.Value.value;
                velocity.Linear = new float3(1, currentVelocityLinear.y, 1);
                velocity.Angular = float3.zero;
                rotation.Value = quaternion.Euler(0, currentRotation.y, 0);
            }).ScheduleParallel();
        }
    }
}
