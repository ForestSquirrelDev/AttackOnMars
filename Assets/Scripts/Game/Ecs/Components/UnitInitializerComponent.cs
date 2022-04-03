using Unity.Entities;
using Unity.Mathematics;

namespace Game.Ecs.Components {
    [GenerateAuthoringComponent]
    public struct UnitInitializerComponent : IComponentData {
        public float3 CurrentPosition;
        public int DestinationDistanceAlongZAxis;
        public int2 SpeedRange;
        public float ReachedDistanceThreshold;
        public uint SpeedSeed;
    }
}