using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace Game.WeirdHacks {
    public class AnimatorTextureBaker : MonoBehaviour {
        public ComputeShader InfoTextureGenerator;
        private void Awake() {
            StartCoroutine(BakeAnimationRoutine());
        }

        private IEnumerator BakeAnimationRoutine() {
            var animator = GetComponent<Animator>();
            var clips = animator.runtimeAnimatorController.animationClips;
            var skin = GetComponentInChildren<SkinnedMeshRenderer>();
            var vCount = skin.sharedMesh.vertexCount;

            var mesh = new Mesh();
            animator.speed = 0;
            var textureWidth = Mathf.NextPowerOfTwo(vCount);
            foreach (var clip in clips) {
                int index = 0;
                var frames = Mathf.NextPowerOfTwo((int)(clip.length / 0.05f));
                var info = new List<VertInfo>();

                var positionRenderTexture = new RenderTexture(textureWidth, frames, 0, RenderTextureFormat.ARGBHalf);
                var normalRenderTexture = new RenderTexture(textureWidth, frames, 0, RenderTextureFormat.ARGBHalf);
                positionRenderTexture.name = $"{name}.{clip.name}.positionTexture";
                normalRenderTexture.name = $"{name}.{clip.name}.normalTexture";

                foreach (var renderTexture in new[] { positionRenderTexture, normalRenderTexture }) {
                    renderTexture.enableRandomWrite = true;
                    renderTexture.Create();
                    RenderTexture.active = renderTexture;
                    GL.Clear(true, true, Color.clear);
                }
                
                animator.Play(clip.name);
                yield return null;
                for (int i = 0; i < frames; i++) {
                    animator.Play(clip.name, 0, (float) i / frames);
                    yield return null;
                    skin.BakeMesh(mesh);
                    info.AddRange(Enumerable.Range(0, vCount).Select(idx => new VertInfo() {
                        Position = mesh.vertices[idx],
                        Normal = mesh.normals[idx]
                    }));
                }
                var buffer = new ComputeBuffer(info.Count, System.Runtime.InteropServices.Marshal.SizeOf(typeof(VertInfo)));
                buffer.SetData(info);

                var kernel = InfoTextureGenerator.FindKernel("CSMain");
                uint x, y, z;
                InfoTextureGenerator.GetKernelThreadGroupSizes(kernel, out x, out y, out z);
                
                InfoTextureGenerator.SetInt("VertCount", vCount);
                InfoTextureGenerator.SetBuffer(kernel, "_MeshInfo", buffer);
                InfoTextureGenerator.SetTexture(kernel, "OutPosition", positionRenderTexture);
                InfoTextureGenerator.SetTexture(kernel, "OutNormal", normalRenderTexture);
                
                InfoTextureGenerator.Dispatch(kernel, vCount / (int)x + 1, frames / (int)y + 1, (int)z);
                buffer.Release();
#if UNITY_EDITOR
                var posTex = Convert(positionRenderTexture);
                var normalTex = Convert(normalRenderTexture);
                
                Graphics.CopyTexture(positionRenderTexture, posTex);
                Graphics.CopyTexture(normalRenderTexture, normalTex);
                
                AssetDatabase.CreateAsset(posTex, ($"Assets/Textures/{++index}position.asset"));
                AssetDatabase.CreateAsset(normalTex, ($"Assets/Textures/{++index}normal.asset"));
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
#endif
            }
        }

        public Texture2D Convert(RenderTexture renderTexture) {
            var texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBAHalf, false);
            RenderTexture.active = renderTexture;
            texture.ReadPixels(Rect.MinMaxRect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            RenderTexture.active = null;
            return texture;
        }

        private struct VertInfo {
            public Vector3 Position;
            public Vector3 Normal;
        }
    }
}
