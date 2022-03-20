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
    public partial class BuildingGhostPositioningQuadSystem : SystemBase {
        private GridKeeperSystem _gridKeeper;
        private EntityQueryDesc _quadsQueryDescription;
        private EndInitializationEntityCommandBufferSystem _commandBufferSystem;

        protected override void OnCreate() {
            _gridKeeper = World.GetOrCreateSystem<GridKeeperSystem>();
            _commandBufferSystem = World.GetOrCreateSystem<EndInitializationEntityCommandBufferSystem>();
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
                    (localToWorld[1], localToWorld[2]) = (localToWorld[2], localToWorld[1]);
                    int sortKey = firstEntityIndex + i + chunkIndex;
                    
                    float4 xzMinWorld = math.mul(localToWorld, xzMin);
                    float4 xzMaxWorld = math.mul(localToWorld, xzMax);

                    Rect rect = new Rect {
                        xMin = xzMinWorld.x,
                        yMin = xzMinWorld.z,
                        xMax = xzMaxWorld.x,
                        yMax = xzMaxWorld.z
                    };

                    if (BuildingGrid.IntersectsWithOccupiedTiles(rect)) {
                        Ecb.RemoveComponent<Tag_AvailableForPlacementGhostQuad>(sortKey, entity);
                    } 
                    else {
                        Ecb.AddComponent<Tag_AvailableForPlacementGhostQuad>(sortKey, entity);
                    }
                }
            }
        }
    }
}