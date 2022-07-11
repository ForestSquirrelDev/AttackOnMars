using Game.AddressableConfigs;
using Game.Ecs.Components.Enemies;
using Game.Ecs.Components.Pathfinding;
using Game.Ecs.Components.Tags;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Game.Ecs.Systems {
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    public partial class MoveEnemiesSystem : SystemBase {
        private EnemyStatsConfig _config;

        protected override void OnCreate() {
            _config = AddressablesLoader.Get<EnemyStatsConfig>(AddressablesConsts.DefaultEnemyStatsConfig);
        }

        protected override void OnUpdate() {
            var xzSpeed = _config.XZMoveSpeed;
            var ySpeed = _config.YMoveSpeed;
            Entities.WithAll<Tag_Enemy>().ForEach((ref Translation translation, in EnemyStateComponent enemyState, in BestEnemyGridDirectionComponent bestDirection) => {
                if (enemyState.Value != EnemyState.Moving && enemyState.Value != EnemyState.ReadyToAttack) return;
                var x = Mathf.MoveTowards(translation.Value.x, translation.Value.x + bestDirection.Value.x, xzSpeed);
                var y = Mathf.MoveTowards(translation.Value.y, translation.Value.y + bestDirection.Value.y, ySpeed);
                var z = Mathf.MoveTowards(translation.Value.z, translation.Value.z + bestDirection.Value.z, xzSpeed);
                translation.Value = new float3(x, y, z);
            }).ScheduleParallel();
        }
    }
}
