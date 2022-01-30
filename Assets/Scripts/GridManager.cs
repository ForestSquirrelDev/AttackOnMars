using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game {
    public class GridManager : MonoBehaviour {
        public int width = 20;
        public int height = 20;
        public float cellSize = 3;
        public Transform cube;
        
        private Grid grid;
        
        private void Awake() {
            grid = new Grid(width, height, cellSize, transform);
            Debug.Log(grid.WorldToGridCell(cube.position));
        }
    }
}