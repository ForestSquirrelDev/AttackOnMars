using System.Collections.Generic;

namespace Game.Ecs.Monobehaviours {
    /// <summary>
    /// Contains entities that are converted from prefabs on the initialization stage. Those entities are then used to spawn new ones on runtime.
    /// </summary>
    public static class ConvertedEntitiesContainer {
        public static Dictionary<BuildingType, MonoBuildingsToEntitiesConverter.ConvertedEntityPrefabData> entities 
            = new Dictionary<BuildingType, MonoBuildingsToEntitiesConverter.ConvertedEntityPrefabData>();
    }
}
