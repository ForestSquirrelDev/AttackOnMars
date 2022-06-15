using Game.Ecs.Components.Pathfinding;
using Game.Ecs.Utils;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Utils.Pathfinding;

namespace Game.Ecs.Systems.Pathfinding {
    public class GenerateFlowFieldSubsystem {
        private FlowfieldJobDependenciesHandler _dependenciesHandler;

        public GenerateFlowFieldSubsystem(FlowfieldJobDependenciesHandler dependenciesHandler) {
            _dependenciesHandler = dependenciesHandler;
        }

        public JobHandle Schedule(UnsafeList<FlowfieldCellComponent>.ParallelWriter cellsWriter, int2 gridSize, NativeArray<FlowfieldCellComponent> bestCellIn, JobHandle inputDeps) {
            var flowfieldJob = new GenerateFlowFieldJob(cellsWriter, gridSize, bestCellIn);
            return _dependenciesHandler.ScheduleReadWrite(flowfieldJob, dependenciesIn: inputDeps);
        }
        
        public JobHandle Schedule(float3 targetWorld, int2 gridSize, UnsafeList<FlowfieldCellComponent>.ParallelWriter cellsWriter, JobHandle inputDeps) {
            var emptyArray = new NativeArray<FlowfieldCellComponent>(0, Allocator.TempJob);
            var flowfieldJob = new GenerateFlowFieldJob(cellsWriter, gridSize, targetWorld, emptyArray);
            var handle = _dependenciesHandler.ScheduleReadWrite(flowfieldJob, dependenciesIn: inputDeps);
            emptyArray.Dispose(handle);
            return handle;
        }

        [BurstCompile]
        private readonly struct GenerateFlowFieldJob : IJob {
            private readonly UnsafeList<FlowfieldCellComponent>.ParallelWriter _writer;
            private readonly int2 _gridSize;
            private readonly NativeArray<FlowfieldCellComponent> _bestCellIn;

            // used only for parent grid generation
            private readonly float3 _targetWorldPosition;

            public GenerateFlowFieldJob(UnsafeList<FlowfieldCellComponent>.ParallelWriter writer, int2 gridSize, NativeArray<FlowfieldCellComponent> bestCellIn) {
                _writer = writer;
                _gridSize = gridSize;
                _bestCellIn = bestCellIn;
                _targetWorldPosition = BoilerplateShortcuts.Invalid();
            }
            
            public GenerateFlowFieldJob(UnsafeList<FlowfieldCellComponent>.ParallelWriter writer, int2 gridSize, float3 targetWorldPosition, NativeArray<FlowfieldCellComponent> emptyArray) {
                _writer = writer;
                _gridSize = gridSize;
                _targetWorldPosition = targetWorldPosition;
                _bestCellIn = emptyArray;
            }
            
            public unsafe void Execute() {
                for (var i = 0; i < _writer.ListData->Length; i++) {
                    var cell = _writer.ListData->Ptr[i];
                    if (cell.IsBestCell) continue;
                    var index = FlowfieldUtility.CalculateIndexFromGrid(cell.GridPosition, _gridSize);

                    var neighbours = FindNeighbours(cell, _writer, _gridSize);
                    var bestDirection = FindBestDirectionBasedOnCosts(cell, neighbours, _writer, _gridSize);
                    neighbours.Dispose();
                    cell.BestDirection = bestDirection;
                    
                    _writer.ListData->Ptr[index] = cell;
                }
            }
            
            private unsafe NativeList<FlowfieldCellComponent> FindNeighbours(FlowfieldCellComponent currentCell, UnsafeList<FlowfieldCellComponent>.ParallelWriter allCells, int2 gridSize) {
                var neighbourOffsets = FlowfieldUtility.GetNeighbourOffsets();
                var neighbours = new NativeList<FlowfieldCellComponent>(8, Allocator.Temp);

                for (var i = 0; i < neighbourOffsets.Length; i++) {
                    var neighbourOffset = neighbourOffsets[i];
                    var neighbourGridPosition = new int2(currentCell.GridPosition.x + neighbourOffset.x, currentCell.GridPosition.y + neighbourOffset.y);
                    if (FlowfieldUtility.TileOutOfGrid(neighbourGridPosition, gridSize))
                        continue;
                    var neighbourIndex = FlowfieldUtility.CalculateIndexFromGrid(neighbourGridPosition.x, neighbourGridPosition.y, gridSize);
                    var neighbourCell = allCells.ListData->Ptr[neighbourIndex];
                    neighbours.Add(neighbourCell);
                }

                neighbourOffsets.Dispose();
                return neighbours;
            }
            
            private unsafe int2 FindBestDirectionBasedOnCosts(FlowfieldCellComponent currentCell, NativeList<FlowfieldCellComponent> validNeighbours,
                UnsafeList<FlowfieldCellComponent>.ParallelWriter writer, int2 gridSize) {
                var bestCell = FindLowestCostCellSlow(validNeighbours);
                return bestCell.GridPosition - currentCell.GridPosition;
            }

            private FlowfieldCellComponent FindLowestCostCellSlow(NativeList<FlowfieldCellComponent> validNeighbours) {
                var bestCell = new FlowfieldCellComponent {
                    BestCost = float.MaxValue,
                    GridPosition = new int2(-1, -1)
                };
                for (var i = 0; i < validNeighbours.Length; i++) {
                    var currentCell = validNeighbours[i];
                
                    if (currentCell.BestCost < bestCell.BestCost) {
                        bestCell = currentCell;
                    }
                }
                return bestCell;
            }
        }
    }
}
