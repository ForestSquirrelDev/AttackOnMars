using Flowfield;
using Game.Ecs.Flowfield.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Game.Ecs.Flowfield.Systems {
    public class GenerateFlowFieldSubsystem {
        private FlowfieldJobDependenciesHandler _dependenciesHandler;

        public GenerateFlowFieldSubsystem(FlowfieldJobDependenciesHandler dependenciesHandler) {
            _dependenciesHandler = dependenciesHandler;
        }

        public JobHandle Schedule(UnsafeList<FlowfieldCellComponent>.ParallelWriter cellsWriter, int2 gridSize, float3 targetWorldPosition, JobHandle inputDeps) {
            var flowfieldJob = new GenerateFlowFieldJob(cellsWriter, gridSize, targetWorldPosition);
            return _dependenciesHandler.ScheduleReadWrite(flowfieldJob, dependenciesIn: inputDeps);
        }

        [BurstCompile]
        private readonly struct GenerateFlowFieldJob : IJob {
            private readonly UnsafeList<FlowfieldCellComponent>.ParallelWriter _writer;
            private readonly int2 _gridSize;
            private readonly float3 _targetWorldPosition;

            public GenerateFlowFieldJob(UnsafeList<FlowfieldCellComponent>.ParallelWriter writer, int2 gridSize, float3 targetWorldPosition) {
                _writer = writer;
                _gridSize = gridSize;
                _targetWorldPosition = targetWorldPosition;
            }
            
            public unsafe void Execute() {
                for (var i = 0; i < _writer.ListData->Length; i++) {
                    var cell = _writer.ListData->Ptr[i];
                    var index = FlowfieldUtility.CalculateIndexFromGrid(cell.GridPosition, _gridSize);
                
                    if (cell.BaseCost == 0 && cell.BestCost == 0) {
                        var worldDirection = math.normalize(_targetWorldPosition - cell.WorldCenter);
                        cell.BestDirection = new int2(Mathf.RoundToInt(worldDirection.x), Mathf.RoundToInt(worldDirection.z));
                    } else {
                        var neighbours = FindNeighbours(cell, _writer, _gridSize);
                        var bestDirection = FindBestDirectionBasedOnCosts(cell, neighbours);
                        cell.BestDirection = bestDirection;
                    }
                
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
            
            private int2 FindBestDirectionBasedOnCosts(FlowfieldCellComponent currentCell, NativeList<FlowfieldCellComponent> validNeighbours) {
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
                validNeighbours.Dispose();
                return bestCell;
            }
        }
    }
}
