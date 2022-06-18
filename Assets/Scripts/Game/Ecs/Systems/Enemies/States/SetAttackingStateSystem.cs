using Game.AddressableConfigs;
using Game.Ecs.Components.Enemies;
using Game.Ecs.Components.Pathfinding;
using Game.Ecs.Components.Tags;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using Utils;

namespace Game.Ecs.Systems.Spawners {
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    public partial class SetAttackingStateSystem : SystemBase {
        private EnemyStatsConfig _config;

        public void InjectConfigs(EnemyStatsConfig config) {
            _config = config;
        }
        
		protected override void OnUpdate() {
            var raycastHeight = _config.RaycastHeight;
            var raycastLength = _config.RAycastLength;
            var world = World.DefaultGameObjectInjectionWorld.GetExistingSystem<Unity.Physics.Systems.BuildPhysicsWorld>().PhysicsWorld.CollisionWorld;
            
            Dependency = Entities.WithAll<Tag_Enemy>().ForEach((ref EnemyStateComponent enemyState, ref Translation translation, in BestEnemyDirectionComponent bestDirection, in LocalToWorld ltw) => {
                if (enemyState.Value != EnemyState.ReadyToAttack) return;
                var start = ltw.Position + new float3(0, raycastHeight, 0);
                var raycast = new RaycastInput {
                    Start = start, Filter = CollisionLayers.Enemy(), End = ltw.Position + ltw.Forward * raycastLength
                };
                var ray = world.CastRay(raycast, out var hito);
                //Debug.Log($"{hito.Entity}//{hito.Position}");
                if (ray) {
                    enemyState.Value = EnemyState.Attacking;
                }
            }).Schedule(Dependency);
        }
    }
}