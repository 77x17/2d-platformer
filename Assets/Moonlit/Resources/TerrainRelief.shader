Shader "Moonlit/TerrainRelief"
{
    Properties
    {
        _MainTex ("Earth cross section", 2D) = "white" {}
        _Crest ("Painted ground edges", 2D) = "white" {}
        _CrestData ("Atlas row, pixels per unit, atlas height, chapter", Vector) = (150,137,941,0)
        _WetStone ("Wet stone", Float) = 0
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
            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            TEXTURE2D(_Crest); SAMPLER(sampler_Crest);
            CBUFFER_START(UnityPerMaterial)
                float4 _CrestData;
                float _WetStone;
            CBUFFER_END
            float4 _CuoiTerrainLight;
            float4 _CuoiVillageLights[4];

            struct Attributes
            {
                float3 pos : POSITION;
                float2 coord : TEXCOORD0;
                float2 distanceToSides : TEXCOORD1;
                float2 geology : TEXCOORD2;
                half4 exposed : COLOR;
            };
            struct Varyings
            {
                float4 pos : SV_POSITION;
                float2 coord : TEXCOORD0;
                float2 world : TEXCOORD1;
                float2 side : TEXCOORD2;
                float geology : TEXCOORD3;
                half3 exposed : COLOR;
            };
            Varyings Vert(Attributes v)
            {
                Varyings o;
                float3 world = TransformObjectToWorld(v.pos);
                o.pos = TransformWorldToHClip(world);
                o.world = world.xy;
                o.coord = v.coord;
                o.side = v.distanceToSides;
                o.geology = v.geology.x;
                o.exposed = v.exposed.rgb;
                return o;
            }
            float StoneU(float x, float depth)
            {
                float drift = sin(x * .22 + .8 * _CrestData.w) * .044
                            + sin(x * .067 - depth * .4) * .054;
                return 1 - abs(frac((x / 12.9 + drift + depth * .012) * .5) * 2 - 1);
            }
            half3 Stratum(float x, float slot, float bedDepth)
            {
                // Different outcrops at each depth, at the original rock's pixel scale.
                float seed = slot + _CrestData.w * 13;
                float jitterX = frac(sin(seed * 27.53 + 1.37) * 435.17);
                float jitterY = frac(sin(seed * 41.91 + 4.58) * 783.94);
                float u = StoneU(x + (jitterX - .5) * 13.8, bedDepth + slot);
                float v = .67 + .13 * jitterY - bedDepth * .181;
                return SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,float2(u,v)).rgb;
            }
            half3 Pigment(float2 p)
            {
                float depth = abs(p.y);
                float seam = 2.82 + .19 * sin(p.x * .39 + _CrestData.w);
                float u = StoneU(p.x,depth);
                half3 top = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,float2(u,.913 - depth * .187)).rgb;
                if (depth < seam) return top;
                float slot = floor((depth - seam) / 2.55);
                float along = depth - seam - slot * 2.55;
                half3 rock = Stratum(p.x,slot,along);
                if (along >= .55) return rock;
                half3 preceding = slot < .5 ? top : Stratum(p.x,slot-1,along+2.55);
                return lerp(preceding,rock,smoothstep(0,.55,along));
            }
            half Luma(half3 c) { return dot(c,half3(.24,.56,.20)); }

            half4 Frag(Varyings i) : SV_Target
            {
                float2 at = floor(i.coord * 48 + .5) / 48;
                float x = at.x, d = at.y;
                float crestRelief = .043 * sin(x * .66 + _CrestData.w)
                                  + .024 * sin(x * 1.73 + .4) + .010 * sin(x * 4.3);
                float relativeDepth = d + crestRelief;
                float atlasRow = _CrestData.x + relativeDepth * _CrestData.y;
                float crestU = frac(x / 11.9 + .036 * sin(x * .40) + .108 * sin(x * .067));
                half4 crest = SAMPLE_TEXTURE2D(_Crest,sampler_Crest,float2(crestU, 1 - atlasRow / _CrestData.z));
                // Only the selected biome's strip can decorate this lip.
                crest.a *= step(-.75,relativeDepth) * (1 - step(.97,relativeDepth));
                float border = min(lerp(2.0,i.side.x,i.exposed.r),lerp(2.0,i.side.y,i.exposed.g));
                if (relativeDepth < -.1) crest.a *= smoothstep(.015,.15,border);
                half opacity = max(crest.a,step(.025,relativeDepth));
                clip(opacity - .14);

                float2 stoneAt = float2(x, floor(i.geology * 48 + .5) / 48);
                half3 base = Pigment(stoneAt);
                half lum = Luma(base);
                float2 slope = float2(Luma(Pigment(stoneAt+float2(.045,0)))-lum,lum-Luma(Pigment(stoneAt+float2(0,.045))));
                float tread = (1 - smoothstep(.06,.34,relativeDepth)) * crest.a;
                float3 normal = normalize(float3(-slope.x * 3.7,lerp(-slope.y * 3.5,.85,tread),.95));
                float2 delta = _CuoiTerrainLight.xy - i.world;
                float radius = lerp(4.7,8.0,_CuoiTerrainLight.z);
                float falloff = pow(saturate(1-length(delta)/radius),1.6);
                float lambert = saturate(dot(normal,normalize(float3(delta,1.25))));
                half3 moon = lerp(half3(.66,.75,.89),half3(.80,.87,.99),saturate(dot(normal,normalize(float3(-.6,.9,.9)))));
                half3 amber = half3(.90,.52,.19) * (lambert*.91+.16) * falloff * (1+.4*_CuoiTerrainLight.z);
                [unroll] for (int index=0; index<4; index++)
                {
                    float4 lamp = _CuoiVillageLights[index];
                    float2 ray = lamp.xy - i.world;
                    float local = saturate(1-length(ray)/max(.01,lamp.z));
                    amber += half3(.63,.36,.14) * local * local * lamp.w * saturate(dot(normal,normalize(float3(ray,1.2))));
                }

                float undercut = .17 * smoothstep(.12,.36,relativeDepth) * (1-smoothstep(.46,.96,relativeDepth));
                float chip = .16 + .085*sin(floor(d*5)*1.21 + x*.13);
                float sideFace = (1 - smoothstep(chip,chip + .23,border)) * smoothstep(.04,.25,d);
                half3 sideStone = Pigment(stoneAt + float2(1.27 + .31*sin(d*1.8), .13));
                half3 pigment = lerp(lerp(base,sideStone,sideFace*.7),crest.rgb,crest.a);
                float leftLight = (1 - smoothstep(.018,.083,i.side.x)) * i.exposed.r * smoothstep(.08,.32,d);
                half3 color = pigment * (moon + amber) * (1 - undercut) * (1 - sideFace * .27);
                color += leftLight * half3(.055,.070,.082) * (.25 + lum);
                color += _WetStone * pow(lambert,12) * falloff * half3(.15,.087,.038);
                color *= 1 - .58 * smoothstep(2.4,10.5,i.geology);
                return half4(color,opacity);
            }
            ENDHLSL
        }
    }
}
