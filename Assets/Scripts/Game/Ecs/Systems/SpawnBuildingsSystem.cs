using Game.Ecs.Components;
using Game.Ecs.Components.BufferElements;
using Game.Ecs.Containers;
using Game.Ecs.Monobehaviours;
using Unity.Entities;
using UnityEngine;
using Utils;
using static Game.InstantiatedBuildingsContainer;

namespace Game.Ecs.Systems {
    public class SpawnBuildingsSystem : ComponentSystem {
        private Entity buildingGhost;
        private Entity buildingGhostQuad;
        private BuildingGhostSystem ghostSystem;
        private int clicksCount;
        private Camera camera;
        
        protected override void OnCreate() {
            camera = Camera.main;
            ghostSystem = World.GetExistingSystem<BuildingGhostSystem>();
        }

        protected override void OnUpdate() {
            if (!Input.GetMouseButtonDown(0)) return;
            if (clicksCount == 0) {
                SpawnGhost();
                clicksCount++;
            } else {
                if (!TrySpawnBuilding()) return;
                Reset();
            }
        }
        
        private void SpawnGhost() {
            buildingGhost = EntityManager.Instantiate(ConvertedEntitiesContainer.entities[BuildingType.Turret].ghost);
            buildingGhostQuad = buildingGhost.ReverseFindEntityWithComponent<PositioningQuadComponent>(EntityManager);
            EntityManager.AddBuffer<Int2BufferElement>(buildingGhostQuad);
        }
        
        private bool TrySpawnBuilding() {
            if (!ghostSystem.CanSpawn) return false;
            if (!BuildingGridInstantiater.InstantiateOnGrid(InputUtility.MouseToWorld(camera),
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
            EntityManager.DestroyEntity(buildingGhost);
            EntityManager.DestroyEntity(buildingGhostQuad);
            clicksCount = 0;
        }
    }
}