using Game.AddressableConfigs;
using Game.Ecs.Components;
using Game.Ecs.Components.Enemies;
using Game.Ecs.Components.Tags;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using Utils;

namespace Game.Ecs.Systems.Spawners {
    public partial class EnemiesAttackSystem : SystemBase {
        private EnemyStatsConfig _config;
        private EndSimulationEntityCommandBufferSystem _ecb;

        public void InjectConfigs(EnemyStatsConfig config) {
            _config = config;
            _ecb = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }
        
		protected override void OnUpdate() {
            var world = World.DefaultGameObjectInjectionWorld.GetExistingSystem<Unity.Physics.Systems.BuildPhysicsWorld>().PhysicsWorld.CollisionWorld;
            var boxSize = _config.BoxCastSize;
            var offset = _config.BoxCastOffset;
            var maxDistance = _config.BoxCastMaxDistance;
            var attackCooldown = _config.AttackCooldown;
            var damage = _config.Damage;
            var delta = UnityEngine.Time.deltaTime;
            var ecb = _ecb.CreateCommandBuffer();
            
            Dependency = Entities.WithAll<Tag_Enemy>().ForEach((ref AttackCounterComponent attackCounter, in LocalToWorld ltw, in EnemyStateComponent enemyState) => {
                if (enemyState.State != EnemyState.Attacking) 
                    return;
                if (attackCounter.Value > 0) {
                    attackCounter.Value -= delta;
                    return;
                }
                
                var cast = world.BoxCast(ltw.Position + offset, ltw.Rotation, boxSize, ltw.Forward, maxDistance, out var hitInfo, CollisionLayers.Enemy());
                if (cast) {
                    Debug.Log($"Entity: {hitInfo.Entity}. Collider: {hitInfo.ColliderKey}. Pos: {hitInfo.Position}");
                    var component = GetComponent<HealthComponent>(hitInfo.Entity);
                    component.CurrentHealth -= damage;
                    ecb.SetComponent(hitInfo.Entity, component);
                }
                
                attackCounter.Value = attackCooldown;
            }).Schedule(Dependency);
            
            _ecb.AddJobHandleForProducer(Dependency);
        }
    }
}