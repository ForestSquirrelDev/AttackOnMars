using UnityEngine;

namespace Game.Ecs.Systems {
    public abstract class SystemInitializerBase : MonoBehaviour {
        public abstract void OnAwake();
    }
}