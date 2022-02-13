using Game.Ecs.Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Utils;

namespace Game.Ecs.Systems {
    public class BuildingGhostSystem : ComponentSystem {
        public bool CanSpawn { get; private set; }
        
        private Camera camera;
        private PositioningQuadSystem quadSystem;

        protected override void OnCreate() {
            camera = Camera.main;
            quadSystem = World.GetExistingSystem<PositioningQuadSystem>();
        }

        protected override void OnUpdate() {
            Entities.WithAll<Tag_BuildingGhost>().ForEach((ref Translation translation) => {
                float3 mouse = InputUtility.MouseToWorld(camera);
                float3 grid = BuildingGrid.WorldToGridCentered(mouse);
                translation.Value = grid;
                foreach (var tile in quadSystem.GetPositionsInGrid()) {
                    if (BuildingGrid.TileIsOccupied(new Vector2Int(tile.x, tile.y))) {
                        CanSpawn = false;
                        return;
                    }
                }
                CanSpawn = true;
            });
        }
    }
}