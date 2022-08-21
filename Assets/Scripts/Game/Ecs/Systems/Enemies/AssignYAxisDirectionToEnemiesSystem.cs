using Game.Ecs.Components.Enemies;
using Game.Ecs.Components.Pathfinding;
using Game.Ecs.Components.Tags;
using Game.Ecs.Systems.Pathfinding;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Utils.Pathfinding;

namespace Game.Ecs.Systems.Spawners {
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    public partial class AssignYAxisDirectionToEnemiesSystem : SystemBase {
        private UnsafeList<FlowfieldCellComponent>.ParallelWriter _parentCellsWriter;
        private FlowfieldRuntimeData _runtimeData;

        public void InjectFlowfieldDependencies(UnsafeList<FlowfieldCellComponent>.ParallelWriter parentCells, FlowfieldRuntimeData runtimeData) {
            _parentCellsWriter = parentCells;
            _runtimeData = runtimeData;
        }

        protected override unsafe void OnUpdate() {
            var runtimeData = _runtimeData;
            var parentCells = _parentCellsWriter;
            
            Entities.WithAll<Tag_Enemy>().ForEach((ref YAxisEnemyDirectionComponent direction, in Translation translation) => {
                var parentCellIndex = FlowfieldUtility.CalculateIndexFromWorld(translation.Value, runtimeData.ParentGridOrigin, runtimeData.ParentGridSize, runtimeData.ParentCellSize);
                var parentCell = parentCells.ListData->Ptr[parentCellIndex];
                if (!parentCell.IsCreated || parentCell.Unwalkable) return;

                var childCellIndex = FlowfieldUtility.CalculateIndexFromWorld(translation.Value, parentCell.WorldPosition, runtimeData.ChildGridSize, runtimeData.ChildCellSize);
                var childCell = parentCell.ChildCells.ListData->Ptr[childCellIndex];
                if (!childCell.IsCreated || childCell.Unwalkable) return;

                var directionToCellCenter = math.normalizesafe(childCell.WorldCenter - translation.Value);
                direction.Value = directionToCellCenter.y;
            }).ScheduleParallel();
        }
    }
}