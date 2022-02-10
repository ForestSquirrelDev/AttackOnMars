using System.Collections.Generic;
using Game.Ecs.Utils;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Utils;

namespace Game.Ecs.Systems {
    public class PositioningQuadSystem : ComponentSystem {
        private Matrix4x4 transformCenter;
        private Matrix4x4 gridOrigin;
        private LocalToWorld localToWorld;
        private PositioningGrid positioningGrid;
        private List<Vector2Int> localGrid = new List<Vector2Int>();
        private List<Vector3> worldGrid = new List<Vector3>();
        private List<Vector2Int> globalGrid = new List<Vector2Int>();

        protected override void OnUpdate() {
            Entities.WithAll<PositioningQuadComponent>().ForEach((ref LocalToWorld localToWorld, ref PositioningQuadComponent positioningQuad) => {
                if (positioningQuad.inited) return;
                this.localToWorld = localToWorld;
                BoilerplateShortcuts.AxesWiseMatrix(ref transformCenter, localToWorld.Right, localToWorld.Up, localToWorld.Forward, localToWorld.Position);
                Debug.Log($"{transformCenter.GetColumn(0)}, {transformCenter.GetColumn(1)}, {transformCenter.GetColumn(2)}, {transformCenter.GetColumn(3)}");
                InitGrid();
                Debug.Log(GetOccupiedGlobalGridTiles().Count);
                GetOccupiedGlobalGridTiles().ForEach(tile => Debug.Log(tile));
                positioningQuad.inited = true;
            });
        }

        public List<Vector2Int> GetOccupiedGlobalGridTiles() {
            positioningGrid.GetGrid(localGrid);
            Debug.Log($"Local grid count: {localGrid.Count}");
            FillWorldGrid();
            FillGlobalGrid();
            return globalGrid;
        }
        
        private void InitGrid() {
            int2 size = CalculateGridSize();
            positioningGrid = new PositioningGrid(size.x, size.y);
        }

       public void OnDrawGizmos() {
            Gizmos.color = Color.green;
            foreach (var VARIABLE in worldGrid) {
                Gizmos.DrawSphere(VARIABLE, 0.5f);
            }
            
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(gridOrigin.GetColumn(3), gridOrigin.GetColumn(3) + gridOrigin.GetColumn(0));
            Gizmos.DrawLine(gridOrigin.GetColumn(3), gridOrigin.GetColumn(3) + gridOrigin.GetColumn(1));
            Gizmos.DrawLine(gridOrigin.GetColumn(3), gridOrigin.GetColumn(3) + gridOrigin.GetColumn(2));
            
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transformCenter.GetColumn(3), transformCenter.GetColumn(3) + transformCenter.GetColumn(0));
            Gizmos.DrawLine(transformCenter.GetColumn(3), transformCenter.GetColumn(3) + transformCenter.GetColumn(1));
            Gizmos.DrawLine(transformCenter.GetColumn(3), transformCenter.GetColumn(3) + transformCenter.GetColumn(2));
        }

        private void FillWorldGrid() {
            worldGrid.Clear();
            BoilerplateShortcuts.AxesWiseMatrixUnscaled(ref transformCenter, localToWorld.Right, localToWorld.Up, localToWorld.Forward, localToWorld.Position);
            foreach (var tile in localGrid) {
                Vector3 world = transformCenter.MultiplyPoint3x4(tile.ToVector3XZ() * BuildingGrid.CellSize);
                worldGrid.Add(world);
            }
        }
        
        private void FillGlobalGrid() {
            globalGrid.Clear();
            foreach (var tile in worldGrid) {
                globalGrid.Add(BuildingGrid.WorldToGridFloored(tile));
            }
        }
        
        private int2 CalculateGridSize() {
            float3 leftBottomCorner = new float3(0, 0, 0);
            float3 leftTopCorner = new float3(0, 0, 1f);
            float3 rightBottomCorner = new float3(1f, 0f, 0);
            
            float3 gridPosition = new float3(-1f, 0f, -1f);
            //gridPosition.x *= localToWorld.Right.ToVector4(1).magnitude;
            //gridPosition.z *= localToWorld.Forward.ToVector4(1).magnitude;
            Debug.Log($"X: {gridPosition.x}, Y: {gridPosition.y}");

            BoilerplateShortcuts.AxesWiseMatrix(ref gridOrigin, localToWorld.Right, localToWorld.Up, localToWorld.Forward, 
                transformCenter.MultiplyPoint3x4(gridPosition.ToVector4()));

            leftBottomCorner = transformCenter.MultiplyPoint3x4(leftBottomCorner);
            leftTopCorner = transformCenter.MultiplyPoint3x4(leftTopCorner);
            rightBottomCorner = transformCenter.MultiplyPoint3x4(rightBottomCorner);
        
            int2 leftBottomToGlobalGrid = BuildingGrid.WorldToGridFloored(leftBottomCorner).ToInt2();
            int2 leftTopToGlobalGrid = BuildingGrid.WorldToGridFloored(leftTopCorner).ToInt2();
            int2 rightBottomToGlobalGrid = BuildingGrid.WorldToGridFloored(rightBottomCorner).ToInt2();
            
            int width = Mathf.Abs(rightBottomToGlobalGrid.x - leftBottomToGlobalGrid.x);
            int height = Mathf.Abs(leftTopToGlobalGrid.y - leftBottomToGlobalGrid.y);
            Debug.Log($"x: {width}, y: {height}");
            return new int2(width, height);
        }
    }
}