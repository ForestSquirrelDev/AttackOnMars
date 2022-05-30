using Unity.Entities;

namespace Game.Ecs.Flowfield.Components {
    public struct EntityBufferElementData : IBufferElementData {
        public Entity Value;
    }
}