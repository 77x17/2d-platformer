Shader "Moonlit/Banyan canopy"
{
    Properties
    {
        [PerRendererData] _MainTex("Sprite", 2D) = "white" {}
        [HideInInspector] _Color("Tint", Color) = (1,1,1,1)
        [HideInInspector] _RendererColor("Renderer tint", Color) = (1,1,1,1)
        [PerRendererData] _GuardianPose("Gesture time and strength", Vector) = (0,0,0,0)
        [PerRendererData] _TreePhase("Wind phase", Float) = 0
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
            struct Varyings { COMMON_2D_OUTPUTS half4 color : COLOR; };
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/2DCommon.hlsl"
            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                float4 _MainTex_TexelSize;
                float _TreePhase;
                float4 _GuardianPose;
            CBUFFER_END
            #include "BanyanPose.cginc"

            Varyings Vertex(Attributes input)
            {
                UNITY_SETUP_INSTANCE_ID(input);
                SetUpSpriteInstanceProperties();
                input.positionOS = UnityFlipSprite(input.positionOS, unity_SpriteProps.xy);
                Varyings output = CommonUnlitVertex(input);
                output.color = input.color * _Color * unity_SpriteColor;
                return output;
            }

            half4 Fragment(Varyings input) : SV_Target
            {
                float2 uv = BanyanPoseUV(input.uv, _GuardianPose.xy, _MainTex_TexelSize.xy);
                float time = _Time.y + _TreePhase;
                // Branch tips and aerial roots follow the crown with a slight delay.
                // The lower trunk, face and soil contact keep the original texels.
                float tip = smoothstep(.08, .40, abs(uv.x - .5));
                float weight = smoothstep(.49, .91, uv.y) + tip * smoothstep(.29, .63, uv.y) * .55;
                float gust = sin(time * 1.48 - uv.y * 2.8) + .32 * sin(time * 2.34 + uv.x * 9);
                float nod = sin(time * 1.48 + uv.x * 7) * tip;
                float2 motion = weight * float2(.007 * gust, .004 * nod);
                if (_GuardianPose.y > 0) motion.x *= smoothstep(0., .04, min(uv.x, 1.-uv.x));
                float2 offset = round(motion * _MainTex_TexelSize.zw) * _MainTex_TexelSize.xy;
                half4 ink = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv - offset);
                return ink * input.color;
            }
            ENDHLSL
        }
    }
}
