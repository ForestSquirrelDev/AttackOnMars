using Game.Ecs.Flowfield.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace Game.Ecs.Flowfield.Systems {
    // Flowfield step 1: load configs and create empty grid.
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial class InitializeFlowfieldSystem : SystemBase {
        private EndSimulationEntityCommandBufferSystem _commandBufferSystem;

        protected override void OnCreate() {
            _commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate() {
            // var ecb = _commandBufferSystem.CreateCommandBuffer().AsParallelWriter();
            // Dependency = Entities.WithAll<EmptyCellsGenerationRequest>().ForEach((int entityInQueryIndex, ref DynamicBuffer<FlowfieldCellBufferElementData> cellsBuffer, ref Entity entity, 
            // ref EmptyCellsGenerationRequest generationRequest) => {
            //     //if (generationRequest.IsProcessing) return;
            //     Debug.Log($"Initialize system running");
            //     generationRequest.IsProcessing = true;
            //     
            //     if (!HasComponent<BaseCostAndHeightsGenerationRequest>(entity)) 
            //         ecb.AddComponent<BaseCostAndHeightsGenerationRequest>(entityInQueryIndex, entity);
            //     ecb.SetComponent(entityInQueryIndex, entity, new BaseCostAndHeightsGenerationRequest {Entity = entity, IsProcessing = false});
            // }).ScheduleParallel(Dependency);
            // _commandBufferSystem.AddJobHandleForProducer(Dependency);
        }

        private struct KekwPelinNoiseJob : IJob {
            public NativeArray<FlowfieldCellComponent> Cells;
            public void Execute() {
                for (int i = 10000; i < Cells.Length; i++) {
                    var noise = Mathf.PerlinNoise(i, Mathf.Sqrt(i));
                }
                for (int i = 10000; i < Cells.Length; i++) {
                    var noise = Mathf.PerlinNoise(i, Mathf.Sqrt(i));
                }
                for (int i = 10000; i < Cells.Length; i++) {
                    var noise = Mathf.PerlinNoise(i, Mathf.Sqrt(i));
                }
            }
        }
    }
}