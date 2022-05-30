using UnityEngine;

namespace Utils {
    public static class TerrainUtility {
        public static Rect GetWorldRect(float terrainWorldPosX, float terrainWorldPosZ, float terrainSizeX, float terrainSizeZ) {
            return new Rect(terrainWorldPosX, terrainWorldPosZ, terrainSizeX, terrainSizeZ);
        }
    }
}