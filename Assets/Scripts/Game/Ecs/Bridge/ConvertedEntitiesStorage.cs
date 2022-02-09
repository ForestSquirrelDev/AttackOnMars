using System.Collections.Generic;

namespace Game.Ecs.Monobehaviours {
    public static class ConvertedEntitiesStorage {
        public static Dictionary<BuildingType, MonoBuildingsToEntitiesConverter.ConvertedEntityPrefabData> entities 
            = new Dictionary<BuildingType, MonoBuildingsToEntitiesConverter.ConvertedEntityPrefabData>();
    }
}
