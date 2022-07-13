using Game.Ecs.Components.Pathfinding;
using Game.Ecs.Components.Tags;
using Game.Ecs.Systems.Pathfinding;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Utils.Pathfinding;

namespace Game.Ecs.Systems.Spawners {
    public partial class AddEnemiesToGridSystem : SystemBase {
        private UnsafeList<FlowfieldCellComponent>.ParallelWriter _parentCellsWriter;
        private DependenciesScheduler _dependenciesScheduler;
        private FlowfieldRuntimeData _flowfieldRuntimeData;
        
        public void InjectFlowfieldDependencies(UnsafeList<FlowfieldCellComponent>.ParallelWriter parentCellsWriter, DependenciesScheduler dependenciesScheduler, FlowfieldRuntimeData flowfieldRuntimeData) {
            _parentCellsWriter = parentCellsWriter;
            _dependenciesScheduler = dependenciesScheduler;
            _flowfieldRuntimeData = flowfieldRuntimeData;
        }
        
		protected override unsafe void OnUpdate() {
            var writer = _parentCellsWriter;
            var deps = _dependenciesScheduler.GetCombinedReadWriteDependencies();
            var flowfieldData = _flowfieldRuntimeData;
            
            Dependency = Entities.WithAll<Tag_Enemy>().ForEach((in LocalToWorld ltw, in Entity entity) => {
                var parentCellIndex = FlowfieldUtility.CalculateIndexFromWorld(
                    ltw.Position, flowfieldData.ParentGridOrigin, flowfieldData.ParentGridSize, flowfieldData.ParentCellSize);
                var parentCell = writer.ListData->Ptr[parentCellIndex];
                if (parentCell.Unwalkable) return;
                parentCell.Entities.Add(entity);
            }).Schedule(JobHandle.CombineDependencies(deps, Dependency));
            
            _dependenciesScheduler.AddExternalReadWriteDependency(Dependency);
            // add: get cell from world position
            // remove: get ComponentDataFromEntity<ltw> and see if we are still here
        }
    }
}