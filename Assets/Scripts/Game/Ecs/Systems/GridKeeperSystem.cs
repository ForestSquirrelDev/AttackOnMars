using Game.Ecs.Systems.Bridge.GlobalGrid;
using Unity.Entities;
using UnityEngine;

namespace Game.Ecs.Systems {
    public class GridKeeperSystem : SystemBase {
        public BuildingGrid buildingGrid;

        public void Init(GridManager gridManager) {
            buildingGrid = new BuildingGrid();
            Transform transform = gridManager.transform;
            buildingGrid.Init(gridManager.Width, gridManager.Height, gridManager.CellSize,
                transform.localToWorldMatrix, transform.worldToLocalMatrix, gridManager.TotalCellsCount);
        }
        
        protected override void OnUpdate() {
            
        }

        protected override void OnDestroy() {
            buildingGrid.Dispose();
        }
    }
}