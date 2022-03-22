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
    public class MonoGridManager : MonoBehaviour {
        public int Width => _width;
        public int Height => _height;
        public float CellSize => _cellSize;
        public Terrain Terrain => _terrain;
        public BuildingGrid EditorGrid;

        private Stopwatch _stopwatch = new Stopwatch();

        [FormerlySerializedAs("width")] [SerializeField] private int _width = 20;
        [FormerlySerializedAs("height")] [SerializeField] private int _height = 20;
        [FormerlySerializedAs("cellSize")] [SerializeField] private float _cellSize = 3;
        [FormerlySerializedAs("terrain")] [SerializeField] private Terrain _terrain;

        private void Awake() { 
            Bounds bounds = _terrain.terrainData.bounds;
            Rect terrainPerimeter = new Rect {
                xMin = bounds.min.x,
                yMin = bounds.min.z,
                xMax = bounds.max.x,
                yMax = bounds.max.z
            };
            int totalCellsCount = Mathf.CeilToInt(terrainPerimeter.size.x * terrainPerimeter.size.y / _cellSize);
            InitEditorGrid();
            var keeper = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<GridKeeperSystem>();
            keeper.Init(transform, _width, _height, _cellSize, totalCellsCount);
            SetGridSampledHeights(ref keeper.BuildingGrid, terrainPerimeter);
        }

        public void InitEditorGrid() {
            EditorGrid.EditorInit(Width, Height, CellSize, transform.localToWorldMatrix, transform.worldToLocalMatrix);
        }

        private void SetGridSampledHeights(ref BuildingGrid grid, Rect rect) {
            for (int x = 0; x < rect.width; x += (int)_cellSize) {
                for (int y = 0; y < rect.height; y += (int)_cellSize) {
                    float3 world = grid.WorldToGridCentered(new Vector3(x, 0, y), false);
                    grid.SetSampledHeight(world, _terrain.SampleHeight(world));
                }
            }
        }
    }
}