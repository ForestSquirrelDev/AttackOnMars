using Game.AddressableConfigs;
using Game.Ecs.Components;
using Game.Ecs.Components.Buildings;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Game.Ecs.Systems.Spawners {
    public partial class DestroyHumanBaseSystem : SystemBase {
        private EndSimulationEntityCommandBufferSystem _ecbSystem;
        private GameObject _explosionParticles;

        protected override void OnCreate() {
            _ecbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            _explosionParticles = AddressablesLoader.Get<GameObject>(AddressablesConsts.HumanBaseExplosionParticles);
            RequireSingletonForUpdate<MainHumanBaseSingletonComponent>();
        }

        protected override void OnUpdate() {
            var ecb = _ecbSystem.CreateCommandBuffer();
            Entities.WithAll<Tag_MainHumanBase>().ForEach((in BuildingHealthComponent health, in LocalToWorld ltw, in Entity entity) => {
                if (health.Value <= 0) {
                    EntityManager.DestroyEntity(entity);
                    Object.Instantiate(_explosionParticles, ltw.Position, Quaternion.identity);
                }
            }).WithStructuralChanges().WithoutBurst().Run();
        }
    }
}