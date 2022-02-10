using System.Collections.Generic;
using UnityEngine;

namespace Game {
    public struct PositioningGrid {
        private List<Vector2Int> positions;

        public PositioningGrid(int width, int height) {
            positions = new List<Vector2Int>();
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    positions.Add(new Vector2Int(x, y));
                }
            }
        }

        public void GetGrid(List<Vector2Int> filled) {
            if (filled.Count > 0) filled.Clear();
            foreach (var tile in positions) {
                filled.Add(tile);
            }
        }
    }
}