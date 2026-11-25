Shader "Moonlit/LanternDarkness"
{
    Properties
    {
        _LightCenter ("Lantern world position", Vector) = (0,0,0,0)
        _Radius ("Light radius", Float) = 6.4
        _Softness ("Soft edge", Float) = 2.3
        _Darkness ("Gameplay darkness opacity", Range(0,1)) = 0.925
        _FocusCenter ("Tutorial focus position", Vector) = (0,0,0,0)
        _FocusRadius ("Tutorial spotlight radius", Float) = 2.5
        _FocusBlend ("Tutorial focus amount", Range(0,1)) = 0
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent+400" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        ZTest Always
        Cull Off
        Pass
        {
            Tags { "LightMode"="Universal2D" }
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings { float4 positionCS : SV_POSITION; float2 worldXY : TEXCOORD0; };
            CBUFFER_START(UnityPerMaterial)
                float4 _VillageLights[4];
                float4 _LightCenter;
                float _Radius;
                float _Softness;
                float _Darkness;
                float4 _FocusCenter;
                float _FocusRadius;
                float _FocusBlend;
            CBUFFER_END
            Varyings Vert(Attributes input)
            {
                Varyings output;
                float3 world = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(world);
                output.worldXY = world.xy;
                return output;
            }
            half4 Frag(Varyings input) : SV_Target
            {
                float distanceFromLantern = distance(input.worldXY, _LightCenter.xy);
                float shadow = smoothstep(max(.01, _Radius - _Softness), _Radius, distanceFromLantern);
                float warmth = 0;
                [unroll] for (int index = 0; index < 4; index++)
                {
                    float4 light = _VillageLights[index];
                    float reach = 1 - smoothstep(.08, 1, distance(input.worldXY,light.xy) / max(.01,light.z));
                    warmth = max(warmth,reach * light.w);
                }
                shadow *= 1 - warmth * .88;
                float tutorialShadow = smoothstep(_FocusRadius - .55, _FocusRadius, distance(input.worldXY, _FocusCenter.xy));
                // Normal play retains ambient visibility. Tutorials still isolate the subject.
                return half4(.007 + warmth*.16,.010 + warmth*.06,.031 + warmth*.011, lerp(shadow * _Darkness, tutorialShadow, _FocusBlend));
            }
            ENDHLSL
        }
    }
}
