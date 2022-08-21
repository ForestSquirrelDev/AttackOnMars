using Game.AddressableConfigs;
using Game.Ecs.Components.Enemies;
using Game.Ecs.Components.Pathfinding;
using Game.Ecs.Components.Tags;
using Game.Ecs.Systems.Pathfinding;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Utils.Maths;
using Utils.Pathfinding;

namespace Game.Ecs.Systems.Spawners {
    public partial class LocalAvoidanceSystemV3 : SystemBase {
        private UnsafeList<FlowfieldCellComponent>.ParallelWriter _parentCellsWriter;
        private DependenciesScheduler _flowfieldDependenciesScheduler;
        private FlowfieldRuntimeData _flowfieldRuntimeData;
        private LocalAvoidanceConfig _localAvoidanceConfig;
        private int _counter;

        protected override void OnCreate() {
            _localAvoidanceConfig = AddressablesLoader.Get<LocalAvoidanceConfig>(AddressablesConsts.LocalAvoidanceConfig);
        }

        public void InjectFlowfieldDependencies(UnsafeList<FlowfieldCellComponent>.ParallelWriter parentCellsWriter, DependenciesScheduler flowfieldDependenciesScheduler, FlowfieldRuntimeData runtimeData) {
            _parentCellsWriter = parentCellsWriter;
            _flowfieldDependenciesScheduler = flowfieldDependenciesScheduler;
            _flowfieldRuntimeData = runtimeData;
        }
        
		protected override unsafe void OnUpdate() {
            var writer = _parentCellsWriter;
            var runtimeData = _flowfieldRuntimeData;
            var localtoworldData = GetComponentDataFromEntity<LocalToWorld>(true);
            var maxDistanceSq = _localAvoidanceConfig.MaxDistanceSquared;
            var sigmoidK = _localAvoidanceConfig.TunableSigmoidK;
            var framesToSkip = UnityEngine.Random.Range(_localAvoidanceConfig.FramesSkipRange.x, _localAvoidanceConfig.FramesSkipRange.y);
            var maxVectorLength = _localAvoidanceConfig.MaxVectorLength;

            Entities.WithAll<Tag_Enemy>().ForEach((ref BestEnemyLocalAvoidanceDirection bestEnemyLocalDirection, ref LocalAvoidanceTickCounterComponent counter, in Translation testedTranslation, in Entity testedEntity) => {
                if (counter.Value > 0) {
                    counter.Value--;
                    return;
                }
                counter.Value = framesToSkip;
                // find child cell we are standing on
                var parentCellIndex = FlowfieldUtility.CalculateIndexFromWorld(testedTranslation.Value, runtimeData.ParentGridOrigin, runtimeData.ParentGridSize, runtimeData.ParentCellSize);
                if (FlowfieldUtility.IsOutOfRange(writer.ListData->Length, parentCellIndex)) return;

                var parentCell = writer.ListData->Ptr[parentCellIndex];
                if (!parentCell.IsCreated || parentCell.Unwalkable) return;

                var childCellIndex = FlowfieldUtility.CalculateIndexFromWorld(testedTranslation.Value, parentCell.WorldPosition, runtimeData.ChildGridSize, runtimeData.ChildCellSize);
                if (FlowfieldUtility.IsOutOfRange(parentCell.ChildCells.ListData->Length, childCellIndex)) return;

                var childCell = parentCell.ChildCells.ListData->Ptr[childCellIndex];
                if (!childCell.IsCreated || childCell.Unwalkable) return;

                var neighboursIndexes = childCell.NeighboursIndexes;

                // get neighbour cells and entities that those cells contain
                var neighbourEntities = new NativeList<Entity>(8, Allocator.Temp);
                for (int i = 0; i < FlowfieldNeighbours.Count; i++) {
                    var index = neighboursIndexes[i];
                    if (FlowfieldUtility.IsOutOfRange(parentCell.ChildCells.ListData->Length, index)) continue;

                    var neighbourCell = parentCell.ChildCells.ListData->Ptr[index];
                    if (neighbourCell.Unwalkable || !neighbourCell.IsCreated) continue;

                    var entitiesOnCell = neighbourCell.Entities;
                    if (entitiesOnCell.IsEmpty) continue;

                    foreach (var neighbourEntity in entitiesOnCell) {
                        neighbourEntities.Add(neighbourEntity);
                    }
                }

                // get entities that are on same cell that we are on
                var childCellEntities = childCell.Entities;
                if (!childCellEntities.IsEmpty) {
                    foreach (var nearbyEntity in childCell.Entities) {
                        if (nearbyEntity != testedEntity) {
                            neighbourEntities.Add(nearbyEntity);
                        }
                    }
                }
                if (neighbourEntities.Length <= 1) {
                    bestEnemyLocalDirection.Value = float2.zero;
                    neighbourEntities.Dispose();
                    return;
                }

                var resultingLocalVector = new float3(0, 0, 0);
                foreach (var neighbourEntity in neighbourEntities) {
                    if (!localtoworldData.TryGetComponent(neighbourEntity, out var ltw)) continue;

                    var neighbourToTestedEntityVector = math.normalizesafe(ltw.Position - testedTranslation.Value);
                    var distSq = math.distancesq(ltw.Position, testedTranslation.Value);
                    var t = math.clamp(distSq / maxDistanceSq, 0, 1);
                    var significanceCoefficient = MathfUtility.ReverseTunableSigmoid(sigmoidK, t);
                    neighbourToTestedEntityVector *= significanceCoefficient;
                    resultingLocalVector += neighbourToTestedEntityVector;
                }
                resultingLocalVector.y = 0;

                var float2LocalVector = math.normalizesafe(new float2(resultingLocalVector.x, resultingLocalVector.z)) * maxVectorLength;
                bestEnemyLocalDirection.Value = float2LocalVector;

                neighbourEntities.Dispose();
            }).WithReadOnly(localtoworldData).ScheduleParallel();
        }
    }
}