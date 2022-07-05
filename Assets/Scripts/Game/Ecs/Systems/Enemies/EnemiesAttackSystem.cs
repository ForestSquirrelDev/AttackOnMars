using Game.AddressableConfigs;
using Game.Ecs.Components;
using Game.Ecs.Components.Enemies;
using Game.Ecs.Components.Tags;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Transforms;
using Utils;
using RaycastHit = Unity.Physics.RaycastHit;

namespace Game.Ecs.Systems.Spawners {
    public partial class EnemiesAttackSystem : SystemBase {
        private NativeQueue<DamageEvent> _pendingEnemyAttacks;
        private EnemyStatsConfig _config;

        public void InjectConfigs(EnemyStatsConfig config) {
            _config = config;
        }

        protected override void OnCreate() {
            _pendingEnemyAttacks = new NativeQueue<DamageEvent>(Allocator.Persistent);
        }

        protected override void OnUpdate() {
            var world = World.DefaultGameObjectInjectionWorld.GetExistingSystem<Unity.Physics.Systems.BuildPhysicsWorld>().PhysicsWorld.CollisionWorld;
            var startOffset = _config.StartRaycastOffset;
            var raycastLength = _config.AttackRaycastLength;
            var attackCooldown = _config.AttackCooldown;
            var damage = _config.Damage;
            var delta = UnityEngine.Time.deltaTime;
            var damageEventsQueue = _pendingEnemyAttacks;

            Dependency = Entities.WithAll<Tag_Enemy>().ForEach((ref AttackCounterComponent attackCounter, in LocalToWorld ltw, in EnemyStateComponent enemyState, in Entity enemyEntity) => {
                if (enemyState.State != EnemyState.Attacking) 
                    return;
                if (attackCounter.Value > 0) {
                    attackCounter.Value -= delta;
                    return;
                }

                var start = ltw.Position + startOffset;
                var end = ltw.Position + ltw.Forward * raycastLength;
                var raycastInput = new RaycastInput {
                    Start = start, End = end, Filter = CollisionLayers.Enemy()
                };
                var raycast = world.CastRay(raycastInput, out RaycastHit closestHit);
                if (raycast) {
                    var damageEvent = new DamageEvent {
                        Amount = damage, Source = enemyEntity, Target = closestHit.Entity
                    };
                    damageEventsQueue.Enqueue(damageEvent);
                }
                
                attackCounter.Value = attackCooldown;
            }).Schedule(Dependency);

            var applyDamageEventsJob = new ApplyDamageEventsToBuildingsJob(damageEventsQueue, GetComponentDataFromEntity<BuildingHealthComponent>());
            Dependency = applyDamageEventsJob.Schedule(Dependency);
        }

        protected override void OnDestroy() {
            _pendingEnemyAttacks.Dispose();
        }

        [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
        private struct ApplyDamageEventsToBuildingsJob : IJob {
            private NativeQueue<DamageEvent> _damageEvents;
            private ComponentDataFromEntity<BuildingHealthComponent> _buildingsHealthCollection;

            public ApplyDamageEventsToBuildingsJob(NativeQueue<DamageEvent> damageEvents, ComponentDataFromEntity<BuildingHealthComponent> buildingsHealthCollection) {
                _damageEvents = damageEvents;
                _buildingsHealthCollection = buildingsHealthCollection;
            }
            
            public void Execute() {
                for (int i = _damageEvents.Count; i > 0; i--) {
                    var damageEvent = _damageEvents.Dequeue();
                    var buildingHealth = _buildingsHealthCollection[damageEvent.Target];
                    buildingHealth.CurrentHealth -= damageEvent.Amount;
                    _buildingsHealthCollection[damageEvent.Target] = buildingHealth;
                }
            }
        }
        
        private struct DamageEvent {
            public Entity Source;
            public Entity Target;
            public int Amount;
        }
    }
}