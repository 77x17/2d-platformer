Shader "Moonlit/Story banyan"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite", 2D) = "white" {}
        _GuardianPose ("Gesture time and strength", Vector) = (0,1,0,0)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Cull Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "UnityCG.cginc"
            #include "BanyanPose.cginc"
            struct Input { float4 pos : POSITION; float2 uv : TEXCOORD0; half4 tint : COLOR; };
            struct Interpolator { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; half4 tint : COLOR; };
            sampler2D _MainTex;
            float4 _MainTex_TexelSize, _GuardianPose;
            Interpolator Vert(Input v)
            {
                Interpolator o;
                o.pos = UnityObjectToClipPos(v.pos);
                o.uv = v.uv;
                o.tint = v.tint;
                return o;
            }
            half4 Frag(Interpolator i) : SV_Target
            {
                float2 uv = BanyanPoseUV(i.uv, _GuardianPose.xy, abs(_MainTex_TexelSize.xy));
                return tex2D(_MainTex, uv) * i.tint;
            }
            ENDHLSL
        }
    }
}
