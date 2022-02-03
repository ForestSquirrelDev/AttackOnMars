using System.Collections;
using System.Collections.Generic;
using Game.Buildings;
using UnityEngine;

namespace Game {
    public class GridTile {
        public IBuilding building;

        public GridTile(IBuilding building) {
            this.building = building;
        }
    }
}