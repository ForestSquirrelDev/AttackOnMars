using Unity.Entities;
using UnityEngine;

namespace Game.Ecs.Systems {
    public class GridKeeperSystem : SystemBase {
        public BuildingGrid buildingGrid;

        public void Init(Transform transform, int width, int height, float cellSize, int totalCellsCount) {
            buildingGrid = new BuildingGrid();
            buildingGrid.Init(width, height, cellSize,
                transform.localToWorldMatrix, transform.worldToLocalMatrix, totalCellsCount);
        }
        
        protected override void OnUpdate() {

        }

        protected override void OnDestroy() {
            buildingGrid.Dispose();
        }
    }
}