// ============================================================
//  Custom/FootprintDecal  —  شيدر طابع القدم المتوهّج
//  ينحط على مادة بريفاب الخطوة (Quad). يعرض صورة القدم (_MainTex)
//  كقدم بنفسجية متوهّجة. الشفافية الكليّة يتحكم فيها FootprintDecal.cs
//  عبر _Alpha (للخبو مع العمر + الكشف بالفلاش).
// ============================================================
Shader "Custom/FootprintDecal"
{
    Properties
    {
        _MainTex ("Foot Mask", 2D) = "white" {}
        [HDR] _Color ("Glow Color", Color) = (1.4, 0.3, 1.7, 1)
        _Alpha      ("Master Alpha", Range(0,1)) = 1
        _Cutoff     ("Mask Cutoff", Range(0,1)) = 0.15
        [Toggle] _UseAlpha ("Use Alpha Channel (1=transparent tex, 0=white-on-black)", Float) = 1
        _PulseSpeed ("Pulse Speed", Float) = 3
        _PulseAmount("Pulse Amount", Range(0,1)) = 0.2
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float  _Alpha;
            float  _Cutoff;
            float  _UseAlpha;
            float  _PulseSpeed;
            float  _PulseAmount;

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f     { float2 uv : TEXCOORD0; float4 vertex : SV_POSITION; };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, i.uv);
                // القناع: إما من قناة الـAlpha (تكستشر شفاف، أي لون للقدم)،
                // أو من سطوع الـRGB (قدم بيضاء على خلفية سوداء). يتحكم فيه _UseAlpha.
                float lum  = max(c.r, max(c.g, c.b));
                float mask = lerp(lum, c.a, _UseAlpha);
                if (mask < _Cutoff) discard;

                // وميض توهّج خفيف عشان "تلمع".
                float pulse = 1.0 + _PulseAmount * sin(_Time.y * _PulseSpeed);

                float3 col = _Color.rgb * mask * pulse;
                float  a   = mask * _Alpha * _Color.a;
                return fixed4(col, a);
            }
            ENDCG
        }
    }
}
