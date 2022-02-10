using System;
using System.Collections.Generic;
using Game.Ecs.Systems;
using Unity.Entities;
using UnityEngine;

namespace Editor {
    public class EcsSystemsGizmosDrawer : MonoBehaviour {
        private PositioningQuadSystem system;
        [SerializeField] private Mesh mesh;
        private List<Vector3> l = new List<Vector3>();
        private void Start() {
            system = World.DefaultGameObjectInjectionWorld.GetExistingSystem<PositioningQuadSystem>();
        }

        private void OnDrawGizmos() {
            system?.OnDrawGizmos();
            if (mesh != null) {
                mesh.GetVertices(l);
                foreach (var vertex in l) {
                    Debug.Log(vertex);
                }
            }
        }
    }
}