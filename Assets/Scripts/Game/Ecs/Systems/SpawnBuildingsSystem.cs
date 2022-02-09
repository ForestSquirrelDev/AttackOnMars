using Game.Ecs.Components;
using Game.Ecs.Monobehaviours;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Game.Ecs.Systems {
    public class SpawnBuildingsSystem : ComponentSystem {
        protected override void OnUpdate() {
            if (Input.GetMouseButtonDown(0)) {
                Entity spawnedEntity = EntityManager.Instantiate(ConvertedEntitiesStorage.entities[BuildingType.Turret].ghost);
                EntityManager.SetComponentData(spawnedEntity, new Translation());
                EntityManager.SetComponentData(spawnedEntity, new LocalToWorld());
            }
        }
        protected override void OnStartRunning() {
            Debug.Log("On start running");
            base.OnStartRunning();
        }
    }
}