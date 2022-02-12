using Unity.Collections;
using Unity.Entities;
using UnityEngine;


namespace Game.Ecs.Components {
    [GenerateAuthoringComponent]
    public struct PositioningQuadComponent : IComponentData {
        public bool inited;
    }
}