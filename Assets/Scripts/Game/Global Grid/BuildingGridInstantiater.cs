using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Game {
    public static class BuildingGridInstantiater {
        private static List<Vector2Int> occupiedTilesBuffer = new List<Vector2Int>();

        public static bool InstantiateOnGrid(Vector3 inWorldPos, Entity entityIn, EntityManager manager, out Entity entityOut) {
            Vector2Int spawnTile = BuildingGrid.WorldToGridFloored(inWorldPos);
            if (BuildingGrid.TileIsOccupied(spawnTile)) {
                entityOut = default;
                return false;
            }
            entityOut = InstantiateEcs(BuildingGrid.GridToWorldCentered(spawnTile), entityIn, manager);
            occupiedTilesBuffer.Clear();
            //occupiedTilesBuffer.AddRange(building.positionsInGrid);
            // if (!isPlaceable()) {
            //     Object.Destroy(building.gameObject);
            //     return false;
            // }
            foreach (Vector2Int occupiedTile in occupiedTilesBuffer) {
                //tiles[occupiedTile] = new GridTile(building);
            }
            return true;
        }
        
        private static Entity InstantiateEcs(Vector3 inWorldPos, Entity entity, EntityManager manager) {
            Entity building = manager.Instantiate(entity);
            manager.SetComponentData(building, new Translation {Value = inWorldPos});
            return building;
        }

        private static bool isPlaceable() {
            foreach (Vector2Int occupiedTile in occupiedTilesBuffer) {
                if (BuildingGrid.TileOutOfGrid(occupiedTile)) {
                    Debug.LogWarning("Can't instantiate: index was out of range");
                    return false;
                }
                if (BuildingGrid.TileIsOccupied(occupiedTile)) {
                    Debug.Log("Can't Instantiate: tile is occupied " + occupiedTile);
                    return false;
                }
            }
            return true;
        }
    }
}