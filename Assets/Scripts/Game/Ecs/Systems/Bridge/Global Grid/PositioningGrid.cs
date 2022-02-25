using Unity.Collections;
using Unity.Mathematics;

namespace Game {
    public struct PositioningGrid {
        public NativeList<int2> positions;

        public void FillGrid(int width, int height) {
            positions = new NativeList<int2>(Allocator.Temp);
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    positions.Add(new int2(x, y));
                }
            }
        }

        public void Dispose() {
            positions.Dispose();
        }
    }
}