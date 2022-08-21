using System;
using Game.Ecs.Components.Pathfinding;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Utils.Pathfinding;

namespace Game.Ecs.Systems.Pathfinding {
    // Flowfield step 1. Create grid of empty cells.
    public class EmptyCellsGenerationSubSystem {
        private DependenciesScheduler _jobDependenciesScheduler;

        public EmptyCellsGenerationSubSystem(DependenciesScheduler dependenciesScheduler) {
            _jobDependenciesScheduler = dependenciesScheduler;
        }
        
        public JobHandle ScheduleReadWrite(float cellSize, int2 gridSize, float3 origin, UnsafeList<FlowfieldCellComponent>.ParallelWriter writer, JobHandle inputDeps = default, bool initEntities = false) {
            var fillEmptyCellsJob = new FillEmptyCellsJob(writer, gridSize, origin, cellSize, initEntities);
            return _jobDependenciesScheduler.ScheduleReadWrite(fillEmptyCellsJob, 4, inputDeps);
        }

        [BurstCompile]
        private readonly struct FillEmptyCellsJob : IJob {
            private readonly UnsafeList<FlowfieldCellComponent>.ParallelWriter _cellsWriter;
            private readonly int2 _gridSize;
            private readonly float3 _origin;
            private readonly float _cellSize;
            private readonly bool _initEntities;

            public FillEmptyCellsJob(UnsafeList<FlowfieldCellComponent>.ParallelWriter cellsWriter, int2 gridSize, float3 origin, float cellSize, bool initEntities) {
                _cellsWriter = cellsWriter;
                _gridSize = gridSize;
                _origin = origin;
                _cellSize = cellSize;
                _initEntities = initEntities;
            }
            
            public unsafe void Execute() {
                for (int x = 0; x < _gridSize.x; x++) {
                    for (int y = 0; y < _gridSize.y; y++) {
                        var cell = new FlowfieldCellComponent();
                        var gridPosition = new int2(x, y);
                        cell.GridPosition = gridPosition;
                        cell.NeighboursIndexes = FindNeighbourIndexes(_gridSize, gridPosition);
                        cell.WorldPosition = FlowfieldUtility.ToWorld(gridPosition, _origin, _cellSize);
                        cell.WorldCenter = FlowfieldUtility.FindCellCenter(cell.WorldPosition, _cellSize);
                        var cellRect = new FlowFieldRect {
                            X = cell.WorldPosition.x,
                            Y = cell.WorldPosition.z,
                            Height = (int)_cellSize,
                            Width = (int)_cellSize
                        };
                        cell.Size = _cellSize;
                        cell.WorldRect = cellRect;
                        cell.BestCost = float.MaxValue;
                        if (_initEntities) {
                            cell.InitEntities(4, Allocator.Persistent);
                        }
                        cell.IsCreated = true;
                        _cellsWriter.ListData->AddNoResize(cell);
                    }
                }
            }

            private FlowfieldNeighbours FindNeighbourIndexes(int2 gridSize, int2 gridPosition) {
                var neighbourOffsets = FlowfieldUtility.GetNeighbourOffsets();
                var neighbours = new FlowfieldNeighbours(
                    FlowfieldUtility.CalculateIndexFromGrid(gridPosition + neighbourOffsets[0], gridSize),
                    FlowfieldUtility.CalculateIndexFromGrid(gridPosition + neighbourOffsets[1], gridSize),
                    FlowfieldUtility.CalculateIndexFromGrid(gridPosition + neighbourOffsets[2], gridSize),
                    FlowfieldUtility.CalculateIndexFromGrid(gridPosition + neighbourOffsets[3], gridSize),
                    FlowfieldUtility.CalculateIndexFromGrid(gridPosition + neighbourOffsets[4], gridSize),
                    FlowfieldUtility.CalculateIndexFromGrid(gridPosition + neighbourOffsets[5], gridSize),
                    FlowfieldUtility.CalculateIndexFromGrid(gridPosition + neighbourOffsets[6], gridSize), 
                    FlowfieldUtility.CalculateIndexFromGrid(gridPosition + neighbourOffsets[7], gridSize)
                );
                neighbourOffsets.Dispose();
                return neighbours;
            }
        }
    }
}
