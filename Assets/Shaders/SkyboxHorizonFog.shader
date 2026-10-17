// Cubemap skybox that fades into the fog color at and below the horizon. The curved world bends
// the street down out of sight, so the lower half of the sky is visible; blending it into the fog
// keeps the road's far end, the fog and the sky one seamless haze (and follows the per-city fog).
Shader "Custom/SkyboxHorizonFog"
{
    Properties
    {
        [NoScaleOffset] _Tex("Cubemap", Cube) = "grey" {}
        _Tint("Tint", Color) = (1, 1, 1, 1)
        _Exposure("Exposure", Range(0, 8)) = 1
        _Rotation("Rotation", Range(0, 360)) = 0
        _HorizonLow("Fog fully below (dir.y)", Range(-0.5, 0.5)) = -0.02
        _HorizonHigh("Sky fully above (dir.y)", Range(-0.5, 1)) = 0.15
    }

    SubShader
    {
        Tags { "Queue" = "Background" "RenderType" = "Background" "PreviewType" = "Skybox" "RenderPipeline" = "UniversalPipeline" }
        Cull Off
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURECUBE(_Tex);
            SAMPLER(sampler_Tex);

            CBUFFER_START(UnityPerMaterial)
                half4 _Tint;
                half _Exposure;
                float _Rotation;
                float _HorizonLow;
                float _HorizonHigh;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 direction : TEXCOORD0;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                float angle = _Rotation * PI / 180.0;
                float s = sin(angle), c = cos(angle);
                float3 d = input.positionOS.xyz;
                output.direction = float3(c * d.x - s * d.z, d.y, s * d.x + c * d.z);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float3 dir = normalize(input.direction);
                half3 sky = SAMPLE_TEXTURECUBE(_Tex, sampler_Tex, dir).rgb * _Tint.rgb * _Exposure;
                float skyAmount = smoothstep(_HorizonLow, _HorizonHigh, dir.y);
                return half4(lerp(unity_FogColor.rgb, sky, skyAmount), 1);
            }
            ENDHLSL
        }
    }
}
