// Additive, camera-facing glow (soft halo + slowly turning 4-point rays) for pickups.
// The quad's center is bent with the same curve as Custom/CurvedWorld so it stays on its object.
Shader "Custom/CurvedGlow"
{
    Properties
    {
        _BaseColor("Color", Color) = (1, 1, 1, 1)
        _Intensity("Intensity", Float) = 1.5
        _RaySharpness("Ray Sharpness", Float) = 18
        _Spin("Ray Spin (rad/s)", Float) = 0.8
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True" }

        Pass
        {
            Name "Glow"
            Tags { "LightMode" = "UniversalForward" }
            Blend One One
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half _Intensity;
                half _RaySharpness;
                half _Spin;
            CBUFFER_END

            float _CurveStrength;
            float3 _CurveOrigin;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float fogFactor : TEXCOORD1;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                float3 centerWS = TransformObjectToWorld(float3(0, 0, 0));
                float2 d = centerWS.xz - _CurveOrigin.xz;
                centerWS.y -= _CurveStrength * dot(d, d);

                // Billboard: offset the quad corners in view space, sized by the object's scale.
                float scale = length(float3(UNITY_MATRIX_M._m00, UNITY_MATRIX_M._m10, UNITY_MATRIX_M._m20));
                float3 centerVS = TransformWorldToView(centerWS);
                float3 positionVS = centerVS + float3(input.positionOS.xy * scale, 0);
                output.positionCS = TransformWViewToHClip(positionVS);
                output.uv = input.uv;
                output.fogFactor = ComputeFogFactor(output.positionCS.z);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 p = input.uv * 2 - 1;
                float r = length(p);
                float edge = saturate(1 - r);

                float halo = exp(-r * r * 7) * edge;

                float angle = _Time.y * _Spin;
                float s = sin(angle), c = cos(angle);
                float2 q = float2(c * p.x - s * p.y, s * p.x + c * p.y);
                float rays = (exp(-abs(q.x) * _RaySharpness) + exp(-abs(q.y) * _RaySharpness)) * edge * edge;

                half3 color = _BaseColor.rgb * (halo * 0.9 + rays * 0.7) * _Intensity;
                // Additive: fog fades the glow out instead of tinting it.
                color = MixFogColor(color, half3(0, 0, 0), input.fogFactor);
                return half4(color, 1);
            }
            ENDHLSL
        }
    }
}
