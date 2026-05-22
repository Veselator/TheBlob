Shader "TheBlob/DiagonalStripes"
{
    Properties
    {
        _Color1 ("Color 1", Color) = (0.6, 0.9, 0.6, 1)
        _Color2 ("Color 2", Color) = (0.5, 0.8, 0.5, 1)
        _StripeWidth ("Stripe Width", Float) = 0.1
        _Gap ("Gap Between Stripes", Float) = 0.1
        _Scale ("Overall Scale", Float) = 1.0
        _Angle ("Angle (degrees)", Float) = 45
        _Offset ("Scroll Offset", Vector) = (0, 0, 0, 0)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }
        LOD 100

        Pass
        {
            Tags { "LightMode"="Universal2D" }
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
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
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _Color1;
                float4 _Color2;
                float _StripeWidth;
                float _Gap;
                float _Scale;
                float _Angle;
                float4 _Offset;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            float4 frag(Varyings IN) : SV_Target
            {
                float2 uv = (IN.uv + _Offset.xy) * _Scale;

                float rad = _Angle * 3.14159265 / 180.0;
                float projected = uv.x * cos(rad) + uv.y * sin(rad);

                // One full period = stripe + gap
                float period = _StripeWidth + _Gap;
                float pos = frac(projected / period) * period;

                // Color1 for the stripe portion, Color2 for the gap
                float mask = step(_StripeWidth, pos);

                return lerp(_Color1, _Color2, mask);
            }
            ENDHLSL
        }
    }
}
