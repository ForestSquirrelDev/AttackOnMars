using Game.Ecs.Components;
using Game.Ecs.Components.BufferElements;
using Game.Ecs.Containers;
using Shared;
using Unity.Entities;
using UnityEngine;
using Utils;

namespace Game.Ecs.Systems {
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial class SpawnBuildingGhostSystem : SystemBase {
        protected override void OnUpdate() {
            if (!Input.GetMouseButtonDown(0)) return;
            if (GetSingleton<SpawningGhostSingletonData>().CanSpawn) SpawnGhost();
        }

        private void SpawnGhost() {
            var buildingGhost = EntityManager.Instantiate(ConvertedEntitiesContainer.Entities[BuildingType.Turret].ghost);
            var buildingGhostQuad = buildingGhost.ReverseFindEntityWithComponent<PositioningQuadComponent>(EntityManager);
            EntityManager.AddBuffer<Int2BufferElement>(buildingGhostQuad);
            SetSingleton(new SpawningGhostSingletonData{CanSpawn = false});
        }
    }
}