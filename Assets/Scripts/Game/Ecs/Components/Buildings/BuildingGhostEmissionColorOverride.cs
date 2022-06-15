using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;

namespace Game.Ecs.Components {
    [GenerateAuthoringComponent]
    [MaterialProperty("_Emission_Color", MaterialPropertyFormat.Float4)]
    public struct BuildingGhostEmissionColorOverride : IComponentData {
        public float4 Value;
    }
}