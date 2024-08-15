using System;
using UnityEngine.Rendering;

namespace Assets.URP.Runtime.Bokeh
{
    [Serializable]
    [VolumeComponentMenu("Digital Glitch")]
    public class BokehVolumeComponent : VolumeComponent
    {
        public static readonly int MAX_SAMPLES = 32;
        
        public BoolParameter isCircle = new(true);
        public ClampedIntParameter sides = new(4, 3, 10); // TODO: Disable sides if isCircle + enforce even sides (kernel limitation)
        public ClampedFloatParameter radius = new(0.5f, 0f, 10f);
        public ClampedFloatParameter focusDist = new(5f, 0.1f, 100f);
        public ClampedFloatParameter rotation = new(0f, 0f, 360f);
        public ClampedIntParameter samples = new(8, 1, MAX_SAMPLES);
    }
}