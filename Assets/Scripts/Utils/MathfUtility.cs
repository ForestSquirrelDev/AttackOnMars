using UnityEngine;

namespace Utils.Maths
{
    public static class MathfUtility {
        private static float s_halfMaxValueBacking = 0;
        public static float HalfMaxValue {
            get {
                if (s_halfMaxValueBacking == 0)
                    s_halfMaxValueBacking = float.MaxValue / 2;
                return s_halfMaxValueBacking;
            }
        }

        public static float InverseRelationship(float constant, float mutable) {
            return constant / mutable;
        }

        public static float Frac(this float f) {
            return f - Mathf.FloorToInt(f);
        }

        public static float ClampNeg1To1(this float f) {
            return Mathf.Clamp(f, -1f, 1f);
        }

        public static float ClampPos1ToMaxValue(this float f) {
            return Mathf.Clamp(f, 1f, float.MaxValue);
        }
        
        public static float GrowExponential(this float value, float square = 3) {
            return 2 * Mathf.Pow(value, square);
        }
    }
}
