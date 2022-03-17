using Game.Ecs.Components;
using Game.Ecs.Components.BufferElements;
using Game.Ecs.Containers;
using Shared;
using Unity.Entities;
using UnityEngine;
using Utils;

namespace Game.Ecs.Systems {
    public partial class SpawnBuildingGhostSystem : SystemBase {
        private Entity _buildingGhost;
        private Entity _buildingGhostQuad;

        protected override void OnUpdate() {
            if (!Input.GetMouseButtonDown(0)) return;
            SpawnGhost();
        }

        private void SpawnGhost() {
            _buildingGhost = EntityManager.Instantiate(ConvertedEntitiesContainer.Entities[BuildingType.Turret].ghost);
            _buildingGhostQuad = _buildingGhost.ReverseFindEntityWithComponent<PositioningQuadComponent>(EntityManager);
            EntityManager.AddBuffer<Int2BufferElement>(_buildingGhostQuad);
        }
    }
}
