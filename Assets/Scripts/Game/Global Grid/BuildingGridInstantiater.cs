using System.Collections.Generic;
using Game.Ecs.Utils;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Game {
    public static class BuildingGridInstantiater {
        private static List<Vector2Int> occupiedTilesBuffer = new List<Vector2Int>();

        public static bool InstantiateOnGrid(Vector3 inWorldPos, Entity entity, EntityManager manager) {
            Vector2Int spawnTile = BuildingGrid.WorldToGridFloored(inWorldPos);
            if (BuildingGrid.TileIsOccupied(spawnTile)) return false;
            InstantiateEcs(BuildingGrid.GridToWorldCentered(spawnTile), entity, manager);
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
        
        private static void InstantiateEcs(Vector3 inWorldPos, Entity entity, EntityManager manager) {
            Entity building = manager.Instantiate(entity);
            manager.SetComponentData(building, new Translation {Value = inWorldPos});
            InstantiatedBuildingsStorage.buildings.Add(building);
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

    public static class InstantiatedBuildingsStorage {
        public static List<Entity> buildings = new List<Entity>();
    }
}