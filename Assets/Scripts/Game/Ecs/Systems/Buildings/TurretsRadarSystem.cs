using Game.AddressableConfigs;
using Game.Ecs.Components.Buildings;
using Game.Ecs.Components.Tags;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Game.Ecs.Systems.Spawners {
    public partial class TurretsRadarSystem : SystemBase {
        private TurretsConfig _turretsConfig;
        private EnemiesSpawnerConfig _spawnerConfig;

        protected override void OnCreate() {
            _turretsConfig = ConfigsLoader.Get<TurretsConfig>(AddressablesConsts.DefaultTurretsConfig);
            _spawnerConfig = ConfigsLoader.Get<EnemiesSpawnerConfig>(AddressablesConsts.DefaultEnemiesSpawnerConfig);
        }
        
		protected override void OnUpdate() {
            var allEnemies = CollectAllEnemies();
            
            var radarFrequency = _turretsConfig.RadarFrequencySeconds;
            var maxDistane = _turretsConfig.EffectiveRadius;
            var deltaTime = UnityEngine.Time.deltaTime;
            Dependency = Entities.WithAll<Tag_Turret>().ForEach((ref RadarTickCounterComponent radarTickCounter, ref CurrentTargetComponent currentTarget, in LocalToWorld ltw) => {
                if (radarTickCounter.Value > 0) {
                    radarTickCounter.Value -= deltaTime;
                    return;
                }
                if (currentTarget.Entity != Entity.Null) return;

                Entity nearestEntity = Entity.Null;
                LocalToWorld nearestEntityLtw = default;
                float nearestDistance = float.MaxValue;
                for (int j = 0; j < allEnemies.Length; j++) {
                    var enemy = allEnemies[j];
                    var distance = math.distance(ltw.Position, enemy.Ltw.Position);
                    if (distance < nearestDistance) {
                        nearestEntity = enemy.Entity;
                        nearestDistance = distance;
                        nearestEntityLtw = enemy.Ltw;
                    }
                }

                if (nearestDistance <= maxDistane) {
                    currentTarget.Entity = nearestEntity;
                    currentTarget.Ltw = nearestEntityLtw;
                }
                
                radarTickCounter.Value = radarFrequency;
            }).Schedule(Dependency);

            allEnemies.Dispose(Dependency);
        }
        
        private NativeList<EnemyEntityData> CollectAllEnemies() {
            var allEnemies = new NativeList<EnemyEntityData>(_spawnerConfig.EnemiesCount, Allocator.TempJob);
            Dependency = Entities.WithAll<Tag_Enemy>().ForEach((in Entity entity, in LocalToWorld ltw) => {
                allEnemies.Add(new EnemyEntityData { Entity = entity, Ltw = ltw });
            }).Schedule(Dependency);
            return allEnemies;
        }

        private struct EnemyEntityData {
            public Entity Entity;
            public LocalToWorld Ltw;
        }
    }
}