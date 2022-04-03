using Game.Ecs.Components.Tags;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

namespace Game.Ecs.Systems {
    public partial class HardcodedEnemiesRotationConstraintSystem : SystemBase {
        protected override void OnUpdate() {
            Entities.WithAll<Tag_Enemy>().ForEach((ref PhysicsVelocity velocity, ref Rotation rotation) => {
                float4 currentRotation = rotation.Value.value;
                velocity.Angular = float3.zero;
                rotation.Value = quaternion.Euler(0, currentRotation.y, 0);
            }).ScheduleParallel();
        }
    }
}
