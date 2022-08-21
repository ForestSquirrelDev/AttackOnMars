using System;
using UnityEngine;
using Utils.Maths;

namespace Experiments {
    public class TunableSigmoidExperiments : MonoBehaviour {
        [SerializeField] private float _k;
        [SerializeField] private float _t;

        private void Update() {
            Debug.Log(MathfUtility.ReverseTunableSigmoid(_k, _t));
        }
    }
}