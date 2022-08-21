/*using System.Collections;
using Game.Ecs.Components.Pathfinding;
using Game.Ecs.Systems.Pathfinding;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Utils.Pathfinding;

namespace Game.Ecs.Systems.Spawners {
    public partial class LocalAvoidanceSystemV2 : SystemBase {
        private UnsafeList<FlowfieldCellComponent>.ParallelWriter _parentCellsWriter;
        private FlowfieldRuntimeData _flowfieldData;
        
        public void InjectFlowfieldDependencies(UnsafeList<FlowfieldCellComponent>.ParallelWriter parentCellsWriter, FlowfieldRuntimeData flowfieldData) {
            _parentCellsWriter = parentCellsWriter;
            _flowfieldData = flowfieldData;
        }
        
		protected override unsafe void OnUpdate() {
            return;
            var localDirectionJob = new FormBestLocalDirectionJob(_parentCellsWriter);
            Dependency = localDirectionJob.Schedule(_parentCellsWriter.ListData->Length, Dependency);
        }
        
        [BurstCompile]
        private readonly struct FormBestLocalDirectionJob : IJobFor {
            private readonly UnsafeList<FlowfieldCellComponent>.ParallelWriter _parentCellsWriter;

            public FormBestLocalDirectionJob(UnsafeList<FlowfieldCellComponent>.ParallelWriter parentCellsWriter) {
                _parentCellsWriter = parentCellsWriter;
            }
            
            public unsafe void Execute(int index) {
                var parentCell = _parentCellsWriter.ListData->Ptr[index];
                if (parentCell.Unwalkable || !parentCell.IsCreated) return;
                
                var childCells = parentCell.ChildCells;
                for (int testedCellIndexer = 0; testedCellIndexer < childCells.ListData->Length; testedCellIndexer++) {
                    var childCell = childCells.ListData->Ptr[testedCellIndexer];
                    if (childCell.Unwalkable || !childCell.IsCreated) return;

                    if (childCell.Entities.IsEmpty) {
                        //Debug.Log($"Parent cell: {parentCell.WorldPosition}. Child cell: {childCell.WorldPosition}. Empty");
                        childCell.BestLocalDirection = int2.zero;
                        childCells.ListData->Ptr[testedCellIndexer] = childCell; 
                        continue;
                    }
                    
                    //c.
                    // у flowfieldcell есть очередь лучших направлений, которые entity разбирают как горячие пирожки
                    // LocalAvoidanceSystem ставит лучшеие направления в очередь, сортируя их в порядке в зависимости от того насколько направление клетки совпадает с её
                    // У каждого направления есть вес. Если вес направления больше того что есть у сущности, она может взять его вместо своего. Но если клетка с этим направлением дальше от той к которой движется сущность
                    // сейчас, то оно игнорируется.
                    for (int neighbourCellIndexer = 0; neighbourCellIndexer < FlowfieldNeighbours.Count; neighbourCellIndexer++) {
                        var neighbourIndex = childCell.NeighboursIndexes[neighbourCellIndexer];
                        if (FlowfieldUtility.IsOutOfRange(childCells.ListData->Length, neighbourIndex)) continue;
                        
                        var neighbourCell = childCells.ListData->Ptr[neighbourIndex];
                        if (neighbourCell.Unwalkable || !neighbourCell.IsCreated) continue;

                        if (neighbourCell.Entities.IsEmpty) {
                            childCell.BestLocalDirection = neighbourCell.GridPosition - childCell.GridPosition;
                            //Debug.Log($"Set best local direction. Child cell: {childCell.WorldPosition}. Parent cell: {parentCell.WorldPosition}. Best neigbhour: {neighbourCell.WorldPosition}. Direction: {childCell.BestLocalDirection}");
                            //Debug.Log($"Child cell: {childCell}");
                            childCells.ListData->Ptr[testedCellIndexer] = childCell;
                            break;
                        }
                    }
                }
            }
        }
    }
}*/