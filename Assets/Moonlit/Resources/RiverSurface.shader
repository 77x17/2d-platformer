Shader "Moonlit/RiverSurface"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite", 2D) = "white" {}
        _Waterline ("Water level, chapter, current phase", Vector) = (0,1,0,0)
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }
        Cull Off ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            Tags { "LightMode"="Universal2D" }
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
                float4 _Waterline;
            CBUFFER_END
            TEXTURE2D(_MoonlitBankTexture); SAMPLER(sampler_MoonlitBankTexture);
            float4 _MoonlitBankRect, _MoonlitBankPlane, _CuoiTerrainLight, _CuoiVillageLights[4];
            struct Varyings { float4 clipPos : SV_POSITION; float2 world : TEXCOORD0; };
            Varyings Vert(float3 position : POSITION)
            {
                Varyings o;
                float3 world = TransformObjectToWorld(position);
                o.clipPos = TransformWorldToHClip(world);
                o.world = world.xy;
                return o;
            }
            float Hash(float2 p)
            {
                float3 a = frac(float3(p.xyx) * float3(.1031,.1030,.0973));
                a += dot(a, a.yzx + 33.33);
                return frac((a.x + a.y) * a.z);
            }
            float ReflectionFromLamp(float2 pixel, float depth, float ripple, float4 lamp)
            {
                float height = lamp.y - _Waterline.x;
                float spread = .16 + .24 * depth + .1 * saturate(height);
                float horizontal = abs(pixel.x - lamp.x + ripple * (.09 + .08 * depth));
                float column = saturate(1.0 - horizontal / spread);
                return column * column * exp2(-depth * .85 - abs(height) * .27) * lamp.w;
            }
            half4 Frag(Varyings i) : SV_Target
            {
                // Step in world pixels to keep the silhouette and reflection the same size as the art.
                float2 p = floor(i.world * 40.0) / 40.0;
                float t = _Time.y;
                float x = p.x + _Waterline.z;
                float surface = .039 * sin(x * 1.04 - t * 1.62) + .024 * sin(x * 2.82 + t * 1.14);
                float depth = _Waterline.x + surface - p.y;
                clip(depth);
                float row = floor(depth * 9.0);
                float rowSeed = Hash(float2(row, _Waterline.y));
                float ripple = sin(x * 1.83 + rowSeed * 12.4 + t * (1.1 + rowSeed))
                             + .42 * sin(x * 3.17 - row * .42 - t * 1.9);
                float2 reflected = float2(p.x + ripple * (.13 + depth * .19), _Waterline.x + depth * 1.24);
                float2 uv = (reflected - _MoonlitBankPlane.xy) * _MoonlitBankPlane.zw;
                float visibleBank = step(0, uv.y) * step(uv.y, 1);
                uv = _MoonlitBankRect.xy + float2(frac(uv.x), saturate(uv.y)) * _MoonlitBankRect.zw;
                half4 bank = SAMPLE_TEXTURE2D(_MoonlitBankTexture, sampler_MoonlitBankTexture, uv);

                float crest = smoothstep(.58, 1.17, ripple) * (1 - smoothstep(.012,.065,frac(depth * 9.0) / 9.0));
                float pools = .5 + .5 * sin(x * .36 + rowSeed * 6.3 - t * .46);
                float fade = exp2(-depth * .51);
                half3 shallows = _Waterline.y < 1.5 ? half3(.23,.39,.43) :
                                _Waterline.y < 2.5 ? half3(.17,.41,.39) : half3(.24,.36,.47);
                half3 deep = half3(.07,.16,.23);
                half3 base = lerp(deep, shallows, fade * (.78 + .22 * pools));
                #ifndef UNITY_COLORSPACE_GAMMA
                    base *= base;
                #endif
                base = lerp(base, bank.rgb * half3(.53,.76,.85), .53 * bank.a * visibleBank * fade);
                base += half3(.037,.085,.102) * crest * pools * fade;
                base += half3(.038,.072,.081) * (1 - smoothstep(.0,.045,depth)) * (.2 + .8 * pools);
                float candle = ReflectionFromLamp(p,depth,ripple,float4(_CuoiTerrainLight.xy,0,1.0 + _CuoiTerrainLight.z));
                [unroll] for (int k=0; k<4; k++) candle += ReflectionFromLamp(p,depth,ripple,_CuoiVillageLights[k]);
                base += half3(.78,.31,.07) * candle * (.12 + crest * .84);
                return half4(base, lerp(.74,.98,1-fade));
            }
            ENDHLSL
        }
    }
}
