using Unity.Physics;

namespace Utils {
    public static class CollisionLayers {
        public enum CollisionLayer {
            Terrain = 1 << 0,
            Enemies = 1 << 1,
            Buildings = 1 << 2
        }

        public static CollisionFilter Enemy() {
            return new CollisionFilter {
                BelongsTo = (uint)CollisionLayer.Enemies,
                CollidesWith = (uint)CollisionLayer.Buildings
            };
        }
    }
}