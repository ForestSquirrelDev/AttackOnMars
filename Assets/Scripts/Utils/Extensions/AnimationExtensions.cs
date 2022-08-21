using UnityEngine;

namespace Utils.Extensions {
    // https://forum.unity.com/threads/need-way-to-evaluate-animationcurve-in-the-job.532149/
    public static class AnimationExtensions {
        public static float[] GenerateCurveArray(this AnimationCurve self)
        {
            float[] returnArray = new float[256];
            for (int j = 0; j <= 255; j++)
            {
                returnArray[j] = self.Evaluate(j / 256f);            
            }              
            return returnArray;
        }
    }
}
