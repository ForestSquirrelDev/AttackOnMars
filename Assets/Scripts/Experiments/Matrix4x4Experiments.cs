using Den.Tools.Matrices;
using Unity.Mathematics;
using UnityEngine;
using Utils;

namespace Experiments {
    public class Matrix4x4Experiments : MonoBehaviour {
        [SerializeField] private Transform _parent;
        [SerializeField] private Transform _child;
        [SerializeField] private Vector3 _offset;
        
        private void OnDrawGizmos() {
            var matrix0 = _parent.localToWorldMatrix;
            var t1 = _parent.GetChild(0);
            var matrix1 = t1.localToWorldMatrix;
            var add = Add(matrix0, matrix1);
            var target = new Matrix4x4();
            target.SetColumn(0, matrix1.GetColumn(0));
            target.SetColumn(1, matrix1.GetColumn(1));
            target.SetColumn(2, matrix1.GetColumn(2));
            var pos = matrix1.GetColumn(3);
            var inheritedPos = pos + (Vector4)matrix1.MultiplyPoint3x4(t1.worldToLocalMatrix.GetColumn(3) + (Vector4)_offset);
            target.SetColumn(3, inheritedPos);
            Debug.Log($"{_child.worldToLocalMatrix.GetColumn(3)}");
            //Matrix4x4.

            _child.localScale = target.GetScale();
            _child.rotation = target.GetRotation();
            _child.position = target.GetPosition();
        }

        private Matrix4x4 Add(Matrix4x4 a, Matrix4x4 b) {
            var res = new Matrix4x4();
            
            // add rotation
            // forward vector
            res.m02 = a.m02 + b.m02;
            res.m12 = a.m12 + b.m12;
            res.m22 = a.m22 + b.m22;
            //res.m32 = a.m32 + b.m32;
            // up vector
            res.m01 = a.m01 + b.m01;
            res.m11 = a.m11 + b.m11;
            res.m21 = a.m21 + b.m21;
            //res.m31 = a.m31 + b.m31;
            //right vector
            res.m00 = a.m00 + b.m00;
            res.m10 = a.m10 + b.m10;
            res.m20 = a.m20 + b.m20;
            //res.m30 = a.m30 + b.m30;
            
            // add position
            res.m03 = a.m03 + b.m03;
            res.m13 = a.m13 + b.m13;
            res.m23 = a.m23 + b.m23;
            res.m33 = 1;

            return res;
        }
    }
}