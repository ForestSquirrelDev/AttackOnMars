using Unity.Entities;
using Unity.Mathematics;
using UnityEngine.Experimental.AI;

namespace Game.Ecs.Components {
    [GenerateAuthoringComponent]
    public struct UnitComponent : IComponentData {
        public float3 ToPosition;
        public float3 FromPosition;
        public NavMeshLocation ToLocation;
        public NavMeshLocation FromLocation;
    
        public bool Routed;
        public bool ReachedDestination;
        public bool UsingCachedPath;

        public float3 WaypointDirection;
        public float Speed;
        public float ReachedDistanceThreshold;
        public int CurrentBufferIndex;
    }
}