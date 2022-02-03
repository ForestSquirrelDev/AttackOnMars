using System;
using UnityEngine;
using UnityEngine.Serialization;
using Utils;

namespace Game {
    public class GridManager : MonoBehaviour {
        [field: SerializeField]
        public int Test { get; private set; }

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
        
        
        private void Awake() {
           InitGrid();
        }

        private void Update() {
            if (Input.GetKeyDown(KeyCode.H)) {
                Vector3 mouseToWorld = InputUtility.MouseToWorld(Camera.main, mask);
                Grid.InstantiateOnGrid(mouseToWorld, debugPrefab, Quaternion.identity, transform);
            }
        }

        public void InitGrid() {
            Grid.Init(Width, Height, CellSize, transform, Terrain);
        }
    }
}