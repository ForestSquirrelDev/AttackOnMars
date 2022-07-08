using Game.Ecs.Components;
using Game.Ecs.Components.Tags;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Game.Ecs.Systems {
    public partial class ChangeBuildingGhostColorSystem : SystemBase {
        protected override void OnUpdate() {
            var colorOverrideData = GetComponentDataFromEntity<BuildingGhostEmissionColorOverride>();
            
            Entities.WithAll<BuildingGhostPositioningQuadComponent>().ForEach((in Parent parent, in BuildingGhostPositioningQuadComponent quadComponent) => {
                if (!colorOverrideData.HasComponent(parent.Value)) return;
                var color = quadComponent.AvailableForPlacement 
                    ? new float4(0.0f, 1f, 0.0f, 1f) 
                    : new float4(1f, 0.0f, 0.0f, 1f);
                colorOverrideData[parent.Value] = new BuildingGhostEmissionColorOverride { Value = color };
            }).Schedule();
        }
    }
}
