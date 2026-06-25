Shader "Custom/TrailGlowOverlay"
{
    Properties
    {
        _TrailTex ("Trail", 2D) = "black" {}
        _GlowIntensity ("Glow Intensity", Float) = 2.5
        _FlickerSpeed ("Flicker Speed", Float) = 6
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _TrailTex;
            float _GlowIntensity;
            float _FlickerSpeed;

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
                fixed4 trail = tex2D(_TrailTex, i.uv);
                float flicker = 0.85 + 0.15 * sin(_Time.y * _FlickerSpeed);
                fixed4 col = trail * _GlowIntensity * flicker;
                col.a = trail.a;
                return col;
            }
            ENDCG
        }
    }
}