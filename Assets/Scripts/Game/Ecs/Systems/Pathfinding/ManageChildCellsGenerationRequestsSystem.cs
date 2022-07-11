using Game.Ecs.Components.Pathfinding;
using Game.Ecs.Systems.Pathfinding.Mono;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Utils.Pathfinding;

namespace Game.Ecs.Systems.Pathfinding {
    // the only reason it is SystemBase and not just plain C# object - it needs to access singleton data
    public partial class ManageChildCellsGenerationRequestsSystem : SystemBase {
        public NativeHashMap<int2, ChildCellsGenerationRequest> Requests;

        private UnsafeList<FlowfieldCellComponent>.ParallelWriter _parentCells;
        private DependenciesScheduler _jobDependenciesScheduler;

        private EmptyCellsGenerationSubSystem _emptyCellsGenerationSubSystem;
        private FindBaseCostAndHeightsSubSystem _findBaseCostAndHeightsSubSystem;
        private GenerateIntegrationFieldSubsystem _generateIntegrationFieldSubsystem;
        private GenerateFlowFieldSubsystem _generateFlowFieldSubsystem;

        private int2 _parentGridSize;
        private FlowfieldRuntimeData _flowfieldRuntimeData;
        private bool _initialized;
        
        public void InjectFlowfieldDependencies(DependenciesScheduler dependenciesScheduler, UnsafeList<FlowfieldCellComponent>.ParallelWriter parentCells,
            EmptyCellsGenerationSubSystem emptyCellsGenerationSubSystem, FindBaseCostAndHeightsSubSystem findBaseCostAndHeightsSubSystem,
            GenerateIntegrationFieldSubsystem generateIntegrationFieldSubsystem, GenerateFlowFieldSubsystem generateFlowFieldSubsystem, int2 gridSize, FlowfieldRuntimeData runtimeData) {
            _jobDependenciesScheduler = dependenciesScheduler;
            _emptyCellsGenerationSubSystem = emptyCellsGenerationSubSystem;
            _findBaseCostAndHeightsSubSystem = findBaseCostAndHeightsSubSystem;
            _generateIntegrationFieldSubsystem = generateIntegrationFieldSubsystem;
            _generateFlowFieldSubsystem = generateFlowFieldSubsystem;
            _parentCells = parentCells;
            
            _parentGridSize = gridSize;
            _flowfieldRuntimeData = runtimeData;
            Requests = new NativeHashMap<int2, ChildCellsGenerationRequest>(gridSize.x * gridSize.y, Allocator.Persistent);
            for (int x = 0; x < gridSize.x; x++) {
                for (int y = 0; y < gridSize.y; y++) {
                    var gridPos = new int2(x, y);
                    Requests[gridPos] = new ChildCellsGenerationRequest(gridPos, 0);
                }
            }
            _initialized = true;
        }

        public void OnUpdateManual() {
            if (!_initialized) return;
            ClearUnusedCells(Requests);
            GenerateRequestedCells(Requests);
            DecrementRequestsLifetime(Requests);
        }
        
        protected override void OnUpdate() { }

        private void DecrementRequestsLifetime(NativeHashMap<int2, ChildCellsGenerationRequest> requests) {
            foreach (var request in requests) {
                request.Value.DecrementLifetime();
            }
        }

        private unsafe void ClearUnusedCells(NativeHashMap<int2, ChildCellsGenerationRequest> requests) {
            foreach (var request in requests) {
                if (request.Value.FramesLifetime > 0) continue;

                var arrayIndex = FlowfieldUtility.CalculateIndexFromGrid(request.Key, _parentGridSize);
                var cellsToClear = _parentCells.ListData->Ptr[arrayIndex].ChildCells;
                if (cellsToClear.ListData->Length == 0) continue;
                
                var clearCellsJob = new ClearChildCellsJob(cellsToClear);
                _jobDependenciesScheduler.ScheduleReadWrite(clearCellsJob);
            }
        }

        private unsafe void GenerateRequestedCells(NativeHashMap<int2, ChildCellsGenerationRequest> requests) {
            var hivemindTarget = GetSingleton<CurrentHivemindTargetSingleton>().Value;
            foreach (var request in requests) {
                if (request.Value.FramesLifetime <= 0) continue;

                var arrayIndex = FlowfieldUtility.CalculateIndexFromGrid(request.Key, _parentGridSize);
                var currentParentCell = _parentCells.ListData->Ptr[arrayIndex];
                var cellsToAdd = _parentCells.ListData->Ptr[arrayIndex].ChildCells;
                if (cellsToAdd.ListData->Length > 0 || currentParentCell.Unwalkable) continue;

                var generateCellsJob = _emptyCellsGenerationSubSystem.ScheduleReadWrite(_flowfieldRuntimeData.ChildCellSize, _flowfieldRuntimeData.ChildGridSize,
                    currentParentCell.WorldPosition, cellsToAdd);

                var bestCellOut = new NativeArray<FlowfieldCellComponent>(1, Allocator.TempJob);
                var findClosestToBestParentCellJob = new FindTargetCellJob(arrayIndex, hivemindTarget, bestCellOut, _parentCells, _flowfieldRuntimeData);
                var closestCellHandle = _jobDependenciesScheduler.ScheduleReadWrite(findClosestToBestParentCellJob, dependenciesIn: generateCellsJob);

                var fillHeightsJob = _findBaseCostAndHeightsSubSystem.ScheduleReadWrite(cellsToAdd, _flowfieldRuntimeData.ChildGridSize, closestCellHandle, _flowfieldRuntimeData.UnwalkableAngleThreshold,
                    _flowfieldRuntimeData.CostlyHeightThreshold);
                
                var generateIntegrationFieldJob = _generateIntegrationFieldSubsystem.ScheduleReadWrite(currentParentCell, bestCellOut, _flowfieldRuntimeData.ChildGridSize, cellsToAdd, fillHeightsJob);
                var generateFlowfieldJob = _generateFlowFieldSubsystem.ScheduleReadWrite(cellsToAdd, _flowfieldRuntimeData.ChildGridSize, generateIntegrationFieldJob);
                
                bestCellOut.Dispose(generateFlowfieldJob);
            }
        }

        protected override void OnDestroy() {
            Requests.Dispose();
        }

        [BurstCompile]
        private readonly struct ClearChildCellsJob : IJob {
            private readonly UnsafeList<FlowfieldCellComponent>.ParallelWriter _childCellsWriter;

            public ClearChildCellsJob(UnsafeList<FlowfieldCellComponent>.ParallelWriter childCellsWriter) {
                _childCellsWriter = childCellsWriter;
            }
            
            public unsafe void Execute() {
                if (_childCellsWriter.ListData->Length > 0) {
                    _childCellsWriter.ListData->Clear();
                }
            }
        }
    }

    public struct ChildCellsGenerationRequest {
        public int2 GridPosition;
        public int FramesLifetime;

        private const int _minLifetime = 0;
        private const int _maxLifetime = 10;

        public ChildCellsGenerationRequest(int2 grid, int framesLifetime) {
            GridPosition = grid;
            FramesLifetime = framesLifetime;
        }

        public void DecrementLifetime() {
            FramesLifetime = Mathf.Clamp(FramesLifetime - 1, _minLifetime, _maxLifetime);
        }

        public void IncrementLifetime() {
            FramesLifetime = Mathf.Clamp(FramesLifetime + 2, _minLifetime, _maxLifetime);
        }
    }
}