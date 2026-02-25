Shader "Game/RadialFog"
{
    Properties
    {
        _FogColor ("Fog Color", Color) = (0.85, 0.88, 0.9, 1)
        _Center ("Center (viewport 0-1)", Vector) = (0.5, 0.5, 0, 0)
        _ClearRadius ("Clear Radius X,Y (viewport)", Vector) = (0.2, 0.2, 0, 0)
        _Softness ("Softness (0-2)", Range(0, 2)) = 0.15
        _MaxAlpha ("Max Fog Alpha", Range(0, 1)) = 0.9
    }
    SubShader
    {
        Tags { "Queue" = "Overlay" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        ZTest Always
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 positionHCS : SV_POSITION;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _FogColor;
                half4 _Center;
                half4 _ClearRadius;
                half _Softness;
                half _MaxAlpha;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 uv = IN.uv;
                float2 center = _Center.xy;
                float2 radius = max(_ClearRadius.xy, 0.001);

                float2 d = (uv - center) / radius;
                float dist = length(d);

                half alpha = 0;
                if (dist > 1.0)
                {
                    float t = saturate((dist - 1.0) / max(_Softness, 0.001));
                    alpha = _MaxAlpha * t;
                }

                return half4(_FogColor.rgb, alpha);
            }
            ENDHLSL
        }
    }
    Fallback "Universal Render Pipeline/Unlit"
}
