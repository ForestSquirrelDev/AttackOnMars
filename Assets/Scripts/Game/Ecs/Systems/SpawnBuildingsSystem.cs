using Game.Ecs.BlobAssets;
using Game.Ecs.Components;
using Game.Ecs.Components.BufferElements;
using Game.Ecs.Containers;
using Unity.Entities;
using UnityEngine;
using Utils;
using static Game.InstantiatedBuildingsContainer;

namespace Game.Ecs.Systems {
    public class SpawnBuildingsSystem : ComponentSystem {
        private Entity _buildingGhost;
        private Entity _buildingGhostQuad;
        private BuildingGhostSystem _ghostSystem;
        private int _clicksCount;
        private Camera _camera;
        private GridKeeperSystem _gridKeeperSystem;
        
        protected override void OnCreate() {
            //RequireSingletonForUpdate<ConvertedBuildingsData>();
            _camera = Camera.main;
            _ghostSystem = World.GetExistingSystem<BuildingGhostSystem>();
            _gridKeeperSystem = World.GetOrCreateSystem<GridKeeperSystem>();
        }

        protected override void OnUpdate() {
            if (!Input.GetMouseButtonDown(0)) return;
            if (_clicksCount == 0) {
                SpawnGhost();
                _clicksCount++;
            } else {
                if (!TrySpawnBuilding()) return;
                Reset();
            }
        }
        
        private void SpawnGhost() {
            _buildingGhost = EntityManager.Instantiate(ConvertedEntitiesContainer.entities[BuildingType.Turret].ghost);
            _buildingGhostQuad = _buildingGhost.ReverseFindEntityWithComponent<PositioningQuadComponent>(EntityManager);
            EntityManager.AddBuffer<Int2BufferElement>(_buildingGhostQuad);
        }
        
        private bool TrySpawnBuilding() {
            if (!_ghostSystem.CanSpawn) return false;
            if (!_gridKeeperSystem.buildingGrid.InstantiateOnGrid(InputUtility.MouseToWorld(_camera),
                ConvertedEntitiesContainer.entities[BuildingType.Turret].building, EntityManager, out Entity building)) return false;
            Entity positioningQuad = building.ReverseFindEntityWithComponent<PositioningQuadComponent>(EntityManager);
            EntityManager.SetComponentData(building, new BuildingComponent {positioningQuad = EntityManager.GetComponentData<PositioningQuadComponent>(positioningQuad)});
            EntityManager.AddBuffer<Int2BufferElement>(positioningQuad);
            SpawnedBuilding spawnedBuilding = new SpawnedBuilding 
                { buildingRoot = building, positioningQuad = positioningQuad };
            buildings.Add(spawnedBuilding);
            return true;
        }

        private void Reset() {
            EntityManager.DestroyEntity(_buildingGhost);
            EntityManager.DestroyEntity(_buildingGhostQuad);
            _clicksCount = 0;
        }
    }
}