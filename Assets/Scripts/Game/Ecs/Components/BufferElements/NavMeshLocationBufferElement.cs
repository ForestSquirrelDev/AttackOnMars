using Unity.Entities;
using UnityEngine.Experimental.AI;

namespace Game.Ecs.Components.BufferElements {
    public struct NavMeshLocationBufferElement : IBufferElementData {
        public NavMeshLocation Value;
    }
}