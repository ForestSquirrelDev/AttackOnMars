using Game.Ecs.Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Utils;

namespace Game.Ecs.Systems {
    public class BuildingGhostSystem : ComponentSystem {
        private Camera camera;

        protected override void OnStartRunning() {
            camera = Camera.main;
        }

        protected override void OnUpdate() {
            Entities.WithAll<Tag_BuildingGhost>().ForEach((ref Translation translation) => {
                float3 mouse = InputUtility.MouseToWorld(camera);
                float3 grid = BuildingGrid.WorldToGridCentered(mouse);
                translation.Value = grid;
            });
        }
    }
}