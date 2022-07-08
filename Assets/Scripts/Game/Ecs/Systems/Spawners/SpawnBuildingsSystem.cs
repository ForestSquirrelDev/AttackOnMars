using Game.Ecs.Components;
using Game.Ecs.Containers;
using Shared;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using Utils;

namespace Game.Ecs.Systems {
    [UpdateBefore(typeof(SpawnBuildingGhostSystem))]
    public partial class SpawnBuildingsSystem : SystemBase {
        private GridKeeperSystem _gridKeeperSystem;
        private Camera _camera;
        private EndSimulationEntityCommandBufferSystem _endSimulationEcb;
        
        protected override void OnCreate() {
            _gridKeeperSystem = World.GetOrCreateSystem<GridKeeperSystem>();
            _camera = Camera.main;
            _endSimulationEcb = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate() {
            if (!Input.GetMouseButtonDown(0)) return;
            EntityCommandBuffer ecb = _endSimulationEcb.CreateCommandBuffer();
            Entities.WithAll<BuildingGhostPositioningQuadComponent>().ForEach((in Parent parent, in BuildingGhostPositioningQuadComponent quadComponent) => {
                if (!quadComponent.AvailableForPlacement) return;
                
                TrySpawnBuilding(EntityManager.GetComponentData<BuildingGhostComponent>(parent.Value).BuildingType);
                ecb.DestroyEntity(parent.Value);
                SetSingleton(new SpawningGhostSingletonData{CanSpawn = true});
            }).WithStructuralChanges().WithoutBurst().Run();
        }
        
        private void TrySpawnBuilding(BuildingType type) {
            _gridKeeperSystem.BuildingGrid.InstantiateOnGrid(InputUtility.MouseToWorld(_camera),
                ConvertedEntitiesContainer.s_Entities[type].Building, EntityManager, out _);
        }
    }
}
