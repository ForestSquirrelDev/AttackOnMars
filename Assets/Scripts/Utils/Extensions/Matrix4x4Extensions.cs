using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Utils {
    public static class Matrix4x4Extensions {
        public static Matrix4x4 AxesWiseMatrix(Vector4 right, Vector4 up, Vector4 forward, Vector4 position) {
            Matrix4x4 m = new Matrix4x4();
            m.SetColumn(0, right);
            m.SetColumn(1, up);
            m.SetColumn(2, forward);
            m.SetColumn(3, position);
            return m;
        }
        
        public static void AxesWiseMatrix(ref Matrix4x4 m, float3 right, float3 up, float3 forward, float3 position) {
            m.SetColumn(0, right.ToVector4());
            m.SetColumn(1, up.ToVector4());
            m.SetColumn(2, forward.ToVector4());
            m.SetColumn(3, position.ToVector4(1));
        }
        
        public static void AxesWiseMatrix(ref Matrix4x4 m, Vector4 right, Vector4 up, Vector4 forward, Vector4 position) {
            m.SetColumn(0, right);
            m.SetColumn(1, up);
            m.SetColumn(2, forward);
            m.SetColumn(3, position);
        }
        
        public static void AxesWiseMatrixUnscaled(ref Matrix4x4 m, float4 right, float4 up, float4 forward, float4 position) {
            m.SetColumn(0, right.Normalized());
            m.SetColumn(1, up.Normalized());
            m.SetColumn(2, forward.Normalized());
            m.SetColumn(3, position.Normalized());
        }
        
        public static void AxesWiseMatrixUnscaled(ref Matrix4x4 m, Vector4 right, Vector4 up, Vector4 forward, Vector4 position) {
            m.SetColumn(0, right.normalized);
            m.SetColumn(1, up.normalized);
            m.SetColumn(2, forward.normalized);
            m.SetColumn(3, position);
        }

        public static void ToUnitScale(ref Matrix4x4 m) {
            Vector4 right = m.GetColumn(0).normalized;
            Vector4 up = m.GetColumn(1).normalized;
            Vector4 forward = m.GetColumn(2).normalized;
            m.SetColumn(0, right);
            m.SetColumn(1, up);
            m.SetColumn(2, forward);
        }

        [return: ReadOnly]
        public static float4 MultiplyPoint(this float4x4 matrix, float4 v) {
            return math.mul(matrix, v);
        }
        
        [return: ReadOnly]
        public static float3 MultiplyPoint3x4(this float3x4 matrix, float4 v) {
            return math.mul(matrix, v);
        }

        public static float3 GetScale(this float4x4 matrix) {
            return new float3(math.length(matrix.c0.xyz), math.length(matrix.c1.xyz), math.length(matrix.c2.xyz));
        }

        public static Quaternion GetRotation(this Matrix4x4 m) {
            var forward = m.GetColumn(2);
            var up = m.GetColumn(1);
            return Quaternion.LookRotation(forward, up);
        }

        public static quaternion GetRotation(this float4x4 m) {
            var forward = m.c2;
            var up = m.c1;
            return quaternion.LookRotation(forward.xyz, up.xyz);
        }

        public static Vector3 GetPosition(this Matrix4x4 m) {
            return m.GetColumn(3);
        }
 
        public static Vector3 GetScale(this Matrix4x4 m) {
            return new Vector3(m.GetColumn(0).magnitude, m.GetColumn(1).magnitude, m.GetColumn(2).magnitude);
        }

        /// <summary>
        /// Returns world position of local offset, i.e. as if it was a child object of given matrix.
        /// </summary>
        /// <param name="localToWorldMatrixParent">"Parent" matrix</param>
        /// <param name="offset">Offset in the local space of parent matrix</param>
        /// <returns>World position of the offset</returns>
        public static float4 LocalOffsetToWorldPoint(float4x4 localToWorldMatrixParent, float3 offset) {
            var worldToLocalMatrix = math.inverse(localToWorldMatrixParent);
            var worldVector = math.mul(localToWorldMatrixParent, worldToLocalMatrix.c3 + new float4(offset.xyz, 0));
            var worldPoint = localToWorldMatrixParent.c3 + worldVector;
            return worldPoint;
        }
    }
}