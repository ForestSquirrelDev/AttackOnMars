using System;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Utils.Pathfinding {
    public static class FlowfieldUtility {
        public static int CalculateIndexFromWorld(float3 world, float3 origin, int2 gridSize, float cellSize) {
            var gridPos = ToGrid(new float3(world.x, 0, world.z), origin, cellSize);
            return CalculateIndexFromGrid(gridPos, gridSize);
        }

        public static int CalculateIndexFromGrid(float x, float z, int2 gridSize) {
            var max = Mathf.Max(gridSize.x, gridSize.y);
            var min = Mathf.Min(gridSize.x, gridSize.y);
            var index = (x * gridSize.x + z) + ((max - min) * x);
            return Convert.ToInt32(index);
        }

        public static int CalculateIndexFromGrid(int2 grid, int2 gridSize) {
            return CalculateIndexFromGrid(grid.x, grid.y, gridSize);
        }

        public static int2 ToGrid(float3 worldPos, float3 origin, float cellSize) {
            int x = Mathf.FloorToInt((worldPos.x - origin.x) / cellSize);
            int z = Mathf.FloorToInt((worldPos.z - origin.z) / cellSize);
            return new int2(x, z);
        }

        public static float3 ToWorld(int2 gridPos, float3 origin, float cellSize) {
            float x = (gridPos.x * cellSize + origin.x);
            float z = (gridPos.y * cellSize + origin.z);
            return new float3(x, origin.y, z);
        }

        public static float3 FindCellCenter(float3 worldPos, float cellSize) {
            return new float3(worldPos.x + cellSize / 2, worldPos.y, worldPos.z + cellSize / 2);
        }

        public static NativeArray<int2> GetNeighbourOffsets() {
            NativeArray<int2> offsets = new NativeArray<int2>(8, Allocator.Temp);
            offsets[0] = new int2(-1, 0);
            offsets[1] = new int2(1, 0);
            offsets[2] = new int2(0, 1);
            offsets[3] = new int2(0, -1);
            offsets[4] = new int2(-1, -1);
            offsets[5] = new int2(-1, 1);
            offsets[6] = new int2(1, -1);
            offsets[7] = new int2(1, 1);
            return offsets;
        }
        
        public static bool TileOutOfGrid(float2 gridPos, int2 gridSize) {
            return gridPos.x < 0 || gridPos.y < 0 || gridPos.x > gridSize.x - 1 || gridPos.y > gridSize.y - 1;
        }

        public static bool WorldOutOfGrid(float3 worldPos, FlowFieldRect parentCellRect) {
            return worldPos.x < parentCellRect.XMin || worldPos.x > parentCellRect.XMax || worldPos.z < parentCellRect.YMin || worldPos.z > parentCellRect.YMax;
        }

        public static bool WorldOutOfGrid(float3 worldPos, float3 origin, int2 gridSize, float cellSize) {
            float2 worldMax = new float2(gridSize.x * cellSize, gridSize.y * cellSize);
            return worldPos.x < origin.x || worldPos.x > worldMax.x || worldPos.z < origin.z || worldPos.z > worldMax.y;
        }
        
        public static bool IsOutOfRange(int collectionCount, int index) {
            return index < 0 || index >= collectionCount;
        }

        public static float SampleHeightRaycast(float3 worldPosition) {
            worldPosition.y += 10f;
            var hitUpDown = Physics.Raycast(worldPosition, Vector3.down, out var downHitInfo, 10f);
            if (hitUpDown) {
                return downHitInfo.point.y;
            }

            var hitDownUp = Physics.Raycast(worldPosition, Vector3.up, out var upHitInfo, 10f);
            if (hitDownUp) {
                return upHitInfo.point.y;
            }
            
            return -1;
        }
    }

    [Serializable]
    public struct FlowFieldRect {
        public float XMax => X + Width;
        public float XMin => X;
        public float YMax => Y + Height;
        public float YMin => Y;
        
        public float X;
        public float Y;
        public int Width;
        public int Height;

        public int2 Size => new int2(Width, Height);
    }
}