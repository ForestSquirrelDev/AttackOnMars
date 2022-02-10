using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Utils;

namespace Game.Ecs.Utils {
    public static class BoilerplateShortcuts {
        public static void SetMultipleComponentDatas(Entity entity, EntityManager manager, params IComponentData[] components) {
            foreach (IComponentData component in components) {
                manager.SetComponentData(entity, component);
            }
        }

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
            Vector4 pos = position.ToVector4();
            pos.w = 1;
            m.SetColumn(3, pos);
        }
        
        public static void AxesWiseMatrix(ref Matrix4x4 m, Vector4 right, Vector4 up, Vector4 forward, Vector4 position) {
            m.SetColumn(0, right);
            m.SetColumn(1, up);
            m.SetColumn(2, forward);
            position.w = 1;
            m.SetColumn(3, position);
        }
        
        public static void AxesWiseMatrixUnscaled(ref Matrix4x4 m, float3 right, float3 up, float3 forward, float3 position) {
            m.SetColumn(0, right.ToVector4().normalized);
            m.SetColumn(1, up.ToVector4().normalized);
            m.SetColumn(2, forward.ToVector4().normalized);
            Vector4 pos = position.ToVector4();
            pos.w = 1;
            m.SetColumn(3, pos);
        }
        
        public static void AxesWiseMatrixUnscaled(ref Matrix4x4 m, Vector4 right, Vector4 up, Vector4 forward, Vector4 position) {
            m.SetColumn(0, right.normalized);
            m.SetColumn(1, up.normalized);
            m.SetColumn(2, forward.normalized);
            position.w = 1;
            m.SetColumn(3, position);
        }
    }
}