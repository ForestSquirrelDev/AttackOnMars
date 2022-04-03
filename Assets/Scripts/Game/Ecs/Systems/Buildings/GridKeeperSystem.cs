using Unity.Entities;
using UnityEngine;

namespace Game.Ecs.Systems {
    public partial class GridKeeperSystem : SystemBase {
        public BuildingGrid BuildingGrid;

        public void Init(Transform transform, int width, int height, float cellSize, int totalCellsCount) {
            BuildingGrid.Init(width, height, cellSize,
                transform.localToWorldMatrix, transform.worldToLocalMatrix, totalCellsCount);
        }
        
        protected override void OnUpdate() { }

        protected override void OnDestroy() {
            BuildingGrid.Dispose();
        }
    }
}