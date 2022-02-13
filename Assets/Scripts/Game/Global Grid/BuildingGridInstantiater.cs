using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Game {
    public static class BuildingGridInstantiater {
        public static bool InstantiateOnGrid(Vector3 inWorldPos, Entity entityIn, EntityManager manager, out Entity entityOut) {
            if (entityIn == Entity.Null) {
                Debug.LogError("Can't instantiate as the entity is null");
                entityOut = Entity.Null;
                return false;
            }
            Vector2Int spawnTile = BuildingGrid.WorldToGridFloored(inWorldPos);
            entityOut = InstantiateEcs(BuildingGrid.GridToWorldCentered(spawnTile), entityIn, manager);
            return true;
        }
        
        private static Entity InstantiateEcs(Vector3 inWorldPos, Entity entity, EntityManager manager) {
            Entity building = manager.Instantiate(entity);
            manager.SetComponentData(building, new Translation {Value = inWorldPos});
            return building;
        }
    }
}