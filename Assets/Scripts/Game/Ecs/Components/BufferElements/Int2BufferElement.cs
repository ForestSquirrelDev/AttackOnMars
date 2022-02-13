using Unity.Entities;
using Unity.Mathematics;

namespace Game.Ecs.Components.BufferElements {
    [InternalBufferCapacity(6)]
    public struct Int2BufferElement : IBufferElementData {
        public int2 value;
    }
}