using Unity.Mathematics;
using UnityEngine;

namespace Utils.Maths {
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

        public static float3 Normalize(this float3 value) {
            var mag = value.Magnitude();
            return new float3(value.x / mag, value.y / mag, value.z / mag);
        }

        public static float Magnitude(this float3 value) {
            return Mathf.Sqrt(Mathf.Pow(value.x, 2) + Mathf.Pow(value.y, 2) + Mathf.Pow(value.z, 2));
        }

        public static bool Approximately(this float f, float f2) {
            return Mathf.Approximately(f, f2);
        }

        public static float TunableSigmoid(float k, float t) {
            return (t - t * k) / (k - math.abs(t) * 2.0f * k + 1.0f);
        }
        
        public static float ReverseTunableSigmoid(float k, float t) {
            return -(t - t * k) / (k - math.abs(t) * 2.0f * k + 1.0f) + 1;
        }

        public static bool IsZero(this float f) {
            return math.abs(f) <= float.Epsilon;
        }
    }
}
