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
        private float4x4 _transformCenter;
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
                transformCenter = _transformCenter,
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
            public float4x4 transformCenter;
            [ReadOnly] public ComponentTypeHandle<LocalToWorld> localToWorldHandle;
            [ReadOnly] public ComponentTypeHandle<PositioningQuadComponent> positioningQuadHandle;
            [ReadOnly] public EntityTypeHandle entityTypeHandle;
            [ReadOnly] public BuildingGrid buildingGrid;
            public EntityCommandBuffer.ParallelWriter ecb;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
                NativeArray<Entity> entities = chunk.GetNativeArray(entityTypeHandle);
                for (int i = 0; i < entities.Length; i++) {
                    ecb.AddComponent<Tag_ReadyForGridQuad>(firstEntityIndex + i, entities[i]);
                    ecb.AddBuffer<Int2BufferElement>(firstEntityIndex + i, entities[i]);
                    ecb.SetBuffer<Int2BufferElement>(firstEntityIndex + 1, entities[i]);
                    ecb.AppendToBuffer(firstEntityIndex + i, entities[i], new Int2BufferElement {value = CalculateGridSize()});
                }
            }
        
            private int2 CalculateGridSize() {
                float4 leftBottomCorner = new float4(-0.5f, 0f, -0.5f, 0);
                float4 leftTopCorner = new float4(-0.5f, 0, 0.5f, 0);
                float4 rightBottomCorner = new float4(0.5f, 0f, -0.5f, 0);
        
                leftBottomCorner = transformCenter.MultiplyPoint(leftBottomCorner);
                leftTopCorner = transformCenter.MultiplyPoint(leftTopCorner);
                rightBottomCorner = transformCenter.MultiplyPoint(rightBottomCorner);
        
                int2 leftBottomToGlobalGrid = buildingGrid.WorldToGridCeiled(leftBottomCorner.ToFloat3()).ToInt2();
                int2 leftTopToGlobalGrid = buildingGrid.WorldToGridCeiled(leftTopCorner.ToFloat3()).ToInt2();
                int2 rightBottomToGlobalGrid = buildingGrid.WorldToGridCeiled(rightBottomCorner.ToFloat3()).ToInt2();
            
                int width = math.clamp(math.abs(rightBottomToGlobalGrid.x - leftBottomToGlobalGrid.x) + 1, 1, int.MaxValue);
                int height = math.clamp(math.abs(leftTopToGlobalGrid.y - leftBottomToGlobalGrid.y) + 1, 1, int.MaxValue);
        
                return new int2(width, height);
            }
        }
    }
}