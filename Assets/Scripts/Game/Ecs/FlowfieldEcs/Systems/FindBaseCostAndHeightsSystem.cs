// using Game.Ecs.Flowfield.Components;
// using Game.Ecs.Flowfield.Configs;
// using Unity.Burst;
// using Unity.Collections;
// using Unity.Entities;
// using Unity.Jobs;
// using Unity.Mathematics;
// using UnityEngine;
//
// namespace Game.Ecs.Systems.Spawners {
//     // Flowfield step 2: find heights and base costs via scheduled raycasts.
//     [UpdateInGroup(typeof(PresentationSystemGroup))]
//     public partial class FindBaseCostAndHeightsSystem : SystemBase {
// 		protected override void OnUpdate() {
//             // Entities.WithAll<BaseCostAndHeightsGenerationRequest>().ForEach((ref DynamicBuffer<FlowfieldCellBufferElementData> cellsBuffer, ref BaseCostAndHeightsGenerationRequest request) => {
//             //     Debug.Log($"Running. Cells buffer length: {cellsBuffer.Length}. ");
//             //     //if (request.IsProcessing) return;
//             //     // создать массив рейкастов
//             //     var cellsOriginRaycastCommands = new NativeArray<RaycastCommand>(cellsBuffer.Length, Allocator.TempJob);
//             //     var cellsCenterRaycastCommands = new NativeArray<RaycastCommand>(cellsBuffer.Length, Allocator.TempJob);
//             //     
//             //     var cellsOriginRaycastResults = new NativeArray<RaycastHit>(cellsBuffer.Length, Allocator.TempJob);
//             //     var cellsCenterRaycastResults = new NativeArray<RaycastHit>(cellsBuffer.Length, Allocator.TempJob);
//             //
//             //     // запустить джобу с рейкастами
//             //     var fillRaycastsCommandsJob = new FillRaycastCommandsJob {
//             //         FlowfieldCells = cellsBuffer,
//             //         CellsCenterRaycastCommands = cellsCenterRaycastCommands,
//             //         CellsOriginRaycastCommands = cellsOriginRaycastCommands
//             //     }.Schedule(Dependency);
//             //     var fireOriginRaycastsJob = RaycastCommand.ScheduleBatch(cellsOriginRaycastCommands, cellsOriginRaycastResults, 1, fillRaycastsCommandsJob);
//             //     var fireCenterRaycastsJob = RaycastCommand.ScheduleBatch(cellsCenterRaycastCommands, cellsCenterRaycastResults, 1, fillRaycastsCommandsJob);
//             //     var combinedDependencies = JobHandle.CombineDependencies(fireOriginRaycastsJob, fireCenterRaycastsJob, fillRaycastsCommandsJob);
//             //     //
//             //     // // запустить джобу заполнения параметров, передав в неё как зависимость джобу с рейкастами
//             //     var fillParametersJob = new FillParametersJob {
//             //             CellsBuffer = cellsBuffer,
//             //             CellsCenterRaycastCommands = cellsCenterRaycastCommands,
//             //             CellsCentersRaycastHits = cellsCenterRaycastResults,
//             //             CellsOriginRaycastCommands = cellsOriginRaycastCommands,
//             //             CellsOriginsRaycastHits = cellsOriginRaycastResults
//             //         }
//             //         .Schedule(combinedDependencies);
//             //     
//             //     Dependency = JobHandle.CombineDependencies(fillParametersJob, combinedDependencies);
//             //     request.IsProcessing = true;
//             //     // // в джобе заполнения удалить реквест
//             // }).WithoutBurst().Run();
//         }
//         
//         [BurstCompile]
//         private struct FillRaycastCommandsJob : IJob {
//             public DynamicBuffer<FlowfieldCellBufferElementData> FlowfieldCells;
//             public NativeArray<RaycastCommand> CellsOriginRaycastCommands;
//             public NativeArray<RaycastCommand> CellsCenterRaycastCommands;
//
//             public void Execute() {
//                 for (var i = 0; i < 57600; i++) {
//                     var cell = new FlowfieldCellBufferElementData();//FlowfieldCells[i];
//                     var perlinNoiseKekw = Mathf.PerlinNoise(i, Mathf.Sqrt(i));
//                     var cellOriginRay = new float3(cell.Value.WorldPosition.x, cell.Value.WorldPosition.y + 1000f, cell.Value.WorldPosition.z);
//                     CellsOriginRaycastCommands[i] = new RaycastCommand(cellOriginRay, Vector3.down);
//         
//                     var cellCenterRay = new float3(cell.Value.WorldCenter.x, cell.Value.WorldCenter.y + 1000f, cell.Value.WorldCenter.z);
//                     CellsCenterRaycastCommands[i] = new RaycastCommand(cellCenterRay, Vector3.down);
//                 }
//             }
//         }
//         
//         private struct FillParametersJob : IJob {
//             public DynamicBuffer<FlowfieldCellBufferElementData> CellsBuffer;
//             [DeallocateOnJobCompletion] public NativeArray<RaycastHit> CellsOriginsRaycastHits;
//             [DeallocateOnJobCompletion] public NativeArray<RaycastHit> CellsCentersRaycastHits;
//             [DeallocateOnJobCompletion] public NativeArray<RaycastCommand> CellsOriginRaycastCommands;
//             [DeallocateOnJobCompletion] public NativeArray<RaycastCommand> CellsCenterRaycastCommands;
//
//             public void Execute() {
//                 for (var i = 0; i < CellsBuffer.Length; i++) {
//                     var bufferElement = new FlowfieldCellBufferElementData();//CellsBuffer[i];
//                     bufferElement.Value.WorldPosition.y = CellsOriginsRaycastHits[i].point.y;
//                     bufferElement.Value.WorldCenter.y = CellsCentersRaycastHits[i].point.y;
//                         //CellsBuffer[i] = bufferElement;
//                 }
//                 CellsBuffer.Clear();
//             }
//         }
//         
//         private float FindBaseCost(float3 worldCenter, float3 worldPosition, FlowFieldConfigValueType flowFieldConfig) {
//             // Test normals from center of cell and left bottom edge of cell against world up vector.
//             // If angle is bigger than some threshold, it means that surface is too vertical and we can't move on it.
//             // Otherwise just set base cost depending on height.
//             var rayCenter = Physics.Raycast(new Vector3(worldCenter.x, worldCenter.y + 10, worldCenter.z), Vector3.down, out var hitCenter);
//             var rayLeftBottomEdge = Physics.Raycast(new Vector3(worldPosition.x, worldPosition.y + 10, worldPosition.z), Vector3.down, out var hitEdge);
//             var baseCost = 0f;
//             if (rayCenter && rayLeftBottomEdge) {
//                 var normalCenter = hitCenter.normal;
//                 var normalLeftBottom = hitEdge.normal;
//                 var angleCenter = Vector3.Angle(Vector3.up, normalCenter);
//                 var angleLeftBottom = Vector3.Angle(Vector3.up, normalLeftBottom);
//                 // if (angleCenter > _angleThreshold || angleLeftBottom > _angleThreshold) {
//                 //     baseCost = float.MaxValue;
//                 // } else {
//                 //     baseCost = 1f;
//                 //     if (worldCenter.y > _tooBigHeightThreshold) {
//                 //         baseCost += hitCenter.point.y;
//                 //     }
//                 // }
//             }
//             return baseCost;
//         }
//     }
// }