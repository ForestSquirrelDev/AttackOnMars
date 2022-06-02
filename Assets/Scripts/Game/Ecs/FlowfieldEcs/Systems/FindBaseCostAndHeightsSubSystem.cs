using Game.Ecs.Flowfield.Components;
using Game.Ecs.Flowfield.Systems;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Game.Ecs.Systems.Spawners {
    // Flowfield step 2: find heights and base costs via scheduled raycasts.
    public class FindBaseCostAndHeightsSubSystem {
        private FlowfieldJobDependenciesHandler _jobDependenciesHandler;

        public FindBaseCostAndHeightsSubSystem(FlowfieldJobDependenciesHandler dependenciesHandler) {
            _jobDependenciesHandler = dependenciesHandler;
        }

        public void OnCreate() {
        }

        public unsafe JobHandle Schedule(UnsafeList<FlowfieldCellComponent>.ParallelWriter writer, JobHandle inputDeps) {
            // проблема: основной поток выполняется раньше чем заполняется unsafe list
            var cellsCount = writer.ListData->Capacity;
            Debug.Log($"Writer length: {cellsCount}");
            
            var cellsOriginRaycastCommands = new NativeArray<RaycastCommand>(cellsCount, Allocator.TempJob);
            var cellsCenterRaycastCommands = new NativeArray<RaycastCommand>(cellsCount, Allocator.TempJob);
            var cellsOriginRaycastResults = new NativeArray<RaycastHit>(cellsCount, Allocator.TempJob);
            var cellsCenterRaycastResults = new NativeArray<RaycastHit>(cellsCount, Allocator.TempJob);
            
            var fillRaycastsCommandsJob = new FillRaycastCommandsJob {
                Writer = writer,
                CellsCenterRaycastCommands = cellsCenterRaycastCommands,
                CellsOriginRaycastCommands = cellsOriginRaycastCommands
            };
            var fillRaycastCommandsHandle = _jobDependenciesHandler.ScheduleNonPooled(fillRaycastsCommandsJob, inputDeps);
            
            var fireOriginRaycastsJob = RaycastCommand.ScheduleBatch(cellsOriginRaycastCommands, cellsOriginRaycastResults, 1, fillRaycastCommandsHandle);
            var fireCenterRaycastsJob = RaycastCommand.ScheduleBatch(cellsCenterRaycastCommands, cellsCenterRaycastResults, 1, fillRaycastCommandsHandle);
            
            var combinedDependencies = JobHandle.CombineDependencies(fireOriginRaycastsJob, fireCenterRaycastsJob, fillRaycastCommandsHandle);
            
            var fillHeightsJob = new FillHeightsJob {
                CellsListWriter = writer,
                CellsCentersRaycastHits = cellsCenterRaycastResults,
                CellsOriginsRaycastHits = cellsOriginRaycastResults
            };
            var fillHeightsHandle = _jobDependenciesHandler.ScheduleNonPooled(fillHeightsJob, combinedDependencies);
            
            cellsOriginRaycastCommands.Dispose(fillHeightsHandle);
            cellsCenterRaycastCommands.Dispose(fillHeightsHandle);
            cellsOriginRaycastResults.Dispose(fillHeightsHandle);
            cellsCenterRaycastResults.Dispose(fillHeightsHandle);

            return fillHeightsHandle;
        }
        
        public void OnUpdate() {
        }

        public void OnDestroy() {
        }
        
        [BurstCompile]
        private struct FillRaycastCommandsJob : IJob {
            public UnsafeList<FlowfieldCellComponent>.ParallelWriter Writer;
            public NativeArray<RaycastCommand> CellsOriginRaycastCommands;
            public NativeArray<RaycastCommand> CellsCenterRaycastCommands;

            public unsafe void Execute() {
                for (var i = 0; i < Writer.ListData->Length; i++) {
                    var cell = Writer.ListData->Ptr[i];
                    var cellOriginRay = new float3(cell.WorldPosition.x, cell.WorldPosition.y + 1000f, cell.WorldPosition.z);
                    CellsOriginRaycastCommands[i] = new RaycastCommand(cellOriginRay, Vector3.down);
        
                    var cellCenterRay = new float3(cell.WorldCenter.x, cell.WorldCenter.y + 1000f, cell.WorldCenter.z);
                    CellsCenterRaycastCommands[i] = new RaycastCommand(cellCenterRay, Vector3.down);
                }
            }
        }
        
        private struct FillHeightsJob : IJob {
            public UnsafeList<FlowfieldCellComponent>.ParallelWriter CellsListWriter;
            public NativeArray<RaycastHit> CellsOriginsRaycastHits;
            public NativeArray<RaycastHit> CellsCentersRaycastHits;

            public unsafe void Execute() {
                for (var i = 0; i < CellsListWriter.ListData->Length; i++) {
                    var cell = CellsListWriter.ListData->Ptr[i];
                    cell.WorldPosition.y = CellsOriginsRaycastHits[i].point.y;
                    cell.WorldCenter.y = CellsCentersRaycastHits[i].point.y;
                    CellsListWriter.ListData->Ptr[i] = cell;
                }
            }
        }
    }
}