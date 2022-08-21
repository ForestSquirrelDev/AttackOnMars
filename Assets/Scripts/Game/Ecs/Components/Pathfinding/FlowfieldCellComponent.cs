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
        public int2 BestFlowfieldDirection;
        public bool IsBestCell;

        public FlowfieldNeighbours NeighboursIndexes { get; set; }
        public UnsafeList<FlowfieldCellComponent>.ParallelWriter ChildCells => _childCells.AsParallelWriter();
        public UnsafeHashSet<Entity> Entities { get; private set; }

        public bool IsCreated { get; set; }
        public bool Unwalkable => BaseCost.Approximately(float.MaxValue) || BestCost.Approximately(float.MaxValue);

        private UnsafeList<FlowfieldCellComponent> _childCells;

        public void InitChildCells(int childCellsCapacity, Allocator allocator) {
            _childCells = new UnsafeList<FlowfieldCellComponent>(childCellsCapacity, allocator);
        }

        public void InitEntities(int initialCapacity, Allocator allocator) {
            Entities = new UnsafeHashSet<Entity>(initialCapacity, allocator);
        }

        public void ClearEntities() {
            if (Entities.IsCreated) {
                Entities.Dispose();
            }
        }

        public void ClearChildCells() {
            if (!_childCells.IsCreated) return;
            foreach (var cell in _childCells) {
                cell.Dispose();
            }
            _childCells.Dispose();
        }
        
        public void Dispose() {
            if (!IsCreated) return;
            ClearChildCells();
            ClearEntities();
            IsCreated = false;
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

    public struct FlowfieldNeighbours {
        public const int Count = 8;

        public int Length => Count;
        public int Item1;
        public int Item2;
        public int Item3;
        public int Item4;
        public int Item5;
        public int Item6;
        public int Item7;
        public int Item8;
        
        public int this[int index] {
            get {
                return index switch {
                    0 => Item1,
                    1 => Item2,
                    2 => Item3,
                    3 => Item4,
                    4 => Item5,
                    5 => Item6,
                    6 => Item7,
                    7 => Item8,
                    _ => throw new ArgumentException("Unexpected neighbour index")
                };
            }
        }
        
        public FlowfieldNeighbours(int item1, int item2, int item3, int item4, int item5, int item6, int item7, int item8) {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
            Item4 = item4;
            Item5 = item5;
            Item6 = item6;
            Item7 = item7;
            Item8 = item8;
        }
    }
}