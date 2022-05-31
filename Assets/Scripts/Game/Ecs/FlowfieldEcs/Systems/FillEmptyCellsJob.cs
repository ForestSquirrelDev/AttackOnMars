using Flowfield;
using Game.Ecs.Flowfield.Components;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

namespace Game.Ecs.Flowfield.Systems {
    [BurstCompile]
    public struct FillEmptyCellsJob : IJob {
        public UnsafeList<FlowfieldCellComponent>.ParallelWriter FlowFieldCellsWriter;
        public int2 GridSize;
        public float3 Origin;
        public float CellSize;
            
        public void Execute() {
            int i = 0;
            for (int x = 0; x < GridSize.x; x++) {
                for (int y = 0; y < GridSize.y; y++) {
                    var cell = new FlowfieldCellComponent();
                    cell.GridPosition = new int2(x, y);
                    cell.WorldPosition = FlowfieldUtility.ToWorld(cell.GridPosition, Origin, CellSize);
                    cell.WorldCenter = FlowfieldUtility.FindCellCenter(cell.WorldPosition, CellSize);
                    cell.GridPosition = new int2(x, y);
                    var cellRect = new FlowFieldRect {
                        X = cell.WorldPosition.x,
                        Y = cell.WorldPosition.z,
                        Height = (int)CellSize,
                        Width = (int)CellSize
                    };
                    cell.Size = CellSize;
                    cell.WorldRect = cellRect;
                    cell.BestCost = float.MaxValue;
                    FlowFieldCellsWriter.AddNoResize(cell);
                    i++;
                }
            }
        }
    }
}
