using Game.Ecs.Components.Enemies;
using Game.Ecs.Components.Pathfinding;
using Game.Ecs.Components.Tags;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using Utils.Maths;

namespace Game.Ecs.Systems {
    public partial class EnemiesRotationSystem : SystemBase {
        protected override void OnUpdate() {
            Entities.WithAll<Tag_Enemy>().ForEach((ref Rotation rotation, in EnemyStateComponent enemyState, in LocalToWorld ltw, in BestEnemyDirectionComponent bestEnemyDirectionComponent) => {
                rotation.Value = quaternion.LookRotation( ((ltw.Position + bestEnemyDirectionComponent.Value) - ltw.Position).Normalize(), new float3(0, 1, 0));
            }).ScheduleParallel();
        }
    }
}
