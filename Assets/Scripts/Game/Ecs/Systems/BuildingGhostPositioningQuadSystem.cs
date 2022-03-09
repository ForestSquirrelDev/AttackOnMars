using Game.Ecs.Components;
using Game.Ecs.Components.Tags;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Game.Ecs.Systems {
    public class BuildingGhostPositioningQuadSystem : SystemBase {
        private GridKeeperSystem _gridKeeper;
        private EntityQueryDesc _quadsQueryDescription;
        private EndSimulationEntityCommandBufferSystem _commandBufferSystem;

        protected override void OnCreate() {
            _gridKeeper = World.GetOrCreateSystem<GridKeeperSystem>();
            _commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            _quadsQueryDescription = new EntityQueryDesc {
                All = new ComponentType[] { typeof(Tag_BuildingGhostPositioningQuad), typeof(LocalToWorld)}
            };
        }

        protected override void OnUpdate() {
            UpdatePositionsJob job = new UpdatePositionsJob {
                LocalToWorldHandle = GetComponentTypeHandle<LocalToWorld>(true),
                Ecb = _commandBufferSystem.CreateCommandBuffer().AsParallelWriter(),
                EntityTypeHandle = GetEntityTypeHandle(),
                BuildingGrid = _gridKeeper.buildingGrid
            };
            
            JobHandle handle = job.Schedule(EntityManager.CreateEntityQuery(_quadsQueryDescription), Dependency);
            _commandBufferSystem.AddJobHandleForProducer(handle);
            
            Dependency = handle;
        }

        [BurstCompile]
        private struct UpdatePositionsJob : IJobChunk {
            [ReadOnly] public BuildingGrid BuildingGrid;
            [ReadOnly] public ComponentTypeHandle<LocalToWorld> LocalToWorldHandle;
            [ReadOnly] public EntityTypeHandle EntityTypeHandle;
            public EntityCommandBuffer.ParallelWriter Ecb;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
                NativeArray<Entity> entities = chunk.GetNativeArray(EntityTypeHandle);
                NativeArray<LocalToWorld> localToWorlds = chunk.GetNativeArray(LocalToWorldHandle);
                float4 xzMin = new float4(-0.5f, 0f, -0.5f, 1);
                float4 xzMax = new float4(0.5f, 0f, 0.5f, 1);
                
                for (int i = 0; i < entities.Length; i++) {
                    Entity entity = entities[i];
                    float4x4 localToWorld = localToWorlds[i].Value;
                    int sortKey = firstEntityIndex + i + chunkIndex;
                    
                    float4 xzMinWorld = math.mul(localToWorld, xzMin);
                    float4 xzMaxWorld = math.mul(localToWorld, xzMax);

                    Rect rect = new Rect {
                        xMin = xzMinWorld.x,
                        xMax = xzMaxWorld.x,
                        yMin = xzMinWorld.z,
                        yMax = xzMaxWorld.z
                    };
                    Debug.Log($"world - xMin: {rect.xMin} zMin: {rect.yMin}, xMax: {rect.xMax}, zMax: {rect.yMax}");
                    if (BuildingGrid.IntersectsWithOccupiedTiles(rect)) {
                        Debug.Log("true");
                        Ecb.RemoveComponent<Tag_AvailableForPlacementGhostQuad>(sortKey, entity);
                    } 
                    else {
                        Debug.Log("false");
                        Ecb.AddComponent<Tag_AvailableForPlacementGhostQuad>(sortKey, entity);
                    }
                }
            }
        }
    }
}