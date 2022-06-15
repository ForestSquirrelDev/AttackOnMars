using Game.Ecs.Components.Pathfinding;
using Game.Ecs.Components.Tags;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Game.Ecs.Systems {
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    public partial class MoveEnemiesSystem : SystemBase {
        private const float _xzDelta = 0.02f;
        private const float _yDelta = 0.2f;
        
        protected override void OnUpdate() {
            Entities.WithAll<Tag_Enemy>().ForEach((ref Translation translation, in BestEnemyDirectionComponent bestDirection) => {
                var x = Mathf.MoveTowards(translation.Value.x, translation.Value.x + bestDirection.Value.x, _xzDelta);
                var y = Mathf.MoveTowards(translation.Value.y, bestDirection.Value.y, _yDelta);
                var z = Mathf.MoveTowards(translation.Value.z, translation.Value.z + bestDirection.Value.z, _xzDelta);
                translation.Value = new float3(x, y, z);
            }).ScheduleParallel();
        }
    }
}
