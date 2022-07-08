using Game.Ecs.Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Utils;

namespace Game.Ecs.Systems {
    public partial class SetBuildingGhostPositionSystem : SystemBase {
        private Camera _camera;
        private GridKeeperSystem _gridKeeperSystem;

        protected override void OnCreate() {
            _camera = Camera.main;
            _gridKeeperSystem = World.GetOrCreateSystem<GridKeeperSystem>();
        }

        protected override void OnUpdate() {
            Entities.WithAll<Tag_BuildingGhost>().ForEach((ref Translation translation) => {
                float3 mouse = InputUtility.MouseToWorld(_camera);
                float3 grid = _gridKeeperSystem.BuildingGrid.WorldToGridCentered(mouse);
                translation.Value = grid;
            }).WithoutBurst().Run();
        }
    }
}