using Unity.Entities;
using Unity.Mathematics;

namespace Game.Ecs.Components.Pathfinding {
    public struct EmptyCellsGenerationRequest : IComponentData {
        public bool IsProcessing;
        public float CellSize;
        public float3 Origin;
        public int2 GridSize;

        public EmptyCellsGenerationRequest(float cellSize, float3 origin, int2 gridSize) {
            CellSize = cellSize;
            Origin = origin;
            GridSize = gridSize;
            IsProcessing = false;
        }
    }
}