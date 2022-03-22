using Game.Ecs.Components;
using Game.Ecs.Components.Tags;
using Game.Ecs.Containers;
using Shared;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using Utils;

namespace Game.Ecs.Systems {
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    public partial class SpawnBuildingsSystem : SystemBase {
        private GridKeeperSystem _gridKeeperSystem;
        private Camera _camera;
        private EndSimulationEntityCommandBufferSystem _endSimulationEcb;
        
        protected override void OnStartRunning() {
            _gridKeeperSystem = World.GetOrCreateSystem<GridKeeperSystem>();
            _camera = Camera.main;
            _endSimulationEcb = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate() {
            EntityCommandBuffer ecb = _endSimulationEcb.CreateCommandBuffer();
            Entities.WithAll<Tag_AvailableForPlacementGhostQuad>().ForEach((ref Parent parent) => {
                if (!Input.GetMouseButtonDown(0)) return;
                TrySpawnBuilding(EntityManager.GetComponentData<BuildingGhostComponent>(parent.Value).BuildingType);
                ecb.DestroyEntity(parent.Value);
                SetSingleton(new SpawningGhostSingletonData{CanSpawn = true});
            }).WithStructuralChanges().WithoutBurst().Run();
        }
        
        private void TrySpawnBuilding(BuildingType type) {
            _gridKeeperSystem.BuildingGrid.InstantiateOnGrid(InputUtility.MouseToWorld(_camera),
                ConvertedEntitiesContainer.Entities[type].building, EntityManager, out _);
        }
    }
}
