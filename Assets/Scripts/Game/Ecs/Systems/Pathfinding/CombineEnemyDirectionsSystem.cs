using Game.Ecs.Components.Enemies;
using Game.Ecs.Components.Pathfinding;
using Game.Ecs.Components.Tags;
using Unity.Entities;
using Unity.Mathematics;

namespace Game.Ecs.Systems.Spawners {
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    public partial class CombineEnemyDirectionsSystem : SystemBase {
		protected override void OnUpdate() {
            Entities.WithAll<Tag_Enemy>().ForEach((ref BestEnemyCombinedDirectionComponent combinedDirection, in BestEnemyGridDirectionComponent gridDirection,
                in BestEnemyLocalAvoidanceDirection localAvoidanceDirection, in YAxisEnemyDirectionComponent yAxisDirection, in Entity e) => {
                var combinedDir = new float3(gridDirection.Value.x + localAvoidanceDirection.Value.x, yAxisDirection.Value, gridDirection.Value.y + localAvoidanceDirection.Value.y);
                combinedDirection.Value = math.normalizesafe(combinedDir);
//                Debug.Log($"Combined dir: {combinedDir}. Component value: {combinedDirection.Value}");
            }).ScheduleParallel();
        }
    }
}