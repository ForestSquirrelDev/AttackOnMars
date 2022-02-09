using UnityEngine;
using UnityEngine.Serialization;
using Utils;

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
        [FormerlySerializedAs("prefab")] [SerializeField] private GameObject debugPrefab;
        [SerializeField] private LayerMask mask;
        [SerializeField] private GameObject ghostPrefab;
        
        private BuildingGhost buildingGhost;
        private int clicksCount;

        private void Awake() {
           InitGrid();
        }

        private void Update() {
            if (Input.GetMouseButtonDown(0)) {
                return;
                switch (clicksCount) {
                case 0:
                    CreateBuildingGhost();
                    break;
                case 1: 
                    CreateBuilding();
                    break;
                }
            }
            buildingGhost?.Update();
        }
        
        private void CreateBuildingGhost() {
            buildingGhost = new BuildingGhost(ghostPrefab, Camera.main, mask);
            buildingGhost.Start();
            clicksCount++;
        }
        
        private void CreateBuilding() {
            Vector3 mouseToWorld = InputUtility.MouseToWorld(Camera.main, mask);
            if (!BuildingGrid.InstantiateOnGrid(mouseToWorld, debugPrefab, Quaternion.identity, transform)) return;
            buildingGhost.Dispose();
            buildingGhost = null;
            clicksCount = 0;
        }

        public void InitGrid() {
            BuildingGrid.Init(Width, Height, CellSize, transform, Terrain);
        }
    }
}