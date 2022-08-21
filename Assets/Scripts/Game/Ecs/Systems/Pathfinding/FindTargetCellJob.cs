using Game.Ecs.Components.Pathfinding;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Utils.Pathfinding;

namespace Game.Ecs.Systems.Pathfinding {
    [BurstCompile]
    public struct FindTargetCellJob : IJob {
        private int _currentParentCellIndex;
        private float3 _worldTarget;
        private NativeArray<FlowfieldCellComponent> _bestCellOut;
        private UnsafeList<FlowfieldCellComponent>.ParallelWriter _parentCellsWriter;
        private FlowfieldRuntimeData _runtimeData;

        public unsafe FindTargetCellJob(int parentCellIndex, float3 worldTarget, NativeArray<FlowfieldCellComponent> bestCellOut, UnsafeList<FlowfieldCellComponent>.ParallelWriter parentCellsWriter, FlowfieldRuntimeData runtimeData) {
            _currentParentCellIndex = parentCellIndex;
            _worldTarget = worldTarget;
            _bestCellOut = bestCellOut;
            _parentCellsWriter = parentCellsWriter;
            _runtimeData = runtimeData;
        }
            
        public unsafe void Execute() {
            var targetParentCellIndex = FlowfieldUtility.CalculateIndexFromWorld(_worldTarget, _runtimeData.ParentGridOrigin, _runtimeData.ParentGridSize, _runtimeData.ParentCellSize);
            var targetParentCell = _parentCellsWriter.ListData->Ptr[targetParentCellIndex];
            var currentParentCell = _parentCellsWriter.ListData->Ptr[_currentParentCellIndex];
            FlowfieldCellComponent bestCellOut;
            var childCells = currentParentCell.ChildCells;
                
            if (currentParentCell == targetParentCell) {
                var bestChildCellIndex = FlowfieldUtility.CalculateIndexFromWorld(_worldTarget, targetParentCell.WorldPosition, _runtimeData.ChildGridSize, _runtimeData.ChildCellSize);
                bestCellOut = childCells.ListData->Ptr[bestChildCellIndex];
            } else {
                bestCellOut = FindClosestCellToNextBestCell(targetParentCell, currentParentCell, currentParentCell.ChildCells,
                    _runtimeData.ChildGridSize, _runtimeData.ChildCellSize);
            }
            
            bestCellOut.IsBestCell = true;
            var bestCellDir = math.normalize(currentParentCell.BestFlowfieldDirection);
          //  Debug.Log($"Best cell dir: {bestCellDir}");
            bestCellOut.BestFlowfieldDirection = new int2(Mathf.RoundToInt(bestCellDir.x), Mathf.RoundToInt(bestCellDir.y));
            _bestCellOut[0] = bestCellOut;
          //  Debug.Log($"%%%%%%%%%%%%%%{bestCellOut.WorldPosition}%%%%%%%%%%");

          //  Debug.Log($"Child cells cell is best: {childCells.ListData->Ptr[FlowfieldUtility.CalculateIndexFromGrid(bestCellOut.GridPosition, _runtimeData.ChildGridSize)].IsBestCell}");
            var ind = FlowfieldUtility.CalculateIndexFromGrid(bestCellOut.GridPosition, _runtimeData.ChildGridSize);
          //  Debug.Log($"Ind: {ind}");
            childCells.ListData->Ptr[ind] = bestCellOut;
           // Debug.Log($"{childCells.ListData->Ptr[ind].IsBestCell}. Grid pos: {bestCellOut.GridPosition}. World pos: {bestCellOut.WorldPosition}");
          //  Debug.Log($"Child cells cell is best: {childCells.ListData->Ptr[FlowfieldUtility.CalculateIndexFromGrid(bestCellOut.GridPosition, _runtimeData.ChildGridSize)].IsBestCell}");
        }
            
        private unsafe FlowfieldCellComponent FindClosestCellToNextBestCell(FlowfieldCellComponent bestDirectionParentCell, FlowfieldCellComponent currentParentCell, 
            UnsafeList<FlowfieldCellComponent>.ParallelWriter currentParentCellChildCells, int2 childCellsGridSize, float childCellSize) {
            var directionFromCellCenterToBestParentCell = currentParentCell.BestFlowfieldDirection;
            var bestChildCell = new FlowfieldCellComponent();
            var bestCellWorldPosition = currentParentCell.WorldCenter;
            var bestCellGridPosition = FlowfieldUtility.ToGrid(bestCellWorldPosition, currentParentCell.WorldPosition, childCellSize);

            var infiniteLoopCounter = 20;
            while (true) {
                if (FlowfieldUtility.TileOutOfGrid(bestCellGridPosition, _runtimeData.ChildGridSize)) {
                    break;
                }
                var bestCellIndex = FlowfieldUtility.CalculateIndexFromGrid(bestCellGridPosition, _runtimeData.ChildGridSize);
                var testedCell = currentParentCellChildCells.ListData->Ptr[bestCellIndex];
             //   Debug.Log($"Best cell index: {bestCellIndex}. Origin: {currentParentCell.WorldPosition}. Grid size: {childCellsGridSize}. Cellsize: {childCellSize}");
//                Debug.Log($"!!!!!!!!!!!!!!!!!!!!!Best cell index: {bestCellIndex}. Best child cell grid pos: {testedCell.GridPosition}. Best cell world pos: {testedCell.WorldPosition}. Proposed pos : {directionFromCellCenterToBestParentCell}. ASdfsaodfkosdaf: {bestCellWorldPosition} !!!!!!!!!!!!!!!!!!!!!");

                bestCellGridPosition += directionFromCellCenterToBestParentCell;
               // Debug.Log($"-!-! World: {testedCell.WorldPosition} World rect: {currentParentCell.WorldRect.Size}. XMin: {currentParentCell.WorldRect.XMin}" +
              //            $"XMax: {currentParentCell.WorldRect.XMax}. YMin: {currentParentCell.WorldRect.YMin}. YMax: {currentParentCell.WorldRect.YMax} -!-!");
                bestChildCell = testedCell;
                infiniteLoopCounter--;
                if (infiniteLoopCounter <= 0) {
                    Debug.LogError($"FindClosestToBestParentCellJob.Infinite loop");
                    break;
                }
                if (currentParentCell.IsBestCell) {
                    break;
                }
            }

            //Debug.Log($"Final. Child cell pos: {bestChildCell.WorldPosition}. Grid pos: {bestChildCell.GridPosition} + {bestChildCell.WorldCenter}");
            return bestChildCell;
        }
    }
}
