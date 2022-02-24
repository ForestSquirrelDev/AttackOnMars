using Game.Ecs.Components;
using Game.Ecs.Components.BufferElements;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Utils;

namespace Game.Ecs.Systems {
    public class JobifiedPositioningQuadSystem : SystemBase {
        private EntityQuery _quadsQuery;
        private EntityQueryDesc _quadsQueryDescription;
        private EndSimulationEntityCommandBufferSystem _commandBufferSystem;
        private GridKeeperSystem _gridKeeperSystem;
        private NativeArray<int2> int2s;

        protected override void OnCreate() {
            _gridKeeperSystem = World.GetOrCreateSystem<GridKeeperSystem>();
            _quadsQueryDescription = new EntityQueryDesc {
                All = new ComponentType[] { typeof(PositioningQuadComponent), typeof(Tag_BuildingPositioningQuad), typeof(LocalToWorld)},
                None = new ComponentType[] {typeof(Tag_ReadyForGridQuad)}
            };
            _commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }
        
        protected override void OnUpdate() {
            var localToWorldType = GetComponentTypeHandle<LocalToWorld>(true);
            var positioningQuadType = GetComponentTypeHandle<PositioningQuadComponent>();
            var entityTypeHandle = GetEntityTypeHandle();
            var ecb = _commandBufferSystem.CreateCommandBuffer().AsParallelWriter();
            _quadsQuery = EntityManager.CreateEntityQuery(_quadsQueryDescription);
            
            UpdatePositionsJob job = new UpdatePositionsJob {
                localToWorldHandle = localToWorldType,
                positioningQuadHandle = positioningQuadType,
                ecb = ecb,
                entityTypeHandle = entityTypeHandle,
                buildingGrid = _gridKeeperSystem.buildingGrid,
            };
            
            JobHandle handle = job.ScheduleParallel(_quadsQuery, Dependency);
            _commandBufferSystem.AddJobHandleForProducer(handle);
            
            Dependency = handle;
        }
        
        [BurstCompile]
        private struct UpdatePositionsJob : IJobChunk {
            [ReadOnly] public ComponentTypeHandle<LocalToWorld> localToWorldHandle;
            [ReadOnly] public ComponentTypeHandle<PositioningQuadComponent> positioningQuadHandle;
            [ReadOnly] public EntityTypeHandle entityTypeHandle;
            [ReadOnly] public BuildingGrid buildingGrid;
            public EntityCommandBuffer.ParallelWriter ecb;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
                NativeArray<Entity> entities = chunk.GetNativeArray(entityTypeHandle);
                NativeArray<LocalToWorld> localToWorlds = chunk.GetNativeArray(localToWorldHandle);
                for (int i = 0; i < entities.Length; i++) {
                    Entity entity = entities[i];
                    int sortKey = firstEntityIndex + i;
                    
                    //
                    LocalToWorld localToWorld = localToWorlds[i];
                    float3x4 transformCenter = new float3x4 {
                        [0] = localToWorld.Right,
                        [1] = localToWorld.Forward,
                        [2] = localToWorld.Up,
                        [3] = localToWorld.Position
                    };

                    float3x4 gridOrigin = new float3x4 {
                        [0] = math.normalize(localToWorld.Right),
                        [1] = math.normalize(localToWorld.Forward),
                        [2] = math.normalize(localToWorld.Up),
                        [3] = math.mul(transformCenter, new float4(-0.5f, 0f, -0.5f, 0f))
                    };

                    //
                    int2 size = CalculateGridSize(transformCenter);
                    PositioningGrid positioningGrid = new PositioningGrid();
                    positioningGrid.FillGrid(size.x, size.y);
                    
                    //
                    ecb.AddComponent<Tag_ReadyForGridQuad>(sortKey, entity);
                    ecb.AddBuffer<Int2BufferElement>(sortKey, entity);
                    ecb.AppendToBuffer(sortKey, entity, new Int2BufferElement());
                }
            }
        
            private int2 CalculateGridSize(float3x4 transformCenter) {
                float3 leftBottomCorner = math.mul(transformCenter, new float4(-0.5f, 0f, -0.5f, 0));
                float3 leftTopCorner = transformCenter.MultiplyPoint3x4(new float4(-0.5f, 0, 0.5f, 0));
                float3 rightBottomCorner = transformCenter.MultiplyPoint3x4(new float4(0.5f, 0f, -0.5f, 0));
        
                int2 leftBottomToGlobalGrid = buildingGrid.WorldToGridCeiled(leftBottomCorner).ToInt2();
                int2 leftTopToGlobalGrid = buildingGrid.WorldToGridCeiled(leftTopCorner).ToInt2();
                int2 rightBottomToGlobalGrid = buildingGrid.WorldToGridCeiled(rightBottomCorner).ToInt2();
            
                int width = math.clamp(math.abs(rightBottomToGlobalGrid.x - leftBottomToGlobalGrid.x) + 1, 1, int.MaxValue);
                int height = math.clamp(math.abs(leftTopToGlobalGrid.y - leftBottomToGlobalGrid.y) + 1, 1, int.MaxValue);
        
                return new int2(width, height);
            }
        }
    }
}