using Game.Ecs.Components.Pathfinding;
using Game.Ecs.Utils;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using Utils;
using Utils.Pathfinding;

namespace Game.Ecs.Systems.Pathfinding {
    public class GenerateIntegrationFieldSubsystem {
        private readonly DependenciesScheduler _dependenciesScheduler;

        public GenerateIntegrationFieldSubsystem(DependenciesScheduler dependenciesScheduler) {
            _dependenciesScheduler = dependenciesScheduler;
        }

        public JobHandle ScheduleReadWrite(FlowfieldCellComponent currentParentCell, NativeArray<FlowfieldCellComponent> bestCellIn, int2 gridSize, UnsafeList<FlowfieldCellComponent>.ParallelWriter writer, JobHandle inputDeps) {
            var integrationFieldJob = new CreateIntegrationFieldJob(currentParentCell, bestCellIn, gridSize, writer);
            return _dependenciesScheduler.ScheduleReadWrite(integrationFieldJob, 4, inputDeps);
        }

        public JobHandle ScheduleReadWrite(float3 origin, float3 targetWorld, int2 gridSize, UnsafeList<FlowfieldCellComponent>.ParallelWriter writer, JobHandle inputDeps) {
            var emptyArray = new NativeArray<FlowfieldCellComponent>(0, Allocator.TempJob);
            var integrationFieldJob = new CreateIntegrationFieldJob(origin, targetWorld, gridSize, writer, emptyArray);
            var handle = _dependenciesScheduler.ScheduleReadWrite(integrationFieldJob, 4, inputDeps);
            emptyArray.Dispose(handle);
            return handle;
        }
        
        [BurstCompile]
        private struct CreateIntegrationFieldJob : IJob {
            private NativeArray<FlowfieldCellComponent> _bestCellIn;
            private readonly int2 _gridSize;
            private readonly UnsafeList<FlowfieldCellComponent>.ParallelWriter _cellsWriter;
            private readonly FlowfieldCellComponent _currentParentCell;

            // origin and target as float3's are used only for parent grid, as there are no best child cells to generate from
            private readonly float3 _origin;
            private readonly float3 _targetWorld;

            public CreateIntegrationFieldJob(FlowfieldCellComponent currentParentCell, NativeArray<FlowfieldCellComponent> bestCellIn, int2 gridSize, UnsafeList<FlowfieldCellComponent>.ParallelWriter cellsWriter) {
                _bestCellIn = bestCellIn;
                _gridSize = gridSize;
                _cellsWriter = cellsWriter;
                _currentParentCell = currentParentCell;
                _origin = VectorUtility.InvalidFloat3();
                _targetWorld = VectorUtility.InvalidFloat3();
            }

            public CreateIntegrationFieldJob(float3 origin, float3 targetWorld, int2 gridSize, UnsafeList<FlowfieldCellComponent>.ParallelWriter cellsWriter, NativeArray<FlowfieldCellComponent> emptyArray) {
                _bestCellIn = emptyArray;
                _currentParentCell = default;
                _origin = origin;
                _targetWorld = targetWorld;
                _gridSize = gridSize;
                _cellsWriter = cellsWriter;
            }
            
            public unsafe void Execute() {
                var origin = _currentParentCell == default ? _origin : _currentParentCell.WorldPosition;
                var targetWorld = _bestCellIn.Length == 0 ? _targetWorld : _bestCellIn[0].WorldPosition;
                
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
                closedList.Dispose();
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
