using Unity.Entities;
using UnityEngine;

namespace Game.Ecs.Hybrid.Conversion {
    public abstract class GameObjectsConverterBase : MonoBehaviour {
        protected World World => World.DefaultGameObjectInjectionWorld;
        protected EntityManager EntityManager => World.EntityManager;
        public abstract void Convert();
    }
}
