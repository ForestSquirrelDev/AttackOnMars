using Game.Ecs.Components.Enemies;
using Game.Ecs.Components.Tags;
using Unity.Entities;

namespace Game.Ecs.Systems.Spawners {
    public partial class DestroyDeadEnemiesSystem : SystemBase {
        private EndSimulationEntityCommandBufferSystem _ecbSystem;

        protected override void OnCreate() {
            _ecbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate() {
            var ecb = _ecbSystem.CreateCommandBuffer();

            Dependency = Entities.WithAll<Tag_Enemy>().ForEach((in Entity entity, in EnemyHealthComponent health) => {
                if (health.Value <= 0) {
                    ecb.DestroyEntity(entity);
                }
            }).Schedule(Dependency);
            
            _ecbSystem.AddJobHandleForProducer(Dependency);
        }
    }
}