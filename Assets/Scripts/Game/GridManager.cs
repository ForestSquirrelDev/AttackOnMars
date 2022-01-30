using UnityEngine;
using Utils;

namespace Game {
    public class GridManager : MonoBehaviour {
        public int width = 20;
        public int height = 20;
        public float cellSize = 3;
        public Transform cube;
        public GameObject prefab;
        public LayerMask mask;
        
        private Grid grid;
        
        private void Awake() {
            grid = new Grid(width, height, cellSize, transform);
        }

        private void Update() {
            if (Input.GetKeyDown(KeyCode.H)) {
                Vector3 mouseToWorld = InputUtils.MouseToWorld(Camera.main, mask);
                grid.InstantiateOnGrid(mouseToWorld, prefab, Quaternion.identity, transform);
            }
        }
    }
}