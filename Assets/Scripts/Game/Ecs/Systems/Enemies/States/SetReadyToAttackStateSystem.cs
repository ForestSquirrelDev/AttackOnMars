using Game.Ecs.Components.Enemies;
using Game.Ecs.Components.Pathfinding;
using Game.Ecs.Components.Tags;
using Game.Ecs.Systems.Pathfinding;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
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
            int counter = 0;
            var inputDeps = JobHandle.CombineDependencies(_dependenciesHandler.GetCombinedReadWriteDependencies(), Dependency);
            var writer = _parentCellsWriter;
            
            var origin = _runtimeData.ParentGridOrigin;
            var gridSize = _runtimeData.ParentGridSize;
            var cellSize = _runtimeData.ParentCellSize;
            var hivemindTarget = GetSingleton<CurrentHivemindTargetSingleton>();
            
            Dependency = Entities.WithAll<Tag_Enemy>().ForEach((ref EnemyStateComponent enemyState, ref LocalToWorld ltw) => {
                //Debug.Log($"ReadyToAttack.OnBeforeCheck");
                if (enemyState.Value != EnemyState.Moving) return;
                var currentIndex = FlowfieldUtility.CalculateIndexFromWorld(ltw.Position, origin, gridSize, cellSize);
                var targetIndex = FlowfieldUtility.CalculateIndexFromWorld(hivemindTarget.Value, origin, gridSize, cellSize);
                var currentCell = writer.ListData->Ptr[currentIndex];
                var targetCell = writer.ListData->Ptr[targetIndex];
                var neighbourOfsets = FlowfieldUtility.GetNeighbourOffsets();
                var closeToTarget = new NativeArray<bool>(neighbourOfsets.Length, Allocator.Temp);
                for (int i = 0; i < closeToTarget.Length; i++) {
                    var gridPos = targetCell.GridPosition;
                    gridPos += neighbourOfsets[i];
                    closeToTarget[i] = currentCell.GridPosition.x == gridPos.x && currentCell.GridPosition.y == gridPos.y;
                }
                var arrivedAtCell = currentCell.GridPosition.x == targetCell.GridPosition.x && currentCell.GridPosition.y == targetCell.GridPosition.y;
                if (arrivedAtCell || Any(closeToTarget)) {
                    enemyState.Value = EnemyState.ReadyToAttack;
                }
                neighbourOfsets.Dispose();
                closeToTarget.Dispose();
            }).Schedule(inputDeps);
            
            _dependenciesHandler.AddExternalReadWriteDependency(Dependency);
        }

        private static bool Any(NativeArray<bool> bools) {
            foreach (var value in bools) {
                if (value) return true;
            }
            return false;
        }
    }
}