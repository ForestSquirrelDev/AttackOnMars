﻿using Game.Ecs.Components.Pathfinding;
using Game.Ecs.Systems.Spawners;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Utils.Pathfinding;

namespace Game.Ecs.Systems.Pathfinding {
    // Flowfield step 1. Create grid of empty cells.
    public class EmptyCellsGenerationSubSystem {
        private FlowfieldJobDependenciesHandler _jobDependenciesHandler;

        public EmptyCellsGenerationSubSystem(FlowfieldJobDependenciesHandler dependenciesHandler, FindBaseCostAndHeightsSubSystem findBaseCostAndHeightsSubSystem) {
            _jobDependenciesHandler = dependenciesHandler;
        }
        
        public JobHandle Schedule(float cellSize, int2 gridSize, float3 origin, UnsafeList<FlowfieldCellComponent>.ParallelWriter writer, JobHandle inputDeps) {
            var fillEmptyCellsJob = new FillEmptyCellsJob(writer, gridSize, origin, cellSize);
            return _jobDependenciesHandler.ScheduleReadWrite(fillEmptyCellsJob, 4, inputDeps);
        }

        [BurstCompile]
        private struct FillEmptyCellsJob : IJob {
            private UnsafeList<FlowfieldCellComponent>.ParallelWriter _cellsWriter;
            private int2 _gridSize;
            private float3 _origin;
            private float _cellSize;

            public FillEmptyCellsJob(UnsafeList<FlowfieldCellComponent>.ParallelWriter cellsWriter, int2 gridSize, float3 origin, float cellSize) {
                _cellsWriter = cellsWriter;
                _gridSize = gridSize;
                _origin = origin;
                _cellSize = cellSize;
            }
            
            public unsafe void Execute() {
                for (int x = 0; x < _gridSize.x; x++) {
                    for (int y = 0; y < _gridSize.y; y++) {
                        var cell = new FlowfieldCellComponent();
                        cell.GridPosition = new int2(x, y);
                        cell.WorldPosition = FlowfieldUtility.ToWorld(cell.GridPosition, _origin, _cellSize);
                        cell.WorldCenter = FlowfieldUtility.FindCellCenter(cell.WorldPosition, _cellSize);
                        cell.GridPosition = new int2(x, y);
                        var cellRect = new FlowFieldRect {
                            X = cell.WorldPosition.x,
                            Y = cell.WorldPosition.z,
                            Height = (int)_cellSize,
                            Width = (int)_cellSize
                        };
                        cell.Size = _cellSize;
                        cell.WorldRect = cellRect;
                        cell.BestCost = float.MaxValue;
                        _cellsWriter.ListData->AddNoResize(cell);
                    }
                }
                Debug.Log($"Added cells. Length: {_cellsWriter.ListData->Length}");
            }
        }
    }
}