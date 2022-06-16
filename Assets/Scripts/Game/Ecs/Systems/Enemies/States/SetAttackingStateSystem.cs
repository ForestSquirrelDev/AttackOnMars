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
        private const float _xzMoveDelta = 0.1f;
        private const float _yMoveDelta = 0.2f;
        
		protected override void OnUpdate() {
            Debug.Log($"OnUpdate");
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
                    var x = Mathf.MoveTowards(translation.Value.x, translation.Value.x + bestDirection.Value.x, _xzMoveDelta);
                    var y = Mathf.MoveTowards(translation.Value.y, bestDirection.Value.y, _yMoveDelta);
                    var z = Mathf.MoveTowards(translation.Value.z, translation.Value.x + bestDirection.Value.z, _xzMoveDelta);
                    translation.Value = new float3(x, y, z);
                }
            }).Schedule(Dependency);
        }
    }
}