using UnityEngine;

namespace Game {
    public class Grid {
        public int Width => width;
        public int Height => height;
        public float CellSize => cellSize;
        
        private int width;
        private int height;
        private float cellSize;
        private Transform parent;
        private int[,] cells;

        public Grid(int width, int height, float cellSize, Transform parent) {
            this.width = width;
            this.height = height;
            this.cellSize = cellSize;
            this.parent = parent;

            cells = new int[width, height];
        }

        public Vector3 GridCellToWorld(Vector3 cell) {
            return parent.TransformPoint(cell * cellSize);
        }

        public Vector3 WorldToGridCell(Vector3 world) {
            return parent.InverseTransformPoint(world) / cellSize;
        }
    }
}