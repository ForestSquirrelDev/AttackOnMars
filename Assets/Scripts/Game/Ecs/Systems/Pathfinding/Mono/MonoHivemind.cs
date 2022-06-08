using Unity.Mathematics;
using UnityEngine;

namespace Game.Ecs.Systems.Pathfinding.Mono {
    public class MonoHivemind : MonoBehaviour {
        [SerializeField] private float3 _debugCurrentTarget;
        public static MonoHivemind Instance { get; private set; }
        public float3 CurrentTarget { get; private set; }

        private void Awake() {
            Instance = this;
            CurrentTarget = _debugCurrentTarget;
        }
    }
}