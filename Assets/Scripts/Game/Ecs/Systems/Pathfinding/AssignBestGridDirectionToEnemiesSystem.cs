using Game.AddressableConfigs;
using Game.Ecs.Components.Enemies;
using Game.Ecs.Components.Pathfinding;
using Game.Ecs.Components.Tags;
using Game.Ecs.Systems.Pathfinding;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Utils;
using Utils.Pathfinding;

namespace Game.Ecs.Systems.Spawners {
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    public partial class AssignBestGridDirectionToEnemiesSystem : SystemBase {
        private DependenciesScheduler _dependenciesScheduler;
        private UnsafeList<FlowfieldCellComponent>.ParallelWriter _parentCellsWriter;
        private FlowfieldRuntimeData _flowfieldRuntimeData;
        private EnemyStatsConfig _enemyStatsConfig;

        private bool _initialized;

        protected override void OnCreate() {
            _enemyStatsConfig = AddressablesLoader.Get<EnemyStatsConfig>(AddressablesConsts.DefaultEnemyStatsConfig);
        }

        public void InjectFlowfieldDependencies(DependenciesScheduler dependenciesScheduler, UnsafeList<FlowfieldCellComponent>.ParallelWriter parentCellsWriter,
            FlowfieldRuntimeData runtimeData) {
            _dependenciesScheduler = dependenciesScheduler;
            _parentCellsWriter = parentCellsWriter;
            _flowfieldRuntimeData = runtimeData;
            _initialized = true;
        }

        protected override unsafe void OnUpdate() {
            if (!_initialized) return;
            var parentCellsWriter = _parentCellsWriter;
            var parentGridOrigin = _flowfieldRuntimeData.ParentGridOrigin;
            var parentCellSize = _flowfieldRuntimeData.ParentCellSize;
            var parentGridSize = _flowfieldRuntimeData.ParentGridSize;
            var childGridSize = _flowfieldRuntimeData.ChildGridSize;
            var childCellSize = _flowfieldRuntimeData.ChildCellSize;
            var maxCounter = _enemyStatsConfig.GridDirectionUpdateSkipCount;
            
            Entities.WithAll<Tag_Enemy>().ForEach((ref BestEnemyGridDirectionComponent gridDirection, ref GridDirectionUpdateSkipCounterComponent counter, 
                in LocalToWorld ltw, in EnemyStateComponent enemyState, in BestEnemyLocalAvoidanceDirection localDirection) => {
                if (enemyState.Value == EnemyState.Attacking) return;
                if (counter.Value > 0) {
                    counter.Value--;
                    return;
                }
                counter.Value = maxCounter;
                var enemyPos = ltw.Position;
                var toGrid = FlowfieldUtility.ToGrid(enemyPos, parentGridOrigin, parentCellSize);
                if (FlowfieldUtility.TileOutOfGrid(toGrid, parentGridSize)) return;
                var parentCellIndex = FlowfieldUtility.CalculateIndexFromWorld(enemyPos, parentGridOrigin, parentGridSize, parentCellSize);
                var parentCell = parentCellsWriter.ListData->Ptr[parentCellIndex];
                if (FlowfieldUtility.TileOutOfGrid(parentCell.GridPosition, parentGridSize)) return;
                if (!parentCell.IsCreated) return;
                
                var childCellsWriter = parentCell.ChildCells;
                if (childCellsWriter.ListData->Length <= 0) return;
                
                var childCellIndex = FlowfieldUtility.CalculateIndexFromWorld(enemyPos, parentCell.WorldPosition, childGridSize, childCellSize);
                var childCell = childCellsWriter.ListData->Ptr[childCellIndex];
                if (!childCell.IsCreated) return;
                
                var bestDirectionOnGrid = childCell.BestFlowfieldDirection;
                var bestWorldDirection = math.normalizesafe(new float2(bestDirectionOnGrid.x, bestDirectionOnGrid.y));
                gridDirection.Value += bestWorldDirection;
                gridDirection.Value = math.normalizesafe(gridDirection.Value);
            }).ScheduleParallel();
        }
    }
}