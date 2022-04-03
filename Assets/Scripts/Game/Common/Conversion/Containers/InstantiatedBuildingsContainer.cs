using System.Collections.Generic;
using Unity.Entities;

namespace Game {
    public static class InstantiatedBuildingsContainer {
        public static List<SpawnedBuilding> Buildings = new List<SpawnedBuilding>();
        
        public struct SpawnedBuilding {
            public Entity buildingRoot;
            public Entity positioningQuad;
        }
    }
}
