/*using Game.AddressableConfigs;
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
using Utils;
using Utils.Maths;
using Utils.Pathfinding;

namespace Game.Ecs.Systems.Spawners {
    // complexity of this "algorithm" is complete ass. gotta come up with something better
    public partial class LocalAvoidanceSystem : SystemBase {
        private UnsafeList<FlowfieldCellComponent>.ParallelWriter _parentCellsWriter;
        private FlowfieldRuntimeData _flowfieldRuntimeData;
        private DependenciesScheduler _flowfieldDependenciesScheduler;
        private EnemyStatsConfig _enemiesConfig;

        protected override void OnCreate() {
            _enemiesConfig = AddressablesLoader.Get<EnemyStatsConfig>(AddressablesConsts.DefaultEnemyStatsConfig);
        }

        public void InjectFlowfieldDependencies(UnsafeList<FlowfieldCellComponent>.ParallelWriter parentCellsWriter, FlowfieldRuntimeData flowfieldData, DependenciesScheduler flowfieldDependenciesScheduler) {
            _flowfieldDependenciesScheduler = flowfieldDependenciesScheduler;
            _parentCellsWriter = parentCellsWriter;
            _flowfieldRuntimeData = flowfieldData;
        }
        
		protected override unsafe void OnUpdate() {
            return;
            var ltwData = GetComponentDataFromEntity<LocalToWorld>(true);
            var bestDirectionData = GetComponentDataFromEntity<BestEnemyCombinedDirectionComponent>(true);
            var writer = _parentCellsWriter;
            var flowfieldData = _flowfieldRuntimeData;
            var acceptableRange = 3;//_enemiesConfig.LocalAvoidanceAcceptableRangeSquared;
            var deps = _flowfieldDependenciesScheduler.GetCombinedReadWriteDependencies();
            
            Dependency = Entities.WithAll<Tag_Enemy>().ForEach((ref BestEnemyLocalAvoidanceDirection localDirection, in Entity testedEntity, in EnemyStateComponent state) => {
                if (!ltwData.TryGetComponent(testedEntity, out var testedLtw)) return;
                if (state.Value != EnemyState.Moving && state.Value != EnemyState.ReadyToAttack) return;

                // 1. find on which cell testedEntity is standing
                var cellIndex = FlowfieldUtility.CalculateIndexFromWorld(
                    testedLtw.Position, flowfieldData.ParentGridOrigin, flowfieldData.ParentGridSize, flowfieldData.ParentCellSize);
                if (FlowfieldUtility.IsOutOfRange(writer.ListData->Length, cellIndex)) return;
                var cell = writer.ListData->Ptr[cellIndex];
                if (cell.Entities.IsEmpty) return;
                var entitiesOnSameCell = cell.Entities;
                
                // 2. see if there are even entities nearby. if not, we can move by grid direction alone
                if (!HasNearbyEntities(entitiesOnSameCell, testedEntity, ltwData, testedLtw, acceptableRange, out var nearbyEntities)) {
                    localDirection.Value = float3.zero;
                    nearbyEntities.Dispose();
                    return;
                }
                
                // 3. see if our local direction is already valid (i.e. does not intersect with other entities move directions)
                if (HasValidLocalDirection(ltwData, bestDirectionData, acceptableRange, localDirection.Value, nearbyEntities, testedEntity)) {
                    nearbyEntities.Dispose();
                    return;
                }

                // 4. get 8 directions which we are going to be testing as suitable for local avoidance
                var directions = FlowfieldUtility.GetNeighbourOffsets();
                var bestLocalDirection = VectorUtility.InvalidFloat3();

                foreach (var direction in directions) {
                    // intersects with vector between entities? continue
                    var testedPosition = new float3(testedLtw.Position.x + direction.x  * acceptableRange, testedLtw.Position.y, testedLtw.Position.z + direction.y  * acceptableRange);
                    for (var i = 0; i < nearbyEntities.Length; i++) {
                        var sameCellEntity = nearbyEntities[i];
                        if (sameCellEntity == testedEntity) continue;
                        if (!bestDirectionData.TryGetComponent(sameCellEntity, out var bestNearbyEntityDirection)) continue;
                        var nearbyEntityLtw = ltwData[sameCellEntity];
                        if (math.distancesq(nearbyEntityLtw.Position, testedPosition) <= acceptableRange
                           || math.distancesq(new float3(nearbyEntityLtw.Position.x + bestNearbyEntityDirection.Value.x * acceptableRange,
                                nearbyEntityLtw.Position.y, nearbyEntityLtw.Position.z + bestNearbyEntityDirection.Value.y * acceptableRange), testedPosition) <= acceptableRange) continue;

                        bestLocalDirection = math.normalize(testedPosition - testedLtw.Position);
                        break;
                    }
                    if (bestLocalDirection.IsValidFloat3()) break;
                }
                
                if (bestLocalDirection.IsValidFloat3()) {
                    localDirection.Value = bestLocalDirection;
                }
                
                directions.Dispose();
                nearbyEntities.Dispose();
            }).WithReadOnly(ltwData).Schedule(JobHandle.CombineDependencies(deps, Dependency));
            _flowfieldDependenciesScheduler.AddExternalReadWriteDependency(Dependency);
        }
        
        private static bool HasNearbyEntities(UnsafeHashSet<Entity> entitiesOnSameCell, Entity e, ComponentDataFromEntity<LocalToWorld> ltwData, LocalToWorld ltw, float acceptableRange, 
            out NativeList<Entity> nearbyEntities) {
            nearbyEntities = new NativeList<Entity>(1, Allocator.Temp);

            foreach (var entity in entitiesOnSameCell) {
                if (entity == e) continue;
                if (!ltwData.TryGetComponent(entity, out var nearbyEntityLtw)) continue;
                var dist = math.distancesq(nearbyEntityLtw.Position, ltw.Position);
                if (dist <= acceptableRange) {
                    nearbyEntities.Add(entity);
                }
            }
            
            return nearbyEntities.Length > 0;
        }

        private static bool HasValidLocalDirection(ComponentDataFromEntity<LocalToWorld> ltwData, ComponentDataFromEntity<BestEnemyCombinedDirectionComponent>
        bestCombinedDirection, float acceptableRange, float3 currentLocalDirection, NativeList<Entity> entitiesOnSameCell, Entity testedEntity) {
            // here we should check if nearby entities direction intersects with our direction
            if (!ltwData.TryGetComponent(testedEntity, out var testedLtw)) return false;
            if (currentLocalDirection.x.Approximately(0) && currentLocalDirection.z.Approximately(0)) {
                return false;
            }
            var testedEntityPosition = testedLtw.Position;
            // y component needs to be zero, otherwise we have no way to check intersection between the two
            var testedEntityEndPoint = new float3(testedEntityPosition.x + currentLocalDirection.x * acceptableRange, 0, testedEntityPosition.z + currentLocalDirection.z * acceptableRange);
            foreach (var nearbyEntity in entitiesOnSameCell) {
                if (!ltwData.TryGetComponent(nearbyEntity, out var entityOnSameCellLtw)) continue;
                var nearbyEntityPosition = entityOnSameCellLtw.Position;
                var nearbyEntityDirection = bestCombinedDirection[nearbyEntity];
                var nearbyEntityEndPoint = new float3(nearbyEntityPosition.x + nearbyEntityDirection.Value.x * acceptableRange, 0, nearbyEntityPosition.z + nearbyEntityDirection.Value.z * acceptableRange);
                if (VectorUtility.LineLineIntersection(out var intersection, testedEntityPosition, 
                    testedEntityEndPoint, nearbyEntityPosition, nearbyEntityEndPoint)) {
                    //Debug.Log($"Intersects. Point: {intersection}");
                    return false;
                }
            }
            return true;
        }
    }
}*/