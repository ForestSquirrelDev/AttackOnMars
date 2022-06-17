using Game.AddressableConfigs;
using Game.Ecs.Components.Enemies;
using Game.Ecs.Components.Pathfinding;
using Game.Ecs.Components.Tags;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

namespace Game.Ecs.Systems.Spawners {
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    public partial class SetAttackingStateSystem : SystemBase {
        private EnemyStatsConfig _config;

        public void InjectConfigs(EnemyStatsConfig config) {
            _config = config;
        }
        
		protected override void OnUpdate() {
            var xzSpeed = _config.XZMoveSpeed;
            var ySpeed = _config.YMoveSpeed;
            var world = World.DefaultGameObjectInjectionWorld.GetExistingSystem<Unity.Physics.Systems.BuildPhysicsWorld>().PhysicsWorld.CollisionWorld;
            Dependency = Entities.WithAll<Tag_Enemy>().ForEach((ref EnemyStateComponent enemyState, ref Translation translation, in BestEnemyDirectionComponent bestDirection, in LocalToWorld ltw) => {
                //Debug.Log($"OnBeforeCheck");
                if (enemyState.Value != EnemyState.ReadyToAttack) return;
                //Debug.Log($"OnAfterCheck");
                var start = ltw.Position + new float3(0, 1, 0);
                var raycast = new RaycastInput {
                    Start = start, Filter = CollisionFilter.Default, End = new float3(start.x, start.y, start.z + 1f)
                };
                var ray = world.CastRay(raycast, out var hito);
                //Debug.Log($"{hito.Entity}//{hito.Position}");
                if (ray) {
                    enemyState.Value = EnemyState.Attacking;
                } else {
                    var x = Mathf.MoveTowards(translation.Value.x, translation.Value.x + bestDirection.Value.x, xzSpeed);
                    var y = Mathf.MoveTowards(translation.Value.y, translation.Value.y + bestDirection.Value.y, ySpeed);
                    var z = Mathf.MoveTowards(translation.Value.z, translation.Value.x + bestDirection.Value.z, xzSpeed);
                    translation.Value = new float3(x, y, z);
                }
            }).Schedule(Dependency);
        }
    }
}