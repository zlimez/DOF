using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEditor;

namespace Assets.URP.Runtime.Bokeh
{
    [Serializable]
    public sealed class BokehRenderFeature : ScriptableRendererFeature
    {
        [SerializeField] Shader bokehShader;
        Material _mat;
        BokehRenderPass _bokehRenderPass;

        public override void Create()
        {
            if (bokehShader == null) return;

            _mat = new Material(bokehShader);
            _bokehRenderPass = new BokehRenderPass(_mat);
        }

        // Here you can inject one or multiple render passes in the renderer.
        // This method is called when setting up the renderer once per-camera.
        public override void AddRenderPasses(
            ScriptableRenderer renderer,
            ref RenderingData renderingData
        )
        {
            if (renderingData.cameraData.cameraType == CameraType.Game) renderer.EnqueuePass(_bokehRenderPass);
        }

        protected override void Dispose(bool disposing)
        {
            _bokehRenderPass.Dispose();
#if UNITY_EDITOR
            if (EditorApplication.isPlaying)
            {
                Destroy(_mat);
            }
            else
            {
                DestroyImmediate(_mat);
            }
#else
                Destroy(_mat);
#endif
        }
    }
}
