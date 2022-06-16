using Game.Ecs.Components.Enemies;
using Game.Ecs.Components.Pathfinding;
using Game.Ecs.Components.Tags;
using Game.Ecs.Systems.Pathfinding;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Utils.Pathfinding;

namespace Game.Ecs.Systems.Spawners {
    public partial class SetReadyToAttackStateSystem : SystemBase {
        private UnsafeList<FlowfieldCellComponent>.ParallelWriter _parentCellsWriter;
        private FlowfieldJobDependenciesHandler _dependenciesHandler;
        private FlowfieldRuntimeData _runtimeData;

        private bool _initialized;

        public void InjectFlowfieldDependencies(FlowfieldJobDependenciesHandler dependenciesHandler, UnsafeList<FlowfieldCellComponent>.ParallelWriter parentCellsWriter, FlowfieldRuntimeData runtimeData) {
            _parentCellsWriter = parentCellsWriter;
            _dependenciesHandler = dependenciesHandler;
            _runtimeData = runtimeData;
            _initialized = true;
        }
        
        protected override unsafe void OnUpdate() {
            if (!_initialized) return;
            var inputDeps = JobHandle.CombineDependencies(_dependenciesHandler.GetCombinedReadWriteDependencies(), Dependency);
            var writer = _parentCellsWriter;
            
            var origin = _runtimeData.ParentGridOrigin;
            var gridSize = _runtimeData.ParentGridSize;
            var cellSize = _runtimeData.ParentCellSize;
            var hivemindTarget = GetSingleton<CurrentHivemindTargetSingleton>();
            
            Dependency = Entities.WithAll<Tag_Enemy>().ForEach((ref EnemyStateComponent enemyState, in Translation translation) => {
                if (enemyState.Value != EnemyState.Moving) return;
                var currentCell = writer.ListData->Ptr[FlowfieldUtility.CalculateIndexFromWorld(translation.Value, origin, gridSize, cellSize)];
                var targetCell = writer.ListData->Ptr[FlowfieldUtility.CalculateIndexFromWorld(hivemindTarget.Value, origin, gridSize, cellSize)];
                if (currentCell == targetCell) {
                    enemyState.Value = EnemyState.ReadyToAttack;
                }
            }).WithoutBurst().ScheduleParallel(inputDeps);
            
            _dependenciesHandler.AddExternalReadWriteDependency(Dependency);
        }
    }
}