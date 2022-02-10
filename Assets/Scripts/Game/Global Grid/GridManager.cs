using UnityEngine;

namespace Game {
    public class GridManager : MonoBehaviour {
        public int Width => width;
        public int Height => height;
        public float CellSize => cellSize;
        public Terrain Terrain => terrain;

        [SerializeField] private int width = 20;
        [SerializeField] private int height = 20;
        [SerializeField] private float cellSize = 3;
        [SerializeField] private Terrain terrain;

        private void Awake() {
           InitGrid();
        }

        public void InitGrid() {
            BuildingGrid.Init(Width, Height, CellSize, transform, Terrain);
        }
    }
}