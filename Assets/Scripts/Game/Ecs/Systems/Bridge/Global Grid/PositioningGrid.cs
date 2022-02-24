using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Game {
    public struct PositioningGrid {
        private NativeList<int2> _positions;

        public PositioningGrid(int width, int height) {
            _positions = new NativeList<int2>(Allocator.Temp);
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    _positions.Add(new int2(x, y));
                }
            }
        }

        public void GetGrid(List<int2> filled) {
            if (filled.Count > 0) filled.Clear();
            foreach (var tile in _positions) {
                filled.Add(tile);
            }
        }

        public void Dispose() {
            _positions.Dispose();
        }
    }
}