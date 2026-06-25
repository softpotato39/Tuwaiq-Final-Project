Shader "Hidden/TrailStamp"
{
    Properties
    {
        _MainTex ("Previous", 2D) = "black" {}
        _StampUV ("Stamp UV", Vector) = (0,0,0,0)
        _StampRadius ("Stamp Radius", Float) = 0.05
        _StampColor ("Stamp Color", Color) = (0.55,0.2,0.85,1)
        _StampActive ("Stamp Active", Float) = 0
        _DecayRate ("Decay Rate", Float) = 0.995
    }
    SubShader
    {
        Pass
        {
            ZTest Always Cull Off ZWrite Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _StampUV;
            float _StampRadius;
            float4 _StampColor;
            float _StampActive;
            float _DecayRate;

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f { float2 uv : TEXCOORD0; float4 vertex : SV_POSITION; };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 prev = tex2D(_MainTex, i.uv) * _DecayRate;

                float dist = distance(i.uv, _StampUV.xy);
                float mask = _StampActive * (1.0 - smoothstep(_StampRadius * 0.5, _StampRadius, dist));

                fixed4 col = prev + mask * _StampColor;
                return saturate(col);
            }
            ENDCG
        }
    }
}