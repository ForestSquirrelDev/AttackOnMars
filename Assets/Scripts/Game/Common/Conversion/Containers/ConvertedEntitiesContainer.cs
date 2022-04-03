using System.Collections.Generic;
using Shared;
using static Game.Ecs.Monobehaviours.MonoBuildingsToEntitiesConverter;

namespace Game.Ecs.Containers {
    public static class ConvertedEntitiesContainer {
        public static readonly Dictionary<BuildingType, ConvertedEntityPrefabData> s_Entities 
            = new Dictionary<BuildingType, ConvertedEntityPrefabData>();
    }
}
