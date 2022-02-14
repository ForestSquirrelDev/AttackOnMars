using System;
using Game;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;
using Utils;

namespace Editor.Tests {
    public class BasicCalculationsTest : MonoBehaviour {
    public Matrix4x4 transformCenter;
        public int loops = 10000;

        private void Update() {
            Profiler.BeginSample("Some calcs");
            for (int i = loops; i > 0; i--)
                CalculateGridSize();
            Profiler.EndSample();
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
    }
}