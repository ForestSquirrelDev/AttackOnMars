using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Utils;
using Utils.Logger;

namespace Game.Ecs.Systems {
    public class JobifiedPositioningQuadSystem : JobComponentSystem {
        private Matrix4x4 transformCenter;
        [ReadOnly]
        private NativeArray<int2> result;
        
        protected override void OnCreate() {
            result = new NativeArray<int2>(10000, Allocator.Persistent);
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            if (Input.GetKeyDown(KeyCode.H)) {
                int i = 0;
                foreach (var tile in result) {
                    i++;
                    CustomLogger.Log(tile.ToString(), LogOptions.Joint, i.ToString());
                }
            }
            UpdatePositionsJob job = new UpdatePositionsJob {result = result, transformCenter = transformCenter};
            JobHandle jobHandle = job.Schedule();
            return jobHandle;
        }

        protected override void OnDestroy() {
            result.Dispose();
            Debug.Log("On destroy");
            Debug.Log(result.IsCreated);
        }
        
        private struct UpdatePositionsJob : IJob {
            public Matrix4x4 transformCenter;
            public NativeArray<int2> result;
            
            public void Execute() {
                transformCenter = Matrix4x4.identity;
                for (int i = 0; i < 10000; i++)
                    result[i] = CalculateGridSize();
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
}