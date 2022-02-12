using Game.Ecs.Monobehaviours;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using Utils;

namespace Game.Ecs.Systems {
    public class SpawnBuildingsSystem : ComponentSystem {
        private Entity buildingGhost;
        private int clicksCount;
        private Camera camera;
        
        protected override void OnStartRunning() {
            base.OnStartRunning();
            camera = Camera.main;
        }
        
        protected override void OnUpdate() {
            if (!Input.GetMouseButtonDown(0)) return;
            if (clicksCount == 0) {
                SpawnGhost();
            } else {
                if (!TrySpawnBuilding()) return;
                Reset();
            }
        }
        
        private bool TrySpawnBuilding() {
            if (!BuildingGridInstantiater.InstantiateOnGrid(InputUtility.MouseToWorld(camera),
                ConvertedEntitiesContainer.entities[BuildingType.Turret].building, EntityManager, out Entity entity)) return false;
            InstantiatedBuildingsContainer.buildings.Add(entity);
            return true;
        }

        private void SpawnGhost() {
            buildingGhost = EntityManager.Instantiate(ConvertedEntitiesContainer.entities[BuildingType.Turret].ghost);
            EntityManager.SetComponentData(buildingGhost, new Translation());
            clicksCount++;
        }

        private void Reset() {
            EntityManager.DestroyEntity(buildingGhost);
            clicksCount = 0;
        }
    }
}