using Unity.Entities;

namespace Game.Ecs.Flowfield.Systems {
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
    }
}