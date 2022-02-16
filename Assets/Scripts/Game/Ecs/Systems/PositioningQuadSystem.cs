using System.Collections.Generic;
using System.Diagnostics;
using Game.Ecs.Components;
using Game.Ecs.Components.BufferElements;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Utils;

namespace Game.Ecs.Systems {
    public class PositioningQuadSystem : ComponentSystem {
        private Stopwatch sw = new Stopwatch();
        private Matrix4x4 transformCenter;
        private Matrix4x4 gridOrigin;
        private LocalToWorld localToWorld;
        private PositioningGrid positioningGrid;
        private List<Vector2Int> localGridTiles = new List<Vector2Int>();
        private List<Vector3> worldGridTiles = new List<Vector3>();
        private List<int2> globalGridTiles = new List<int2>();
        
        protected override void OnUpdate() {
            Entities.WithAll<Tag_BuildingGhostPositioningQuad>().ForEach((DynamicBuffer<Int2BufferElement> buffer, ref LocalToWorld localToWorld) => {
                SetPositionsInGrid(localToWorld, buffer);
            });
            Entities.WithAll<Tag_BuildingPositioningQuad>().ForEach((DynamicBuffer<Int2BufferElement> buffer, ref LocalToWorld localToWorld,
                ref PositioningQuadComponent positioningQuad, ref Parent parent) => {
                if (positioningQuad.inited) return;
                SetPositionsInGrid(localToWorld, buffer);
                BuildingGrid.AddBuildingToGrid(globalGridTiles, parent.Value);
                positioningQuad.inited = true;
            });
        }

        public List<int2> GetPositionsInGrid() => globalGridTiles;

        private void SetPositionsInGrid(LocalToWorld localToWorld, DynamicBuffer<Int2BufferElement> buffer) {
            this.localToWorld = localToWorld;
            Matrix4x4Extensions.AxesWiseMatrix(ref transformCenter, localToWorld.Right, localToWorld.Forward, localToWorld.Up, localToWorld.Position);
            InitGrid();
            GetOccupiedGlobalGridTiles();
            for (int i = 0; i < globalGridTiles.Count; i++) {
                if (buffer.Length >= globalGridTiles.Count) {
                    buffer[i] = new Int2BufferElement { value = globalGridTiles[i] };
                } else {
                    buffer.Add(new Int2BufferElement { value = globalGridTiles[i] });
                }
            }
        }

        private void GetOccupiedGlobalGridTiles() {
            positioningGrid.GetGrid(localGridTiles);
            FillWorldGrid();
            FillGlobalGrid();
        }
        
        private void InitGrid() {
            int2 size = CalculateGridSize();
            ConstructGridOriginMatrix(new float3(-0.5f, 0f, -0.5f));
            positioningGrid = new PositioningGrid(size.x, size.y);
        }
        
        private void FillWorldGrid() {
            worldGridTiles.Clear();
            Matrix4x4Extensions.ToUnitScale(ref gridOrigin);
            foreach (var tile in localGridTiles) {
                Vector3 world = gridOrigin.MultiplyPoint3x4(tile.ToVector3XZ() * BuildingGrid.CellSize);
                worldGridTiles.Add(world);
            }
        }
        
        private void FillGlobalGrid() {
            globalGridTiles.Clear();
            foreach (var tile in worldGridTiles) {
                globalGridTiles.Add(BuildingGrid.WorldToGridFloored(tile).ToInt2());
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
        
        [Conditional("UNITY_EDITOR")]
        public void OnDrawGizmos() {
            Gizmos.color = Color.green;
            foreach (var v in worldGridTiles) {
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
    }
}