using UnityEngine;

namespace Game.AddressableConfigs {
    [CreateAssetMenu(menuName = "Game Configs/Flowfield Config")]
    public class FlowfieldConfig : ScriptableObject{
        public float ParentCellSize = 200f;
        public float ChildCellSize = 10f;
        
        [Tooltip("If world position height is higher than this value, when generating cost field it will be taken into account")]
        public float CostHeightThreshold = 21f;
        [Tooltip("When flowfield is generated, a raycast is thrown at left bottom corner of cell and at center of cell. If at either of these position angles between surface normal and Vector3.down is higher than " +
                 "this threshold, a cell is marked as unwalkable.")]
        public float UnwalkableAngleThreshold = 35f;

        public FlowFieldConfigValueType ToValueType() {
            return new FlowFieldConfigValueType {
                ParentCellSize = ParentCellSize,
                ChildCellSize = ChildCellSize,
                CostHeightThreshold = CostHeightThreshold,
                UnwalkableAngleThreshold = UnwalkableAngleThreshold
            };
        }
    }

    public struct FlowFieldConfigValueType {
        public float ParentCellSize;
        public float ChildCellSize;
        public float CostHeightThreshold;
        public float UnwalkableAngleThreshold;
    }
}