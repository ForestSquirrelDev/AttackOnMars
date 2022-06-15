using Game.Ecs.Components.Pathfinding;
using Game.Ecs.Utils;
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

        public JobHandle Schedule(FlowfieldCellComponent currentParentCell, NativeArray<FlowfieldCellComponent> bestCellIn, int2 gridSize, UnsafeList<FlowfieldCellComponent>.ParallelWriter writer, JobHandle inputDeps) {
            var integrationFieldJob = new CreateIntegrationFieldJob(currentParentCell, bestCellIn, gridSize, writer);
            return _dependenciesHandler.ScheduleReadWrite(integrationFieldJob, 4, inputDeps);
        }

        public JobHandle Schedule(float3 origin, float3 targetWorld, int2 gridSize, UnsafeList<FlowfieldCellComponent>.ParallelWriter writer, JobHandle inputDeps) {
            var emptyArray = new NativeArray<FlowfieldCellComponent>(0, Allocator.TempJob);
            var integrationFieldJob = new CreateIntegrationFieldJob(origin, targetWorld, gridSize, writer, emptyArray);
            var handle = _dependenciesHandler.ScheduleReadWrite(integrationFieldJob, 4, inputDeps);
            emptyArray.Dispose(handle);
            return handle;
        }
        
        [BurstCompile]
        private struct CreateIntegrationFieldJob : IJob {
            private NativeArray<FlowfieldCellComponent> _bestCellsIn;
            private readonly int2 _gridSize;
            private readonly UnsafeList<FlowfieldCellComponent>.ParallelWriter _cellsWriter;
            private readonly FlowfieldCellComponent _currentParentCell;

            // origin and target as float3 are used only from parent grid, as there are no best child cells to generate from
            private readonly float3 _origin;
            private readonly float3 _targetWorld;

            public CreateIntegrationFieldJob(FlowfieldCellComponent currentParentCell, NativeArray<FlowfieldCellComponent> bestCellsIn, int2 gridSize, UnsafeList<FlowfieldCellComponent>.ParallelWriter cellsWriter) {
                _bestCellsIn = bestCellsIn;
                _gridSize = gridSize;
                _cellsWriter = cellsWriter;
                _currentParentCell = currentParentCell;
                _origin = BoilerplateShortcuts.Invalid();
                _targetWorld = BoilerplateShortcuts.Invalid();
            }

            public CreateIntegrationFieldJob(float3 origin, float3 targetWorld, int2 gridSize, UnsafeList<FlowfieldCellComponent>.ParallelWriter cellsWriter, NativeArray<FlowfieldCellComponent> emptyArray) {
                _bestCellsIn = emptyArray;
                _currentParentCell = default;
                _origin = origin;
                _targetWorld = targetWorld;
                _gridSize = gridSize;
                _cellsWriter = cellsWriter;
            }
            
            public unsafe void Execute() {
                var origin = _currentParentCell == default ? _origin : _currentParentCell.WorldPosition;
                var targetWorld = _bestCellsIn.Length == 0 ? _targetWorld : _bestCellsIn[0].WorldPosition;
                
                var openList = new NativeQueue<FlowfieldCellComponent>(Allocator.Temp);
                var closedList = new NativeList<FlowfieldCellComponent>(_cellsWriter.ListData->Length, Allocator.Temp);
                var firstCell = _cellsWriter.ListData->Ptr[0];
                var targetCellIndex = FlowfieldUtility.CalculateIndexFromWorld(targetWorld, origin, _gridSize, firstCell.Size);
                var targetCell = _cellsWriter.ListData->Ptr[targetCellIndex];
                targetCell.BaseCost = 0;
                targetCell.BestCost = 0;
                targetCell.IsBestCell = true;
                _cellsWriter.ListData->Ptr[targetCellIndex] = targetCell;
                openList.Enqueue(targetCell);
                closedList.Add(targetCell);

                while (openList.Count > 0) {
                    var currentCell = openList.Dequeue();
                    var neighbours = FindNeighbours(currentCell, _cellsWriter, _gridSize);
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
                            _cellsWriter.ListData->Ptr[neighbourIndex] = neighbour;
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
