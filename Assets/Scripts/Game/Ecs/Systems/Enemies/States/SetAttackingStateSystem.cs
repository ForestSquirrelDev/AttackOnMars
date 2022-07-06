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
    public partial class SetAttackingStateSystem : SystemBase {
        private EnemyStatsConfig _config;

        protected override void OnCreate() {
            _config = ConfigsLoader.Get<EnemyStatsConfig>(AddressablesConsts.DefaultEnemyStatsConfig);
        }

        protected override void OnUpdate() {
            var raycastHeight = _config.RaycastHeight;
            var raycastLength = _config.RaycastLength;
            var world = World.DefaultGameObjectInjectionWorld.GetExistingSystem<Unity.Physics.Systems.BuildPhysicsWorld>().PhysicsWorld.CollisionWorld;
            var raycastCooldown = _config.RaycastCooldown;
            var delta = UnityEngine.Time.deltaTime;
            
            Dependency = Entities.WithAll<Tag_Enemy>().ForEach((ref EnemyStateComponent enemyState, ref Translation translation, in BestEnemyDirectionComponent bestDirection, in LocalToWorld ltw) => {
                if (enemyState.State != EnemyState.ReadyToAttack) return;
                if (enemyState.ArrivedRaycastCheckCounter > 0) {
                    enemyState.ArrivedRaycastCheckCounter -= delta;
                    return;
                }
                enemyState.ArrivedRaycastCheckCounter = raycastCooldown;
                var start = ltw.Position + new float3(0, raycastHeight, 0);
                var raycast = new RaycastInput {
                    Start = start, Filter = CollisionLayers.Enemy(), End = ltw.Position + ltw.Forward * raycastLength
                };
                var ray = world.CastRay(raycast);
                
                if (ray) {
                    enemyState.State = EnemyState.Attacking;
                }
            }).Schedule(Dependency);
        }
    }
}