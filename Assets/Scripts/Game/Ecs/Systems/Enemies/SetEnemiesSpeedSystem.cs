using Game.AddressableConfigs;
using Game.Ecs.Components;
using Game.Ecs.Components.Tags;
using Unity.Entities;
using UnityEngine;

namespace Game.Ecs.Systems.Spawners {
    public partial class SetEnemiesSpeedSystem : SystemBase {
        private EnemyStatsConfig _config;

        protected override void OnCreate() {
            _config = AddressablesLoader.Get<EnemyStatsConfig>(AddressablesConsts.DefaultEnemyStatsConfig);
        }

        protected override void OnUpdate() {
            var defaultSpeed = _config.XZMoveSpeed;
            var randomFactor = _config.XZRandomSpeedFactor;
            var random = randomFactor * Random.value;
            
            Entities.WithAll<Tag_Enemy>().ForEach((ref EnemySpeedComponent speed) => {
                if (speed.Value != 0) return;
                speed.Value = defaultSpeed + random;
            }).ScheduleParallel();
        }
    }
}