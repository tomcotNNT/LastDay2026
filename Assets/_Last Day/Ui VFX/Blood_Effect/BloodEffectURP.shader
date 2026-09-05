Shader "Particles/Blood Effect URP"
{
    Properties
    {
        [Header(Color Controls)]
        [HDR] _BaseColor("Base Color Mult", Color) = (1,1,1,1)
        _LightStr("Lighting Strength", Float) = 0.85
        _AlphaMin("Alpha Clip Min", Range(-0.01,1.01)) = 0.1
        _AlphaSoft("Alpha Clip Softness", Range(0,1)) = 0.022
        _EdgeDarken("Edge Darkening", Float) = 1.0
        _ProcMask("Procedural Mask Strength", Float) = 1.0

        [Header(Mask Controls)]
        _MainTex("Mask Texture", 2D) = "white" {}
        _MaskStr("Mask Strength", Float) = 0.7
        _Columns("Flipbook Columns", Float) = 1
        _Rows("Flipbook Rows", Float) = 1
        _ChannelMask("Channel Mask", Vector) = (1,0,0,0)
        [Toggle] _FlipU("Flip U Randomly", Float) = 0
        [Toggle] _FlipV("Flip V Randomly", Float) = 0

        [Header(Noise Controls)]
        _NoiseTex("Noise Texture", 2D) = "white" {}
        _NoiseAlphaStr("Noise Strength", Float) = 0.8
        _NoiseColorStr("Noise Color Strength", Float) = 0.5
        _ChannelMask2("Channel Mask", Vector) = (1,0,0,0)
        _Randomize("Randomize Noise", Float) = 1.0

        [Header(Warp Controls)]
        _WarpTex("Warp Texture", 2D) = "gray" {}
        _WarpStr("Warp Strength", Float) = 0.1

        [Header(Vertex Physics)]
        _FallOffset("Gravity Offset", Range(-1,0)) = -1.0
        _FallRandomness("Gravity Randomness", Float) = 0.25
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            Name "ForwardLit"

            Tags
            {
                "LightMode"="UniversalForward"
            }

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 color : COLOR;
                float4 texcoord0 : TEXCOORD0;
                float3 texcoord1 : TEXCOORD1;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 uv : TEXCOORD0;
                half4 color : COLOR;
                float3 customData : TEXCOORD1;
                half3 normalWS : TEXCOORD2;
                half3 lighting : TEXCOORD3;
                float fogCoord : TEXCOORD4;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            TEXTURE2D(_NoiseTex);
            SAMPLER(sampler_NoiseTex);

            TEXTURE2D(_WarpTex);
            SAMPLER(sampler_WarpTex);

            float4 _MainTex_ST;
            float4 _NoiseTex_ST;
            float4 _WarpTex_ST;

            half4 _BaseColor;
            half _LightStr;
            half _AlphaMin;
            half _AlphaSoft;
            half _EdgeDarken;
            half _ProcMask;

            half _MaskStr;
            half _Columns;
            half _Rows;
            half4 _ChannelMask;
            half _FlipU;
            half _FlipV;

            half _NoiseAlphaStr;
            half _NoiseColorStr;
            half4 _ChannelMask2;
            half _Randomize;

            half _WarpStr;

            half _FallOffset;
            half _FallRandomness;

            Varyings vert(Attributes v)
            {
                Varyings o;

                float lifetime = v.texcoord0.w;
                lifetime = lifetime * lifetime + (_FallOffset + ((v.texcoord0.z - 0.5) * _FallRandomness)) * lifetime;

                float3 fallOffset = float3(0, v.texcoord1.z * lifetime, 0);

                float2 UVflip = round(frac(float2(v.texcoord0.z * 13, v.texcoord0.z * 8)));
                UVflip = UVflip * 2 - 1;
                UVflip = lerp(1, UVflip, float2(_FlipU, _FlipV));

                float3 positionWS = TransformObjectToWorld(v.positionOS.xyz + fallOffset);

                o.positionCS = TransformWorldToHClip(positionWS);

                o.color = v.color;
                o.color.a *= o.color.a;
                o.color.a += _AlphaMin;

                o.uv.xy = TRANSFORM_TEX(v.texcoord0.xy * UVflip, _MainTex);
                o.uv.zw = o.uv.xy * half2(_Columns, _Rows) + v.texcoord0.z * half2(3,8) * _Randomize;

                o.customData = float3(v.texcoord1.xy, v.texcoord0.z);

                o.normalWS = TransformObjectToWorldNormal(v.normalOS);

                VertexPositionInputs positionInputs = GetVertexPositionInputs(v.positionOS.xyz);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(v.normalOS);

                Light mainLight = GetMainLight();
                half NdotL = saturate(dot(normalInputs.normalWS, mainLight.direction));

                half3 lighting = lerp(1.0h.xxx, mainLight.color * (NdotL + 0.35h), _LightStr);
                o.lighting = lighting;

                o.fogCoord = ComputeFogFactor(o.positionCS.z);

                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                float4 uvWarp = SAMPLE_TEXTURE2D(
                    _WarpTex,
                    sampler_WarpTex,
                    i.uv.zw * _WarpTex_ST.xy +
                    _WarpTex_ST.zw * (i.customData.x + 1) +
                    (float2(5,8) * i.customData.z)
                );

                float2 warp = (uvWarp.xy * 2 - 1) * _WarpStr * i.customData.y;

                half4 mask = SAMPLE_TEXTURE2D(
                    _MainTex,
                    sampler_MainTex,
                    i.uv.xy * _MainTex_ST.xy + warp
                );

                mask = saturate(lerp(1, mask, _MaskStr));

                half2 tempUV = frac(i.uv.xy * half2(_Columns, _Rows)) - 0.5;
                tempUV *= tempUV * 4;

                half edgeMask = saturate(tempUV.x + tempUV.y);
                edgeMask *= edgeMask;
                edgeMask = 1 - edgeMask;
                edgeMask = lerp(1.0, edgeMask, _ProcMask);

                mask *= edgeMask;

                half4 col = max(0.001h, i.color);
                col.a = saturate(dot(mask, _ChannelMask));

                half4 noise4 = SAMPLE_TEXTURE2D(
                    _NoiseTex,
                    sampler_NoiseTex,
                    i.uv.zw * _NoiseTex_ST.xy +
                    _NoiseTex_ST.zw * i.customData.x +
                    warp
                );

                half noise = dot(noise4, _ChannelMask2);
                noise = saturate(lerp(1, noise, _NoiseAlphaStr));

                col.a *= noise;

                half preClipAlpha = col.a;

                half clippedAlpha = saturate(
                    (preClipAlpha * i.color.a - _AlphaMin) / (_AlphaSoft)
                );

                col.a = clippedAlpha;

                half edge = 1 - saturate(preClipAlpha * clippedAlpha);
                edge *= edge;
                edge = 1 - edge;
                edge = edge + lerp(0, noise - 0.5, _NoiseColorStr);

                edge = saturate(lerp(0.71, edge * edge, _EdgeDarken));

                col.a *= saturate(lerp(1.25, _BaseColor.a, edge));

                col.rgb *= lerp(
                    min(col.rgb * col.rgb * col.rgb * 0.3, 1.0),
                    0.71,
                    edge
                );

                col.rgb *= i.lighting * _BaseColor.rgb;

                col.rgb = MixFog(col.rgb, i.fogCoord);

                return col;
            }

            ENDHLSL
        }
    }

    FallBack Off
}