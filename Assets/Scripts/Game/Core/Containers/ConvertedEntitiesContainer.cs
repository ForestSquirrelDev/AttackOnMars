using System.Collections.Generic;
using Game.Ecs.BlobAssets;
using static Game.Ecs.Monobehaviours.MonoBuildingsToEntitiesConverter;

namespace Game.Ecs.Containers {
    public static class ConvertedEntitiesContainer {
        public static Dictionary<BuildingType, ConvertedEntityPrefabData> entities 
            = new Dictionary<BuildingType, ConvertedEntityPrefabData>();
    }
}