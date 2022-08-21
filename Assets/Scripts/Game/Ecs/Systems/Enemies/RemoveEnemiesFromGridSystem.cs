using Game.Ecs.Components.Pathfinding;
using Game.Ecs.Systems.Pathfinding;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Utils.Pathfinding;

namespace Game.Ecs.Systems.Spawners {
    public partial class RemoveEnemiesFromGridSystem : SystemBase {
        private UnsafeList<FlowfieldCellComponent>.ParallelWriter _parentCellsWriter;
        private DependenciesScheduler _flowfieldDependenciesScheduler;
        private FlowfieldRuntimeData _flowfieldRuntimeData;
        
        public void InjectFlowfieldDependencies(UnsafeList<FlowfieldCellComponent>.ParallelWriter parentCellsWriter, DependenciesScheduler dependenciesScheduler, FlowfieldRuntimeData flowfieldRuntimeData) {
            _parentCellsWriter = parentCellsWriter;
            _flowfieldDependenciesScheduler = dependenciesScheduler;
            _flowfieldRuntimeData = flowfieldRuntimeData;
        }
        
        protected override unsafe void OnUpdate() {
            var localToWorldData = GetComponentDataFromEntity<LocalToWorld>(true);
            var deps = JobHandle.CombineDependencies(_flowfieldDependenciesScheduler.GetCombinedReadWriteDependencies(), Dependency);
            var clearCellsEntitiesJob = new ClearCellsEntitiesJob(localToWorldData, _parentCellsWriter, _flowfieldRuntimeData);
            Dependency = clearCellsEntitiesJob.Schedule(_parentCellsWriter.ListData->Length, deps);
            _flowfieldDependenciesScheduler.AddExternalReadWriteDependency(Dependency);
        }
        
        [BurstCompile]
        private struct ClearCellsEntitiesJob : IJobFor {
            [ReadOnly] private ComponentDataFromEntity<LocalToWorld> _ltwData;
            private UnsafeList<FlowfieldCellComponent>.ParallelWriter _parentCellsWriter;
            private FlowfieldRuntimeData _flowfieldData;

            public ClearCellsEntitiesJob(ComponentDataFromEntity<LocalToWorld> ltwData, UnsafeList<FlowfieldCellComponent>.ParallelWriter parentCellsWriter, FlowfieldRuntimeData flowfieldData) {
                _ltwData = ltwData;
                _parentCellsWriter = parentCellsWriter;
                _flowfieldData = flowfieldData;
            }
            
            public unsafe void Execute(int index) {
                var parentCell = _parentCellsWriter.ListData->Ptr[index];
                var childCells = parentCell.ChildCells;
                
                for (int i = 0; i < childCells.ListData->Length; i++) {
                    var childCell = childCells.ListData->Ptr[i];
                    var entities = childCell.Entities;
                    if (!entities.IsCreated) return;
                    
                    foreach (var entity in childCell.Entities) {
                        if (!_ltwData.HasComponent(entity)) {
                            entities.Remove(entity);
                            continue;
                        }
                    
                        var ltw = _ltwData[entity];
                        var testedCellIndex = FlowfieldUtility.CalculateIndexFromGrid(childCell.GridPosition, _flowfieldData.ChildGridSize);
                        var entityCellIndex = FlowfieldUtility.CalculateIndexFromWorld(ltw.Position, parentCell.WorldPosition, _flowfieldData.ChildGridSize, _flowfieldData.ChildCellSize);
                        if (testedCellIndex != entityCellIndex) {
                            entities.Remove(entity);
                        }
                    }
                    //
                }
            }
        }
    }
}
