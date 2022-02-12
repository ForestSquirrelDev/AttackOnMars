using System.Collections.Generic;
using System.Diagnostics;
using Game.Ecs.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Utils;
using Debug = UnityEngine.Debug;

namespace Game.Ecs.Systems {
    public class PositioningQuadSystem : ComponentSystem {
        private Matrix4x4 transformCenter;
        private Matrix4x4 gridOrigin;
        private LocalToWorld localToWorld;
        private PositioningGrid positioningGrid;
        private List<Vector2Int> localGrid = new List<Vector2Int>();
        private List<Vector3> worldGrid = new List<Vector3>();
        private List<Vector2Int> globalGrid = new List<Vector2Int>();

        private Stopwatch sw = new Stopwatch();

        protected override void OnUpdate() {
            Entities.WithAll<PositioningQuadComponent>().ForEach((ref LocalToWorld localToWorld, ref PositioningQuadComponent positioningQuad) => {
                if (positioningQuad.inited) return;
                sw.Start();
                this.localToWorld = localToWorld;
                Matrix4x4Extensions.AxesWiseMatrix(ref transformCenter, localToWorld.Right, localToWorld.Forward, localToWorld.Up, localToWorld.Position);
                InitGrid();
                GetOccupiedGlobalGridTiles();
                positioningQuad.inited = true;
                sw.Stop();
                Debug.Log($"Nanoseconds: {StopwatchExtensions.ToMetricTime(sw.ElapsedTicks, StopwatchExtensions.TimeUnit.Nanoseconds)}, milliseconds: " +
                          $"{StopwatchExtensions.ToMetricTime(sw.ElapsedTicks, StopwatchExtensions.TimeUnit.Milliseconds)}");
                sw.Reset();
            });
        }

        public List<Vector2Int> GetOccupiedGlobalGridTiles() {
            positioningGrid.GetGrid(localGrid);
            FillWorldGrid();
            FillGlobalGrid();
            return globalGrid;
        }
        
        private void InitGrid() {
            int2 size = CalculateGridSize();
            Debug.Log(size);
            ConstructGridOriginMatrix(new float3(-0.5f, 0f, -0.5f));
            positioningGrid = new PositioningGrid(size.x, size.y);
        }
        
        [Conditional("UNITY_EDITOR")]
        public void OnDrawGizmos() {
            Gizmos.color = Color.green;
            foreach (var v in worldGrid) {
                Gizmos.DrawSphere(BuildingGrid.WorldToGridCentered(v), 0.5f);
            }
            Gizmos.color = Color.red;
            Gizmos.DrawLine(gridOrigin.GetColumn(3), gridOrigin.GetColumn(3) + gridOrigin.GetColumn(0));
            Gizmos.DrawLine(transformCenter.GetColumn(3), transformCenter.GetColumn(3) + transformCenter.GetColumn(0));
            Gizmos.color = Color.green;
            Gizmos.DrawLine(gridOrigin.GetColumn(3), gridOrigin.GetColumn(3) + gridOrigin.GetColumn(1));
            Gizmos.DrawLine(transformCenter.GetColumn(3), transformCenter.GetColumn(3) + transformCenter.GetColumn(1));
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(gridOrigin.GetColumn(3), gridOrigin.GetColumn(3) + gridOrigin.GetColumn(2));
            Gizmos.DrawLine(transformCenter.GetColumn(3), transformCenter.GetColumn(3) + transformCenter.GetColumn(2));
            
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(transformCenter.MultiplyPoint3x4(new Vector3(-.5f, 0, .5f)), .5f);
        }

        private void FillWorldGrid() {
            worldGrid.Clear();
            Matrix4x4Extensions.ToUnitScale(ref gridOrigin);
            foreach (var tile in localGrid) {
                Vector3 world = gridOrigin.MultiplyPoint3x4(tile.ToVector3XZ() * BuildingGrid.CellSize);
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
            float3 leftBottomCorner = new float3(-0.5f, 0f, -0.5f);
            float3 leftTopCorner = new float3(-0.5f, 0, 0.5f);
            float3 rightBottomCorner = new float3(0.5f, 0f, -0.5f);

            leftBottomCorner = transformCenter.MultiplyPoint3x4(leftBottomCorner);
            leftTopCorner = transformCenter.MultiplyPoint3x4(leftTopCorner);
            rightBottomCorner = transformCenter.MultiplyPoint3x4(rightBottomCorner);

            int2 leftBottomToGlobalGrid = BuildingGrid.WorldToGridCeiled(leftBottomCorner).ToInt2();
            int2 leftTopToGlobalGrid = BuildingGrid.WorldToGridCeiled(leftTopCorner).ToInt2();
            int2 rightBottomToGlobalGrid = BuildingGrid.WorldToGridCeiled(rightBottomCorner).ToInt2();
            
            int width = math.clamp(math.abs(rightBottomToGlobalGrid.x - leftBottomToGlobalGrid.x) + 1, 1, int.MaxValue);
            int height = math.clamp(math.abs(leftTopToGlobalGrid.y - leftBottomToGlobalGrid.y) + 1, 1, int.MaxValue);

            return new int2(width, height);
        }
        
        private void ConstructGridOriginMatrix(float3 gridPosition) {
            Matrix4x4Extensions.AxesWiseMatrix(ref gridOrigin, localToWorld.Right, localToWorld.Forward, localToWorld.Up,
                transformCenter.MultiplyPoint3x4(gridPosition.ToVector4()));
        }
    }
}