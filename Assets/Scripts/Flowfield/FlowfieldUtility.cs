using System;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Flowfield {
    public static class FlowfieldUtility {
        public static int CalculateIndexFromWorld(float worldX, float worldZ, float3 origin, int2 gridSize, float cellSize) {
            var gridPos = ToGrid(new float3(worldX, 0, worldZ), origin, cellSize);
            var max = Mathf.Max(gridSize.x, gridSize.y);
            var min = Mathf.Min(gridSize.x, gridSize.y);
            var index = (gridPos.x * gridSize.x + gridPos.y) + ((max - min) * gridPos.x);
          //  Debug.Log($"Calculate index. Gridpos: {gridPos}. Max: {max}. Min: {min}.Index: {index}.");
            return Convert.ToInt32(index);
        }

        public static int CalculateIndexFromGrid(float x, float z, int2 gridSize) {
            var max = Mathf.Max(gridSize.x, gridSize.y);
            var min = Mathf.Min(gridSize.x, gridSize.y);
            var index = (x * gridSize.x + z) + ((max - min) * x);
//            Debug.Log($"x/y: {x}/{z}. Min: {min}. Max: {max}. Index: {index}");
            return Convert.ToInt32(index);
        }

        public static int CalculateIndexFromGrid(int2 grid, int2 gridSize) {
            return CalculateIndexFromGrid(grid.x, grid.y, gridSize);
        }

        public static int2 ToGrid(float3 worldPos, float3 origin, float cellSize) {
            var localPosition = worldPos - origin;
            int x = Mathf.FloorToInt(localPosition.x / cellSize);
            int z = Mathf.FloorToInt(localPosition.z / cellSize);
            return new int2(x, z);
        }

        public static float3 ToWorld(int2 gridPos, float3 origin, float cellSize) {
            return new float3(gridPos.x * cellSize + origin.x, origin.y, gridPos.y * cellSize + origin.z);
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
    }
}