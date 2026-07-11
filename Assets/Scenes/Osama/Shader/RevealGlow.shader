// ============================================================
//  Custom/RevealGlow  —  صورة مخفية متوهّجة (كتابة / بصمة / رمز)
//  تنحط على مادة Quad (كتابة على جدار، بصمة على أرض...).
//  الشفافية الكليّة يتحكم فيها RevealUnderFlashlight.cs عبر _Alpha
//  (فتبان فقط داخل ضوء الفلاش).
//
//  مرن:
//   - Use Image Colors: استخدم ألوان الصورة نفسها (مثلاً كتابة دم حمراء)،
//     أو اصبغها بـ Glow Tint (لو الصورة مجرد قناع أبيض/شفاف).
//   - Alpha From Image: القناع من قناة الشفافية، أو من سطوع الصورة.
// ============================================================
Shader "Custom/RevealGlow"
{
    Properties
    {
        _MainTex ("Image", 2D) = "white" {}
        [HDR] _Color ("Glow Tint", Color) = (1.6, 0.15, 0.15, 1)   // أحمر دموي
        _Alpha ("Master Alpha", Range(0,1)) = 1
        [Toggle] _UseImageColor ("Use Image Colors", Float) = 0      // 0=اصبغ بالـTint، 1=ألوان الصورة
        [Toggle] _AlphaFromImage ("Alpha From Image Alpha", Float) = 1 // 1=قناة الشفافية، 0=سطوع
        [Toggle] _Invert ("Invert Mask", Float) = 0
        _Cutoff ("Cutoff", Range(0,1)) = 0.05
        _PulseSpeed ("Pulse Speed", Float) = 2
        _PulseAmount ("Pulse Amount", Range(0,1)) = 0.15
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
            float  _UseImageColor;
            float  _AlphaFromImage;
            float  _Invert;
            float  _Cutoff;
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

                float lum = max(c.r, max(c.g, c.b));
                float mask = lerp(lum, c.a, _AlphaFromImage);
                mask = lerp(mask, 1.0 - mask, _Invert);
                if (mask < _Cutoff) discard;

                float pulse = 1.0 + _PulseAmount * sin(_Time.y * _PulseSpeed);
                float3 rgb = lerp(_Color.rgb, c.rgb * _Color.rgb, _UseImageColor) * pulse;
                float  a   = saturate(mask) * _Alpha * _Color.a;
                return fixed4(rgb, a);
            }
            ENDCG
        }
    }
}
