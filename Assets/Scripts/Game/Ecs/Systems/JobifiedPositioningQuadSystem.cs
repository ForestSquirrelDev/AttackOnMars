using Game.Ecs.Components;
using Game.Ecs.Components.BufferElements;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Utils;

namespace Game.Ecs.Systems {
    public partial class JobifiedPositioningQuadSystem : SystemBase {
        private EntityQueryDesc _quadsQueryDescription;
        private EndSimulationEntityCommandBufferSystem _commandBufferSystem;
        private GridKeeperSystem _gridKeeperSystem;

        protected override void OnCreate() {
            _gridKeeperSystem = World.GetOrCreateSystem<GridKeeperSystem>();
            _quadsQueryDescription = new EntityQueryDesc {
                All = new ComponentType[] { typeof(PositioningQuadComponent), typeof(Tag_BuildingPositioningQuad), typeof(LocalToWorld)},
                None = new ComponentType[] {typeof(Tag_ReadyForGridQuad)}
            };
            _commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }
        
        protected override void OnUpdate() {
            UpdatePositionsJob job = new UpdatePositionsJob {
                LocalToWorldHandle = GetComponentTypeHandle<LocalToWorld>(true),
                Ecb = _commandBufferSystem.CreateCommandBuffer().AsParallelWriter(),
                EntityTypeHandle = GetEntityTypeHandle(),
                BuildingGrid = _gridKeeperSystem.buildingGrid
            };
            
            JobHandle handle = job.Schedule(EntityManager.CreateEntityQuery(_quadsQueryDescription), Dependency);
            _commandBufferSystem.AddJobHandleForProducer(handle);
            
            Dependency = handle;
        }

        // why put this small one-time 0.02ms operation into a job? for learning purposes, of course!
        [BurstCompile(CompileSynchronously = true)]
        private struct UpdatePositionsJob : IJobChunk {
            [ReadOnly] public ComponentTypeHandle<LocalToWorld> LocalToWorldHandle;
            [ReadOnly] public EntityTypeHandle EntityTypeHandle;
            [ReadOnly] public BuildingGrid BuildingGrid;
            public EntityCommandBuffer.ParallelWriter Ecb;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
                NativeArray<Entity> entities = chunk.GetNativeArray(EntityTypeHandle);
                NativeArray<LocalToWorld> localToWorlds = chunk.GetNativeArray(LocalToWorldHandle);
                for (int i = 0; i < entities.Length; i++) {
                    Entity entity = entities[i];
                    int sortKey = firstEntityIndex + i + chunkIndex;
                    
                    //
                    Ecb.AddComponent<Tag_ReadyForGridQuad>(sortKey, entity);
                    Ecb.AddBuffer<Int2BufferElement>(sortKey, entity);
                    
                    //
                    LocalToWorld localToWorld = localToWorlds[i];
                    float4x4 transformCenter = localToWorld.Value;
                    (transformCenter[1], transformCenter[2]) = (transformCenter[2], transformCenter[1]);
                    
                    float4x4 gridOrigin = new float4x4 {
                        [0] = math.normalize(transformCenter[0]),
                        [1] = math.normalize(transformCenter[1]),
                        [2] = math.normalize(transformCenter[2]),
                        [3] = math.mul(transformCenter, new float4(-0.5f, 0f, -0.5f, 1f))
                    };
                    
                    //
                    int2 size = CalculateGridSize(transformCenter);
                    PositioningGrid positioningGrid = new PositioningGrid();
                    positioningGrid.FillGrid(size.x, size.y);

                    for (int tile = 0; tile < positioningGrid.positions.Length; tile++) {
                        int2 unitGridTile = positioningGrid.positions[tile];
                        float4 world = math.mul(gridOrigin, new float4(unitGridTile.x * BuildingGrid.CellSize, 0, unitGridTile.y * BuildingGrid.CellSize, 1));
                        Vector2Int buildingGridTile = BuildingGrid.WorldToGridFloored(world.xyz);
                        Ecb.AppendToBuffer(sortKey, entity, new Int2BufferElement{value = new int2(buildingGridTile.x, buildingGridTile.y)});
                    }

                    //
                    positioningGrid.Dispose();
                }
                entities.Dispose();
                localToWorlds.Dispose();
            }
        
            private int2 CalculateGridSize(float4x4 transformCenter) {
                float4 leftBottomCorner = math.mul(transformCenter, new float4(-0.5f, 0f, -0.5f, 1));
                float4 leftTopCorner = math.mul(transformCenter, new float4(-0.5f, 0, 0.5f, 1));
                float4 rightBottomCorner = math.mul(transformCenter, new float4(0.5f, 0f, -0.5f, 1));

                int2 leftBottomToGlobalGrid = BuildingGrid.WorldToGridCeiled(leftBottomCorner.xyz).ToInt2();
                int2 leftTopToGlobalGrid = BuildingGrid.WorldToGridCeiled(leftTopCorner.xyz).ToInt2();
                int2 rightBottomToGlobalGrid = BuildingGrid.WorldToGridCeiled(rightBottomCorner.xyz).ToInt2();

                int width = math.clamp(math.abs(rightBottomToGlobalGrid.x - leftBottomToGlobalGrid.x) + 1, 1, int.MaxValue);
                int height = math.clamp(math.abs(leftTopToGlobalGrid.y - leftBottomToGlobalGrid.y) + 1, 1, int.MaxValue);
        
                return new int2(width, height);
            }
        }
    }
}