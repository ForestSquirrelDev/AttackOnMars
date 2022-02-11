using Unity.Entities;


namespace Game.Ecs.Components {
    [GenerateAuthoringComponent]
    public struct PositioningQuadComponent : IComponentData {
        public bool inited;
    }
}