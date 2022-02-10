using Game.Ecs.Components;
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
            return BuildingGridInstantiater.InstantiateOnGrid(InputUtility.MouseToWorld(camera),
                ConvertedEntitiesStorage.entities[BuildingType.Turret].building, EntityManager);
        }

        private void SpawnGhost() {
            buildingGhost = EntityManager.Instantiate(ConvertedEntitiesStorage.entities[BuildingType.Turret].ghost);
            EntityManager.SetComponentData(buildingGhost, new Translation());
            clicksCount++;
        }

        private void Reset() {
            EntityManager.DestroyEntity(buildingGhost);
            clicksCount = 0;
        }
    }
}