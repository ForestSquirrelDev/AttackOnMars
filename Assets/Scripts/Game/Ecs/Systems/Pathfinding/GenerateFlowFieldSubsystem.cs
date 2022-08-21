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
        private DependenciesScheduler _dependenciesScheduler;

        public GenerateFlowFieldSubsystem(DependenciesScheduler dependenciesScheduler) {
            _dependenciesScheduler = dependenciesScheduler;
        }

        public JobHandle ScheduleReadWrite(UnsafeList<FlowfieldCellComponent>.ParallelWriter cellsWriter, int2 gridSize, JobHandle inputDeps = default) {
            var flowfieldJob = new GenerateFlowFieldJob(cellsWriter, gridSize);
            return _dependenciesScheduler.ScheduleReadWrite(flowfieldJob, dependenciesIn: inputDeps);
        }

        [BurstCompile]
        private readonly struct GenerateFlowFieldJob : IJob {
            private readonly UnsafeList<FlowfieldCellComponent>.ParallelWriter _writer;
            private readonly int2 _gridSize;

            public GenerateFlowFieldJob(UnsafeList<FlowfieldCellComponent>.ParallelWriter writer, int2 gridSize) {
                _writer = writer;
                _gridSize = gridSize;
            }

            public unsafe void Execute() {
                for (var i = 0; i < _writer.ListData->Length; i++) {
                    var cell = _writer.ListData->Ptr[i];
                    if (cell.IsBestCell) continue;
                    var index = FlowfieldUtility.CalculateIndexFromGrid(cell.GridPosition, _gridSize);

                    var neighbours = FindNeighbours(cell, _writer, _gridSize);
                    var bestDirection = FindBestDirectionBasedOnCosts(cell, neighbours);
                    neighbours.Dispose();
                    cell.BestFlowfieldDirection = bestDirection;
                    
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
                return bestCell;
            }
        }
    }
}
