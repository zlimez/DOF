using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Assets.URP.Runtime.Bokeh
{
    public class BokehRenderPass : ScriptableRenderPass
    {
        static readonly int s_radiusId = Shader.PropertyToID("_Radius");
        static readonly int s_focusDistId = Shader.PropertyToID("_FocusDist");
        static readonly int s_samplesId = Shader.PropertyToID("_Samples");
        static readonly int b_intensitiesId = Shader.PropertyToID("_Intensities");
        static readonly int b_offsetsId = Shader.PropertyToID("_Offsets");
        readonly ProfilingSampler _pfSampler = new("Bokeh Blit");
        readonly Material _bokehMat;
        readonly ComputeBuffer _sampleOffsetsBuffer, _sampleIntensitiesBuffer;
        RenderTextureDescriptor _bokehTexDesc;
        RTHandle _bokehTexHandle;

        public BokehRenderPass(Material bokehMat)
        {
            this._bokehMat = bokehMat;
            _bokehTexDesc = new RenderTextureDescriptor(Screen.width, Screen.height, RenderTextureFormat.Default, 0);
            renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing; // NOTE: To confirm
            int bufferSize = BokehVolumeComponent.MAX_SAMPLES * BokehVolumeComponent.MAX_SAMPLES;
            _sampleOffsetsBuffer = new ComputeBuffer(bufferSize, sizeof(float) * 2);
            _sampleIntensitiesBuffer = new ComputeBuffer(bufferSize, sizeof(float));
            _bokehMat.SetBuffer(b_intensitiesId, _sampleIntensitiesBuffer);
            _bokehMat.SetBuffer(b_offsetsId, _sampleOffsetsBuffer);
        }

        private void UpdateBokehSettings()
        {
            if (_bokehMat == null) return;
            var volumeComponent = VolumeManager.instance.stack.GetComponent<BokehVolumeComponent>();
            _bokehMat.SetFloat(s_radiusId, volumeComponent.radius.value);
            _bokehMat.SetFloat(s_focusDistId, volumeComponent.focusDist.value);
            _bokehMat.SetInt(s_samplesId, volumeComponent.samples.value);

            float[] intensities;
            Vector2[] offsets;
            if (volumeComponent.isCircle.value)
                (intensities, offsets) = BokehFuncs.ComputeCircleIntensities(volumeComponent.samples.value);
            else
                (intensities, offsets) = BokehFuncs.ComputePolyIntensities(volumeComponent.samples.value, volumeComponent.sides.value, volumeComponent.rotation.value);
            
            Logger.Log(intensities, "intensities", true, "intensities");
            Logger.Log(offsets, "offsets", true, "offsets");
            _sampleIntensitiesBuffer.SetData(intensities);
            _sampleOffsetsBuffer.SetData(offsets);
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            _bokehTexDesc.width = cameraTextureDescriptor.width;
            _bokehTexDesc.height = cameraTextureDescriptor.height;
            RenderingUtils.ReAllocateIfNeeded(ref _bokehTexHandle, _bokehTexDesc);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get();
            RTHandle camTargHandle = renderingData.cameraData.renderer.cameraColorTargetHandle;

            using (new ProfilingScope(cmd, _pfSampler))
            {
                UpdateBokehSettings();
                Blit(cmd, camTargHandle, _bokehTexHandle, _bokehMat, 0);
                Blit(cmd, _bokehTexHandle, camTargHandle, _bokehMat, 1);
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public void Dispose()
        {
#if UNITY_EDITOR
            if (EditorApplication.isPlaying)
            {
                Object.Destroy(_bokehMat);
            }
            else
            {
                Object.DestroyImmediate(_bokehMat);
            }
#else
            Object.Destroy(bokehMat);
#endif
            // if (_bokehTexHandle != null)
            // {
            //     _bokehTexHandle.Release();
            //     _bokehTexHandle = null;
            // }
            _sampleIntensitiesBuffer.Release();

        }
    }
}
