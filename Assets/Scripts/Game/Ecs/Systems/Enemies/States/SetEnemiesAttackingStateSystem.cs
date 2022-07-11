using Game.AddressableConfigs;
using Game.Ecs.Components.Enemies;
using Game.Ecs.Components.Pathfinding;
using Game.Ecs.Components.Tags;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using Utils;

namespace Game.Ecs.Systems.Spawners {
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    public partial class SetEnemiesAttackingStateSystem : SystemBase {
        private EnemyStatsConfig _config;

        protected override void OnCreate() {
            _config = AddressablesLoader.Get<EnemyStatsConfig>(AddressablesConsts.DefaultEnemyStatsConfig);
        }

        protected override void OnUpdate() {
            var raycastHeight = _config.RaycastHeight;
            var raycastLength = _config.RaycastLength;
            var world = World.DefaultGameObjectInjectionWorld.GetExistingSystem<Unity.Physics.Systems.BuildPhysicsWorld>().PhysicsWorld.CollisionWorld;
            var raycastCooldown = _config.RaycastCooldown;
            var delta = UnityEngine.Time.deltaTime;
            
            Dependency = Entities.WithAll<Tag_Enemy>().ForEach((ref EnemyStateComponent enemyState, ref HumanBaseDetectionTickCounterComponent detectionCounter,
                ref Translation translation, in BestEnemyGridDirectionComponent bestDirection, in LocalToWorld ltw) => {
                if (enemyState.Value != EnemyState.ReadyToAttack) return;
                if (detectionCounter.Value > 0) {
                    detectionCounter.Value -= delta;
                    return;
                }
                detectionCounter.Value = raycastCooldown;
                var start = ltw.Position + new float3(0, raycastHeight, 0);
                var raycast = new RaycastInput {
                    Start = start, Filter = CollisionLayers.Enemy(), End = ltw.Position + ltw.Forward * raycastLength
                };
                var ray = world.CastRay(raycast);
                
                if (ray) {
                    enemyState.Value = EnemyState.Attacking;
                }
            }).Schedule(Dependency);
        }
    }
}