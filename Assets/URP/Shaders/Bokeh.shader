Shader "Effects/Bokeh"
{
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        LOD 100
        ZWrite Off Cull Off

        Pass
        {
            Name "BokehCoC"

            HLSLPROGRAM
                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
                #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

                uniform float _Radius;
                uniform float _FocusDist;
                uniform int _Samples;

                StructuredBuffer<float> _Intensities;
                StructuredBuffer<float2> _Offsets;

                float4 _BlitTexture_TexelSize;

                #pragma vertex Vert
                #pragma fragment FragCoC
                #pragma target 4.5

                // Idea: Calculate CoC based on depth, fixed vertical and horizontal samples if out of bounds no effect otherwise (single pass)
                float4 FragCoC(Varyings input) : SV_Target
                {
                    float2 uv = input.texcoord;
                    float depth = SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_PointClamp, uv).r;
                    float linearEyeDepth = 1.0 / (_ZBufferParams.z * depth + _ZBufferParams.w);
                    
                    // At FocusDist image should be sharpest correspond to 0 a U shaped curve with minima at (FocusDist, 0), 
                    float dfAxis = saturate(abs(linearEyeDepth / _FocusDist - 1.0));

                    float coc = _Radius * 0.01 * dfAxis * dfAxis;       

                    float3 color = float3(0.0, 0.0, 0.0);
                    float div = 0.0;
                    float2 xyRatio = float2(1.0, _BlitTexture_TexelSize.z / _BlitTexture_TexelSize.w);
                    for (int i = 0; i < _Samples; ++i)
                    {
                        for (int j = 0; j < _Samples; ++j)
                        {
                            float2 offset = _Offsets[i * _Samples + j];
                            float intensity = _Intensities[i * _Samples + j];
                            color += SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv + offset * coc * xyRatio).rgb * intensity;
                            div += intensity;
                        }
                    }

                    float3 acc = saturate(color / div);
                    return float4(acc, 1.0);
                }
            ENDHLSL
        }

        Pass 
        {
            Name "Std"

            HLSLPROGRAM
                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
                #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

                #pragma vertex Vert
                #pragma fragment FragBlit

                float4 FragBlit (Varyings input) : SV_Target
                {
                    return SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, input.texcoord);
                }
            ENDHLSL
        }
    }
}