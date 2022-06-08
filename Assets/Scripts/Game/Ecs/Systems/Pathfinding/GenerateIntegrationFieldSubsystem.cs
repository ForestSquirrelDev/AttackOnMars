using Game.Ecs.Components.Pathfinding;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using Utils.Pathfinding;

namespace Game.Ecs.Systems.Pathfinding {
    public class GenerateIntegrationFieldSubsystem {
        private FlowfieldJobDependenciesHandler _dependenciesHandler;

        public GenerateIntegrationFieldSubsystem(FlowfieldJobDependenciesHandler dependenciesHandler) {
            _dependenciesHandler = dependenciesHandler;
        }

        public JobHandle Schedule(float3 targetWorld, int2 gridSize, UnsafeList<FlowfieldCellComponent>.ParallelWriter writer, JobHandle inputDeps) {
            var integrationFieldJob = new CreateIntegrationFieldJob(targetWorld, gridSize, writer);
            return _dependenciesHandler.ScheduleReadWrite(integrationFieldJob, 4, inputDeps);
        }
        
        [BurstCompile]
        private readonly struct CreateIntegrationFieldJob : IJob {
            private readonly float3 _targetWorld;
            private readonly int2 _gridSize;
            private readonly UnsafeList<FlowfieldCellComponent>.ParallelWriter _childCellsWriter;

            public CreateIntegrationFieldJob(float3 targetWorld, int2 gridSize, UnsafeList<FlowfieldCellComponent>.ParallelWriter childCellsWriter) {
                _targetWorld = targetWorld;
                _gridSize = gridSize;
                _childCellsWriter = childCellsWriter;
            }

            public unsafe void Execute() {
                var openList = new NativeQueue<FlowfieldCellComponent>(Allocator.Temp);
                var closedList = new NativeList<FlowfieldCellComponent>(_childCellsWriter.ListData->Length, Allocator.Temp);
                var firstCell = _childCellsWriter.ListData->Ptr[0];
                var targetCellIndex = FlowfieldUtility.CalculateIndexFromWorld(_targetWorld, firstCell.WorldPosition, _gridSize, firstCell.Size);
                var targetCell = _childCellsWriter.ListData->Ptr[targetCellIndex];
                targetCell.BaseCost = 0;
                targetCell.BestCost = 0;
                _childCellsWriter.ListData->Ptr[targetCellIndex] = targetCell;
                openList.Enqueue(targetCell);

                while (openList.Count > 0) {
                    var currentCell = openList.Dequeue();
                    var neighbours = FindNeighbours(currentCell, _childCellsWriter, _gridSize);
                    for (var i = 0; i < neighbours.Length; i++) {
                        var neighbour = neighbours[i];
                        if (FlowfieldUtility.TileOutOfGrid(neighbour.GridPosition, _gridSize)
                            || neighbour.BaseCost == float.MaxValue || closedList.Contains(neighbour)) {
                            continue;
                        }
                        var totalCost = neighbour.BaseCost + currentCell.BestCost;
                        if (totalCost < neighbour.BestCost) {
                            neighbour.BestCost = totalCost;
                            var neighbourIndex = FlowfieldUtility.CalculateIndexFromGrid(neighbour.GridPosition, _gridSize);
                            _childCellsWriter.ListData->Ptr[neighbourIndex] = neighbour;
                            openList.Enqueue(neighbour);
                            closedList.Add(neighbour);
                        }
                    }
                    neighbours.Dispose();
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
        }
    }
}
