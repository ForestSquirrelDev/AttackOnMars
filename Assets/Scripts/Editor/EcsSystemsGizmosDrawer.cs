using System;
using System.Collections.Generic;
using Game.Ecs.Systems;
using Unity.Entities;
using UnityEngine;

namespace Editor {
    public class EcsSystemsGizmosDrawer : MonoBehaviour {
        private PositioningQuadSystem system;

        private void Start() {
            system = World.DefaultGameObjectInjectionWorld.GetExistingSystem<PositioningQuadSystem>();
        }

        private void OnDrawGizmos() {
            system?.OnDrawGizmos();
        }
    }
}