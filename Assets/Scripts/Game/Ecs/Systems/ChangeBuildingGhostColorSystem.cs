using Game.Ecs.Components;
using Game.Ecs.Components.Tags;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Game.Ecs.Systems {
    public partial class ChangeBuildingGhostColorSystem : SystemBase {
        private EndSimulationEntityCommandBufferSystem _endSimulationEcb;

        protected override void OnCreate() {
            _endSimulationEcb = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate() {
            EntityCommandBuffer ecb = _endSimulationEcb.CreateCommandBuffer();
            Entities.WithAll<Tag_AvailableForPlacementGhostQuad>().ForEach((ref Parent parent) => {
                ecb.SetComponent(parent.Value, new BuildingGhostEmissionColorOverride { Value = new float4(0.0f, 1f, 0.0f, 1f) });
            }).Schedule();

            Entities.WithNone<Tag_AvailableForPlacementGhostQuad>().ForEach((Tag_BuildingGhostPositioningQuad ghostQuad, ref Parent parent) => {
                ecb.SetComponent(parent.Value, new BuildingGhostEmissionColorOverride { Value = new float4(1f, 0.0f, 0.0f, 1f) });
            }).Schedule();
        }
    }
}
