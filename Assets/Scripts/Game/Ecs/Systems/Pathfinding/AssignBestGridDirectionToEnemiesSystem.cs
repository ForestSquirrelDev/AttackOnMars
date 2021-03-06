using Game.Ecs.Components.Enemies;
using Game.Ecs.Components.Pathfinding;
using Game.Ecs.Components.Tags;
using Game.Ecs.Systems.Pathfinding;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Utils.Pathfinding;

namespace Game.Ecs.Systems.Spawners {
    public partial class AssignBestGridDirectionToEnemiesSystem : SystemBase {
        private DependenciesScheduler _dependenciesScheduler;
        private UnsafeList<FlowfieldCellComponent>.ParallelWriter _parentCellsWriter;
        private FlowfieldRuntimeData _flowfieldRuntimeData;

        private bool _initialized;

        public void InjectFlowfieldDependencies(DependenciesScheduler dependenciesScheduler, UnsafeList<FlowfieldCellComponent>.ParallelWriter parentCellsWriter,
            FlowfieldRuntimeData runtimeData) {
            _dependenciesScheduler = dependenciesScheduler;
            _parentCellsWriter = parentCellsWriter;
            _flowfieldRuntimeData = runtimeData;
            _initialized = true;
        }

        protected override unsafe void OnUpdate() {
            if (!_initialized) return;
            var inputDeps = JobHandle.CombineDependencies(Dependency, _dependenciesScheduler.GetCombinedReadWriteDependencies());
            var parentCellsWriter = _parentCellsWriter;
            var parentGridOrigin = _flowfieldRuntimeData.ParentGridOrigin;
            var parentCellSize = _flowfieldRuntimeData.ParentCellSize;
            var parentGridSize = _flowfieldRuntimeData.ParentGridSize;
            var childGridSize = _flowfieldRuntimeData.ChildGridSize;
            var childCellSize = _flowfieldRuntimeData.ChildCellSize;
            
            Dependency = Entities.WithAll<Tag_Enemy>().ForEach((ref BestEnemyGridDirectionComponent bestDirectionComponent, in LocalToWorld ltw, in EnemyStateComponent enemyState) => {
                if (enemyState.Value == EnemyState.Attacking) return;
                var enemyPos = ltw.Position;
                var toGrid = FlowfieldUtility.ToGrid(enemyPos, parentGridOrigin, parentCellSize);
                if (FlowfieldUtility.TileOutOfGrid(toGrid, parentGridSize)) return;
                var parentCellIndex = FlowfieldUtility.CalculateIndexFromWorld(enemyPos, parentGridOrigin, parentGridSize, parentCellSize);
                var parentCell = parentCellsWriter.ListData->Ptr[parentCellIndex];
                if (FlowfieldUtility.TileOutOfGrid(parentCell.GridPosition, parentGridSize)) return;
                if (parentCell.Unwalkable) return;
                
                var childCellsWriter = parentCell.ChildCells;
                if (childCellsWriter.ListData->Length <= 0) return;
                
                var childCellIndex = FlowfieldUtility.CalculateIndexFromWorld(enemyPos, parentCell.WorldPosition, childGridSize, childCellSize);
                var childCell = childCellsWriter.ListData->Ptr[childCellIndex];
                var bestDirectionOnGrid = childCell.BestDirection;
                var bestWorldDirection = math.normalize(new float3(bestDirectionOnGrid.x, math.normalize(childCell.WorldCenter - ltw.Position).y, bestDirectionOnGrid.y));
                bestDirectionComponent.Value = bestWorldDirection;
            }).Schedule(inputDeps);
            
            _dependenciesScheduler.AddExternalReadWriteDependency(Dependency);
        }
    }
}