using Game.Ecs.Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Utils;

namespace Game.Ecs.Systems {
    public partial class BuildingGhostSystem : SystemBase {
        private Camera _camera;
        private PositioningQuadSystem _quadSystem;
        private GridKeeperSystem _gridKeeperSystem;

        protected override void OnCreate() {
            _camera = Camera.main;
            _quadSystem = World.GetExistingSystem<PositioningQuadSystem>();
            _gridKeeperSystem = World.GetOrCreateSystem<GridKeeperSystem>();
        }

        protected override void OnUpdate() {
            Entities.WithAll<Tag_BuildingGhost>().ForEach((ref Translation translation) => {
                float3 mouse = InputUtility.MouseToWorld(_camera);
                float3 grid = _gridKeeperSystem.BuildingGrid.WorldToGridCentered(mouse);
                translation.Value = grid;
                foreach (var tile in _quadSystem.GetPositionsInGrid()) {
                    if (_gridKeeperSystem.BuildingGrid.TileIsOccupied(new Vector2Int(tile.x, tile.y))) {
                        return;
                    }
                }
            }).WithoutBurst().Run();
        }
    }
}