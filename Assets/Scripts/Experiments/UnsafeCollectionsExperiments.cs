using System;
using System.Collections;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace Experiments {
    public class UnsafeCollectionsExperiments : MonoBehaviour {
        private UnsafeList<int> _l;
        
        private void Awake() {
            _l = new UnsafeList<int>(8388608, Allocator.Persistent);
            StartCoroutine(DoShit());
        }

        private IEnumerator DoShit() {
            while (true) {
                for (int i = 0; i < 8388608; i++) {
                    _l.Add(i);
                }
                yield return null;
                _l.Clear();
            }
        }

        private void OnDestroy() {
            _l.Dispose();
        }
    }
}