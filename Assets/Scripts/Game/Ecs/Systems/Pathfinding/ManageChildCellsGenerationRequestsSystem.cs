using Game.Ecs.Components.Pathfinding;
using Game.Ecs.Systems.Pathfinding.Mono;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Utils.Pathfinding;

namespace Game.Ecs.Systems.Pathfinding {
    public class ManageChildCellsGenerationRequestsSystem {
        public NativeHashMap<int2, ChildCellsGenerationRequest> Requests;

        private UnsafeList<FlowfieldCellComponent>.ParallelWriter _parentCells;
        private FlowfieldJobDependenciesHandler _jobDependenciesHandler;

        private EmptyCellsGenerationSubSystem _emptyCellsGenerationSubSystem;
        private FindBaseCostAndHeightsSubSystem _findBaseCostAndHeightsSubSystem;
        private GenerateIntegrationFieldSubsystem _generateIntegrationFieldSubsystem;
        private GenerateFlowFieldSubsystem _generateFlowFieldSubsystem;

        private int2 _parentGridSize;
        private FlowfieldRuntimeData _flowfieldRuntimeData;

        public ManageChildCellsGenerationRequestsSystem(FlowfieldJobDependenciesHandler dependenciesHandler, UnsafeList<FlowfieldCellComponent>.ParallelWriter parentCells,
            EmptyCellsGenerationSubSystem emptyCellsGenerationSubSystem, FindBaseCostAndHeightsSubSystem findBaseCostAndHeightsSubSystem,
            GenerateIntegrationFieldSubsystem generateIntegrationFieldSubsystem, GenerateFlowFieldSubsystem generateFlowFieldSubsystem) {
            _jobDependenciesHandler = dependenciesHandler;
            _emptyCellsGenerationSubSystem = emptyCellsGenerationSubSystem;
            _findBaseCostAndHeightsSubSystem = findBaseCostAndHeightsSubSystem;
            _generateIntegrationFieldSubsystem = generateIntegrationFieldSubsystem;
            _generateFlowFieldSubsystem = generateFlowFieldSubsystem;
            _parentCells = parentCells;
        }

        public void Construct(int2 gridSize, FlowfieldRuntimeData runtimeData) {
            _parentGridSize = gridSize;
            _flowfieldRuntimeData = runtimeData;
            Requests = new NativeHashMap<int2, ChildCellsGenerationRequest>(gridSize.x * gridSize.y, Allocator.Persistent);
            for (int x = 0; x < gridSize.x; x++) {
                for (int y = 0; y < gridSize.y; y++) {
                    var gridPos = new int2(x, y);
                    Requests[gridPos] = new ChildCellsGenerationRequest(gridPos, 0);
                }
            }
        }

        public unsafe void OnUpdate() {
            ClearUnusedCells(Requests);
            GenerateRequestedCells(Requests);
            DecrementRequestsLifetime(Requests);
        }

        private void DecrementRequestsLifetime(NativeHashMap<int2, ChildCellsGenerationRequest> requests) {
            foreach (var request in requests) {
                request.Value.DecrementLifetime();
            }
        }

        private unsafe void ClearUnusedCells(NativeHashMap<int2, ChildCellsGenerationRequest> requests) {
            foreach (var request in requests) {
                if (request.Value.FramesLifetime > 0) continue;

                var arrayIndex = FlowfieldUtility.CalculateIndexFromGrid(request.Key, _parentGridSize);
                var cellsToClear = _parentCells.ListData->Ptr[arrayIndex].ChildCells.AsParallelWriter();
                if (cellsToClear.ListData->Length == 0) continue;
                
                var clearCellsJob = new ClearChildCellsJob(cellsToClear);
                _jobDependenciesHandler.ScheduleReadWrite(clearCellsJob);
            }
        }

        private unsafe void GenerateRequestedCells(NativeHashMap<int2, ChildCellsGenerationRequest> requests) {
            foreach (var request in requests) {
                if (request.Value.FramesLifetime <= 0) continue;

                var arrayIndex = FlowfieldUtility.CalculateIndexFromGrid(request.Key, _parentGridSize);
                var parentCell = _parentCells.ListData->Ptr[arrayIndex];
                var cellsToAdd = _parentCells.ListData->Ptr[arrayIndex].ChildCells.AsParallelWriter();
                if (cellsToAdd.ListData->Length > 0) continue;

                var generateCellsJob = _emptyCellsGenerationSubSystem.Schedule(_flowfieldRuntimeData.ChildCellSize, _flowfieldRuntimeData.ChildGridSize,
                    parentCell.WorldPosition, cellsToAdd, default(JobHandle));
                var fillHeightsJob = _findBaseCostAndHeightsSubSystem.Schedule(cellsToAdd, _flowfieldRuntimeData.ChildGridSize, generateCellsJob, _flowfieldRuntimeData.UnwalkableAngleThreshold,
                    _flowfieldRuntimeData.CostlyHeightThreshold);
                var generateIntegrationFieldJob = _generateIntegrationFieldSubsystem.Schedule(MonoHivemind.Instance.CurrentTarget, _flowfieldRuntimeData.ChildGridSize, cellsToAdd, fillHeightsJob);
                var generateFlowfieldJob = _generateFlowFieldSubsystem.Schedule(cellsToAdd, _flowfieldRuntimeData.ChildGridSize, MonoHivemind.Instance.CurrentTarget, generateIntegrationFieldJob);
            }
        }

        public void OnDestroy() {
            Requests.Dispose();
        }

        [BurstCompile]
        private struct ClearChildCellsJob : IJob {
            private UnsafeList<FlowfieldCellComponent>.ParallelWriter _childCellsWriter;

            public ClearChildCellsJob(UnsafeList<FlowfieldCellComponent>.ParallelWriter childCellsWriter) {
                _childCellsWriter = childCellsWriter;
            }
            
            public unsafe void Execute() {
                if (_childCellsWriter.ListData->Length > 0)
                    _childCellsWriter.ListData->Clear();
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