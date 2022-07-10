using UnityEngine;

namespace Game.Ecs.Utils {
    public static class ParticleSystemExtensions {
        public static void PlaySafe(this ParticleSystem p, bool withChildren) {
            if (!p.isPlaying)
                p.Play(withChildren);
        }
        
        public static void StopSafe(this ParticleSystem p, bool withChildren) {
            if (p.isPlaying)
                p.Stop(withChildren);
        }
    }
}