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
        private readonly FlowfieldJobDependenciesHandler _jobDependenciesHandler;

        public FindBaseCostAndHeightsSubSystem(FlowfieldJobDependenciesHandler dependenciesHandler) {
            _jobDependenciesHandler = dependenciesHandler;
        }
        
        public JobHandle Schedule(UnsafeList<FlowfieldCellComponent>.ParallelWriter writer, int2 gridSize, JobHandle inputDeps, float unwalkableAngleThreshold, float costlyHeightThreshold) {
            var cellsCount = gridSize.x * gridSize.y;

            var cellsOriginRaycastCommands = new NativeArray<RaycastCommand>(cellsCount, Allocator.TempJob);
            var cellsCenterRaycastCommands = new NativeArray<RaycastCommand>(cellsCount, Allocator.TempJob);
            var cellsOriginsRaycastHits = new NativeArray<RaycastHit>(cellsCount, Allocator.TempJob);
            var cellsCentersRaycastHits = new NativeArray<RaycastHit>(cellsCount, Allocator.TempJob);

            var fillRaycastsCommandsJob = new FillRaycastCommandsJob(writer, cellsOriginRaycastCommands, cellsCenterRaycastCommands);
            var fillRaycastCommandsHandle = _jobDependenciesHandler.ScheduleNonPooled(fillRaycastsCommandsJob, inputDeps);
            
            var fireOriginRaycastsJobHandle = RaycastCommand.ScheduleBatch(cellsOriginRaycastCommands, cellsOriginsRaycastHits, 1, fillRaycastCommandsHandle);
            var fireCenterRaycastsJobHandle = RaycastCommand.ScheduleBatch(cellsCenterRaycastCommands, cellsCentersRaycastHits, 1, fillRaycastCommandsHandle);
            
            var combinedDependencies = JobHandle.CombineDependencies(fireOriginRaycastsJobHandle, fireCenterRaycastsJobHandle, fillRaycastCommandsHandle);
            
            var heightsAndBaseCostJob = new FillHeightsAndCalculateBaseCostsJob(writer, cellsOriginsRaycastHits, 
                cellsCentersRaycastHits, unwalkableAngleThreshold, costlyHeightThreshold);
            var heightsAndBaseCostJobHandle = _jobDependenciesHandler.ScheduleReadWrite(heightsAndBaseCostJob, dependenciesIn: combinedDependencies);
            
            cellsOriginRaycastCommands.Dispose(heightsAndBaseCostJobHandle);
            cellsCenterRaycastCommands.Dispose(heightsAndBaseCostJobHandle);
            cellsOriginsRaycastHits.Dispose(heightsAndBaseCostJobHandle);
            cellsCentersRaycastHits.Dispose(heightsAndBaseCostJobHandle);

            return heightsAndBaseCostJobHandle;
        }
        
        [BurstCompile]
        private struct FillRaycastCommandsJob : IJob {
            private readonly UnsafeList<FlowfieldCellComponent>.ParallelWriter _writer;
            private NativeArray<RaycastCommand> _cellsOriginRaycastCommands;
            private NativeArray<RaycastCommand> _cellsCenterRaycastCommands;

            public FillRaycastCommandsJob(UnsafeList<FlowfieldCellComponent>.ParallelWriter writer, NativeArray<RaycastCommand> cellsOriginRaycastCommands, NativeArray<RaycastCommand> cellsCenterRaycastCommands) {
                _writer = writer;
                _cellsOriginRaycastCommands = cellsOriginRaycastCommands;
                _cellsCenterRaycastCommands = cellsCenterRaycastCommands;
            }
            
            public unsafe void Execute() {
                for (var i = 0; i < _writer.ListData->Length; i++) {
                    var cell = _writer.ListData->Ptr[i];
                    var cellOriginRay = new float3(cell.WorldPosition.x, cell.WorldPosition.y + 1000f, cell.WorldPosition.z);
                    _cellsOriginRaycastCommands[i] = new RaycastCommand(cellOriginRay, Vector3.down);
        
                    var cellCenterRay = new float3(cell.WorldCenter.x, cell.WorldCenter.y + 1000f, cell.WorldCenter.z);
                    _cellsCenterRaycastCommands[i] = new RaycastCommand(cellCenterRay, Vector3.down);
                }
            }
        }
        
        private readonly struct FillHeightsAndCalculateBaseCostsJob : IJob {
            [ReadOnly] private readonly NativeArray<RaycastHit> _cellsOriginsRaycastHits;
            [ReadOnly] private readonly NativeArray<RaycastHit> _cellsCentersRaycastHits;
            private readonly UnsafeList<FlowfieldCellComponent>.ParallelWriter _cellsListWriter;
            private readonly float _unwalkableAngleThreshold;
            private readonly float _costlyHeightThreshold;

            public FillHeightsAndCalculateBaseCostsJob(UnsafeList<FlowfieldCellComponent>.ParallelWriter cellsListWriter, NativeArray<RaycastHit> cellsOriginsRaycastHits, NativeArray<RaycastHit> cellsCentersRaycastHits,
                float unwalkableAngleThreshold, float costlyHeightThreshold) {
                _cellsListWriter = cellsListWriter;
                _cellsOriginsRaycastHits = cellsOriginsRaycastHits;
                _cellsCentersRaycastHits = cellsCentersRaycastHits;
                _unwalkableAngleThreshold = unwalkableAngleThreshold;
                _costlyHeightThreshold = costlyHeightThreshold;
            }
            
            public unsafe void Execute() {
                for (var i = 0; i < _cellsListWriter.ListData->Length; i++) {
                    var originHit = _cellsOriginsRaycastHits[i];
                    var centerHit = _cellsCentersRaycastHits[i];
                    var cell = _cellsListWriter.ListData->Ptr[i];
                    
                    cell.WorldPosition.y = originHit.point.y;
                    cell.WorldCenter.y = centerHit.point.y;
                    var baseCost = FindBaseCost(cell.WorldCenter, centerHit.normal, originHit.normal, _unwalkableAngleThreshold, _costlyHeightThreshold);
                    cell.BaseCost = baseCost;
                    _cellsListWriter.ListData->Ptr[i] = cell;
                }
            }

            private float FindBaseCost(float3 cellCenterWorld, Vector3 centerNormal, Vector3 originNormal, float unwalkableAngleThreshold, float heightThreshold) {
                var angleCenter = Vector3.Angle(Vector3.up, centerNormal);
                var angleOrigin = Vector3.Angle(Vector3.up, originNormal);
                if (angleCenter > unwalkableAngleThreshold || angleOrigin > unwalkableAngleThreshold) {
                    return float.MaxValue;
                } 
                else {
                    var baseCost = 1f;
                    if (cellCenterWorld.y > heightThreshold) {
                        baseCost += cellCenterWorld.y;
                    }
                    return baseCost;
                }
            }
        }
    }
}