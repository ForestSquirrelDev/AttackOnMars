using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Utils.Maths;
using Utils.Pathfinding;

namespace Game.Ecs.Components.Pathfinding {
#pragma warning disable 660,661
    public struct FlowfieldCellComponent : IEquatable<FlowfieldCellComponent> {
#pragma warning restore 660,661
        public float3 WorldPosition;
        public float3 WorldCenter;
        public int2 GridPosition;
        public float Size;
        public FlowFieldRect WorldRect;
        public float BaseCost;
        public float BestCost;
        public int2 BestDirection;
        public bool IsBestCell;
        
        public UnsafeList<FlowfieldCellComponent>.ParallelWriter ChildCells => _childCells.AsParallelWriter();
        public UnsafeHashSet<Entity> Entities => _entities;
        public bool Unwalkable => BaseCost.Approximately(float.MaxValue) || BestCost.Approximately(float.MaxValue);
        
        private UnsafeList<FlowfieldCellComponent> _childCells;
        private UnsafeHashSet<Entity> _entities;

        public void Init(int childCellsCapacity, int entitiesCapacity, Allocator allocator) {
            _childCells = new UnsafeList<FlowfieldCellComponent>(childCellsCapacity, allocator);
            _entities = new UnsafeHashSet<Entity>(entitiesCapacity, allocator);
        }
        
        public void Dispose() {
            _childCells.Dispose();
            _entities.Dispose();
        }
        
        public bool Equals(FlowfieldCellComponent other) {
            CompareParameters(this, other, out var gridPosEquals, out var worldPosEquals, out var bestCostEquals);
            return gridPosEquals && worldPosEquals && bestCostEquals;
        }

        public static bool operator ==(FlowfieldCellComponent cell1, FlowfieldCellComponent cell2) {
            CompareParameters(cell1, cell2, out var gridPosEquals, out var worldPosEquals, out var bestCostEquals);
            return gridPosEquals && worldPosEquals && bestCostEquals;
        }

        public static bool operator !=(FlowfieldCellComponent cell1, FlowfieldCellComponent cell2) {
            CompareParameters(cell1, cell2, out var gridPosEquals, out var worldPosEquals, out var bestCostEquals);
            return !gridPosEquals || !worldPosEquals || !bestCostEquals;
        }

        public override string ToString() {
            return $"{GridPosition.ToString()}";
        }

        private static void CompareParameters(FlowfieldCellComponent cell1, FlowfieldCellComponent cell2, out bool gridPosEquals, out bool worldPosEquals, out bool bestCostEquals) {
            gridPosEquals = cell1.GridPosition.x == cell2.GridPosition.x && cell1.GridPosition.y == cell2.GridPosition.y;
            worldPosEquals = cell1.WorldPosition.x.Approximately(cell2.WorldPosition.x)
                             && cell1.WorldPosition.y.Approximately(cell2.WorldPosition.y)
                             && cell1.WorldPosition.z.Approximately(cell2.WorldPosition.z);
            bestCostEquals = cell1.BestCost.Approximately(cell2.BestCost);
        }

        public static FlowfieldCellComponent Null => new FlowfieldCellComponent();
    }
}