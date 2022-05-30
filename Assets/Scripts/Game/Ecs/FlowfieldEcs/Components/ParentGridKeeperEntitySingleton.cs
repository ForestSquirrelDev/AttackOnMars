using Unity.Entities;

namespace Game.Ecs.Flowfield.Components {
    public struct ParentGridKeeperEntitySingleton : IComponentData {
        public Entity Entity;
    }
}