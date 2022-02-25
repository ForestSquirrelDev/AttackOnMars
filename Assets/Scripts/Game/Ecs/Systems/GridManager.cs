using System.Diagnostics;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game.Ecs.Systems.Bridge.GlobalGrid {
    /// <summary>
    /// This script is in Systems assembly because it needs to be refactored. GridManager should not reference and init a system directly. Instead i should try and use some other
    /// form of passing initialization data, perhaps Addresables system or scriptable objects. This script should probably be in Core assembly.
    /// todo: untie GridManager.cs from GridKeeperSystem.cs
    /// </summary>
    public class GridManager : MonoBehaviour {
        public int Width => _width;
        public int Height => _height;
        public float CellSize => _cellSize;
        public Terrain Terrain => _terrain;
        public int TotalCellsCount { get; private set; }
        public BuildingGrid editorGrid;

        private Stopwatch _stopwatch = new Stopwatch();

        [FormerlySerializedAs("width")] [SerializeField] private int _width = 20;
        [FormerlySerializedAs("height")] [SerializeField] private int _height = 20;
        [FormerlySerializedAs("cellSize")] [SerializeField] private float _cellSize = 3;
        [FormerlySerializedAs("terrain")] [SerializeField] private Terrain _terrain;

        private void Awake() { 
            InitGrid();
            var keeper = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<GridKeeperSystem>();
            keeper.Init(this);
            SetGridSampledHeights(ref keeper.buildingGrid);
        }

        public void InitGrid() {
            editorGrid.EditorInit(Width, Height, CellSize, transform.localToWorldMatrix, transform.worldToLocalMatrix);
        }

        private void SetGridSampledHeights(ref BuildingGrid grid) {
            Bounds bounds = _terrain.terrainData.bounds;
            Rect rect = new Rect {
                xMin = bounds.min.x,
                yMin = bounds.min.z,
                xMax = bounds.max.x,
                yMax = bounds.max.z
            };
            TotalCellsCount = Mathf.CeilToInt(rect.size.x * rect.size.y / _cellSize);
            for (int x = 0; x < rect.width; x += (int)_cellSize) {
                for (int y = 0; y < rect.height; y += (int)_cellSize) {
                    float3 world = grid.WorldToGridCentered(new Vector3(x, 0, y), false);
                    grid.SetSampledHeight(world, _terrain.SampleHeight(world));
                }
            }
        }
    }
}