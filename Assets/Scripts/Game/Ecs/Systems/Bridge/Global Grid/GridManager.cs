using System.Diagnostics;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
using Utils;
using static Utils.StopwatchExtensions.TimeUnit;
using Debug = UnityEngine.Debug;

namespace Game.Ecs.Systems.Bridge.GlobalGrid {
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
            SampleHeights(ref keeper.buildingGrid);
        }

        public void InitGrid() {
            editorGrid.Init(Width, Height, CellSize, transform.localToWorldMatrix, transform.worldToLocalMatrix, TotalCellsCount);
        }

        private void SampleHeights(ref BuildingGrid grid) {
            Bounds bounds = _terrain.terrainData.bounds;
            Debug.Log($"size: {bounds.size} min: {bounds.min} max : {bounds.max}");
            Rect rect = new Rect {
                xMin = bounds.min.x,
                yMin = bounds.min.z,
                xMax = bounds.max.x,
                yMax = bounds.max.z
            };
            TotalCellsCount = Mathf.CeilToInt(rect.size.x * rect.size.y / _cellSize);
            _stopwatch.Start();
            for (int x = 0; x < rect.width; x += (int)_cellSize) {
                for (int y = 0; y < rect.height; y += (int)_cellSize) {
                    float3 world = grid.WorldToGridCentered(new Vector3(x, 0, y), false);
                    grid.SetSampledHeight(world, _terrain.SampleHeight(world));
                }
            }
            _stopwatch.Stop();
            Debug.Log($"Seconds: {StopwatchExtensions.ToMetricTime(_stopwatch.ElapsedTicks, Seconds)}");
            Debug.Log($"Memory: {System.GC.GetTotalMemory(true) * 0.000001}");

            _stopwatch.Reset();
        }
    }
}