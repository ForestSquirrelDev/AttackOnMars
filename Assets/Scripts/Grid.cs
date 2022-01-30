using UnityEngine;

namespace Game {
    public class Grid {
        private int width;
        private int height;
        private int[,] cells;

        public Grid(int width, int height) {
            this.width = width;
            this.height = height;

            cells = new int[width, height];
        }
    }
}