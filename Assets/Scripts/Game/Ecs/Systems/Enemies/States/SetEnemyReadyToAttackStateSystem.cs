using Game.AddressableConfigs;
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
    public partial class SetEnemyReadyToAttackStateSystem : SystemBase {
        private UnsafeList<FlowfieldCellComponent>.ParallelWriter _parentCellsWriter;
        private DependenciesScheduler _dependenciesScheduler;
        private FlowfieldRuntimeData _runtimeData;
        private EnemyStatsConfig _enemyStatsConfig;

        private bool _initialized;

        protected override void OnCreate() {
            _enemyStatsConfig = AddressablesLoader.Get<EnemyStatsConfig>(AddressablesConsts.DefaultEnemyStatsConfig);
        }

        public void InjectFlowfieldDependencies(DependenciesScheduler dependenciesScheduler, UnsafeList<FlowfieldCellComponent>.ParallelWriter parentCellsWriter, FlowfieldRuntimeData runtimeData) {
            _parentCellsWriter = parentCellsWriter;
            _dependenciesScheduler = dependenciesScheduler;
            _runtimeData = runtimeData;
            _initialized = true;
        }
        
        protected override unsafe void OnUpdate() {
            if (!_initialized) return;
            var inputDeps = JobHandle.CombineDependencies(_dependenciesScheduler.GetCombinedReadWriteDependencies(), Dependency);
            var writer = _parentCellsWriter;
            
            var origin = _runtimeData.ParentGridOrigin;
            var gridSize = _runtimeData.ParentGridSize;
            var cellSize = _runtimeData.ParentCellSize;
            var hivemindTarget = GetSingleton<CurrentHivemindTargetSingleton>();
            var checkNeighboringCells = _enemyStatsConfig.CheckNeighbourCells;
            
            Dependency = Entities.WithAll<Tag_Enemy>().ForEach((ref EnemyStateComponent enemyState, ref LocalToWorld ltw) => {
                if (enemyState.State != EnemyState.Moving) return;
                var currentIndex = FlowfieldUtility.CalculateIndexFromWorld(ltw.Position, origin, gridSize, cellSize);
                var targetIndex = FlowfieldUtility.CalculateIndexFromWorld(hivemindTarget.Value, origin, gridSize, cellSize);
                var currentCell = writer.ListData->Ptr[currentIndex];
                var targetCell = writer.ListData->Ptr[targetIndex];
                var arrivedAtCell = currentCell.GridPosition.x == targetCell.GridPosition.x && currentCell.GridPosition.y == targetCell.GridPosition.y;
                if (arrivedAtCell) {
                    enemyState.State = EnemyState.ReadyToAttack;
                    return;
                }

                if (checkNeighboringCells) {
                    var neighbourOfsets = FlowfieldUtility.GetNeighbourOffsets();
                    for (int i = 0; i < neighbourOfsets.Length; i++) {
                        var gridPos = targetCell.GridPosition;
                        gridPos += neighbourOfsets[i];
                        var closeToTarget = !FlowfieldUtility.TileOutOfGrid(gridPos, gridSize) && (currentCell.GridPosition.x == gridPos.x && currentCell.GridPosition.y == gridPos.y);
                        if (closeToTarget) {
                            enemyState.State = EnemyState.ReadyToAttack;
                            break;
                        }
                    }
                    neighbourOfsets.Dispose();
                }
            }).Schedule(inputDeps);
            
            _dependenciesScheduler.AddExternalReadWriteDependency(Dependency);
        }
    }
}