Shader "Moonlit/Cuoi paper and moon rim"
{
    Properties
    {
        [PerRendererData] _MainTex("Sprite", 2D) = "white" {}
        [HideInInspector] _Color("Tint", Color) = (1,1,1,1)
        [HideInInspector] _RendererColor("Renderer tint", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        Cull Off
        ZWrite Off
        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vertex
            #pragma fragment Fragment
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"
            struct Attributes { COMMON_2D_INPUTS half4 color : COLOR; };
            struct Varyings { COMMON_2D_OUTPUTS half4 color : COLOR; float2 pose : TEXCOORD1; };
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/2DCommon.hlsl"
            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                float4 _MainTex_TexelSize;
                float4 _PinA, _PinB, _BendA, _BendB, _PoseUvScale;
            CBUFFER_END
            Varyings Vertex(Attributes input)
            {
                UNITY_SETUP_INSTANCE_ID(input);
                SetUpSpriteInstanceProperties();
                float2 pose = input.positionOS.xy;
                input.positionOS = UnityFlipSprite(input.positionOS, unity_SpriteProps.xy);
                Varyings output = CommonUnlitVertex(input);
                output.pose = pose;
                output.color = input.color * _Color * unity_SpriteColor;
                return output;
            }
            half4 Fragment(Varyings input) : SV_Target
            {
                float2 texel = _MainTex_TexelSize.xy;
                float2 a = (input.pose - _PinA.xy) * _PinA.zw;
                float2 b = (input.pose - _PinB.xy) * _PinB.zw;
                float2 shift = _BendA.xy * (1 - smoothstep(.035, 1, dot(a,a))) + _BendB.xy * (1 - smoothstep(.035, 1, dot(b,b)));
                float2 uv = input.uv - shift * _PoseUvScale.xy;
                half4 ink = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
                half back = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + texel * float2(-3.5, 4)).a;
                half front = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + texel * float2(4, .75)).a;
                // The original right-facing sheet always has the star on +U. A
                // rendered flip also flips its warm key. Moonlight picks out the hat.
                half elevation = saturate(input.pose.y / 1.85);
                half moon = saturate(ink.a - back) * (0.23 + 0.36 * elevation);
                half flame = saturate(ink.a - front) * (0.45 - 0.13 * elevation);
                ink.rgb = lerp(ink.rgb, half3(0.40,0.62,0.69), moon);
                ink.rgb = lerp(ink.rgb, half3(1.0,0.63,0.28), flame);
                return ink * input.color;
            }
            ENDHLSL
        }
    }
}
