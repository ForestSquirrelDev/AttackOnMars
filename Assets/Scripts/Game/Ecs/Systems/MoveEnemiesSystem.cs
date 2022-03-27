using Game.Ecs.Components.Tags;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

namespace Game.Ecs.Systems {
    public partial class MoveEnemiesSystem : SystemBase {
        protected override void OnUpdate() {
            Entities.WithAll<Tag_Enemy>().ForEach((ref PhysicsVelocity velocity) => {
                float3 currentVelocityLinear = velocity.Linear;
                velocity.Linear = new float3(1, currentVelocityLinear.y, 1);
            }).ScheduleParallel();
        }
    }
}
