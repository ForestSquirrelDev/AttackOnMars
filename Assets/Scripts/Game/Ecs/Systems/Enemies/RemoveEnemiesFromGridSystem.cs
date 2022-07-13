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
            var cellEntitiesJob = new ClearCellsEntitiesJob(localToWorldData, _parentCellsWriter, _flowfieldRuntimeData);
            Dependency = cellEntitiesJob.Schedule(_parentCellsWriter.ListData->Length, deps);
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
                var cell = _parentCellsWriter.ListData->Ptr[index];
                foreach (var entity in cell.Entities) {
                    if (!_ltwData.HasComponent(entity)) {
                        cell.Entities.Remove(entity);
                        continue;
                    }
                    
                    var ltw = _ltwData[entity];
                    var testedCellIndex = FlowfieldUtility.CalculateIndexFromGrid(cell.GridPosition, _flowfieldData.ParentGridSize);
                    var entityCellIndex = FlowfieldUtility.CalculateIndexFromWorld(ltw.Position, _flowfieldData.ParentGridOrigin, _flowfieldData.ParentGridSize, _flowfieldData.ParentCellSize);
                    if (testedCellIndex != entityCellIndex) {
                        cell.Entities.Remove(entity);
                    }
                }
            }
        }
    }
}
