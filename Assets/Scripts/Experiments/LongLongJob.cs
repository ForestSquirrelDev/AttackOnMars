using Unity.Jobs;
using UnityEngine;

namespace Experiments {
    public struct LongLongJob : IJob {
        private int i;
        public void Execute() {
            i = 10000;
            for (int j = 0; j < i; j++) {
                var PerlinNoiseKekw = Mathf.PerlinNoise(i, j);
                for (int k = 0; k < i; k++) {
                    var PerlinNoiseKekw1 = Mathf.PerlinNoise(i, j);
                    for (int kekw = 0; kekw < i; kekw++) {
                        var PerlinNoiseKekw2 = Mathf.PerlinNoise(i, j);
                        i++;
                    }
                }
            }
            Debug.Log($"Executed long long job");
        }
    }
}