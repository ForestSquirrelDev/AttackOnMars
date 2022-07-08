using Game.Ecs.Systems.Pathfinding;
using Unity.Entities;
using UnityEngine;

namespace Game.Ecs.Systems {
    public partial class GridKeeperSystem : SystemBase {
        public BuildingGrid BuildingGrid;
        public DependenciesScheduler DependenciesScheduler;

        protected override void OnCreate() {
            DependenciesScheduler = new DependenciesScheduler();
        }

        public void Init(Transform transform, int width, int height, float cellSize, int totalCellsCount) {
            BuildingGrid.Init(width, height, cellSize,
                transform.localToWorldMatrix, transform.worldToLocalMatrix, totalCellsCount);
        }

        protected override void OnUpdate() {
            DependenciesScheduler.OnUpdate();
        }

        protected override void OnDestroy() {
            BuildingGrid.Dispose();
            DependenciesScheduler.Dispose();
        }
    }
}