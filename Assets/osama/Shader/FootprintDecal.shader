// ============================================================
//  Custom/FootprintDecal  —  شيدر طابع القدم المتوهّج
//  ينحط على مادة بريفاب الخطوة (Quad). الشفافية الكليّة يتحكم فيها
//  FootprintDecal.cs عبر _Alpha (للخبو مع العمر + الكشف بالفلاش).
//
//  افتراضياً يرسم شكل القدم *إجرائياً* داخل الشيدر (بدون تكستشر) —
//  فيطلع نظيف دايماً بغضّ النظر عن إعدادات استيراد أي صورة.
//  لو تبي تستخدم صورة قدم بدالها، طفّي Procedural Foot وحط الصورة في Foot Mask.
// ============================================================
Shader "Custom/FootprintDecal"
{
    Properties
    {
        _MainTex ("Foot Mask (optional)", 2D) = "white" {}
        [HDR] _Color ("Glow Color", Color) = (1.5, 0.35, 1.9, 1)
        _Alpha  ("Master Alpha", Range(0,1)) = 1
        _Cutoff ("Mask Cutoff", Range(0,1)) = 0.08
        [Toggle] _Procedural ("Procedural Foot (ignore texture)", Float) = 1
        [Toggle] _UseAlpha ("Use Alpha Channel (texture mode)", Float) = 1
        [Toggle] _Invert ("Invert Mask (dark foot on light bg)", Float) = 0
        _Softness ("Edge Softness", Range(0.01,0.4)) = 0.18
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
            float  _Procedural;
            float  _UseAlpha;
            float  _Invert;
            float  _Softness;
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

            // قطعة بيضاوية ناعمة عند مركز c بأنصاف أقطار r.
            float Ellipse (float2 p, float2 c, float2 r, float soft)
            {
                float d = length((p - c) / r);
                return 1.0 - smoothstep(1.0 - soft, 1.0, d);
            }

            // شكل القدم: كرة المقدّمة + الكعب + رابط + ٥ أصابع. (الأصابع نحو +Y = اتجاه المشي)
            float FootMask (float2 uv)
            {
                float2 p = uv - 0.5;        // -0.5 .. 0.5
                float s = _Softness;
                float m = 0.0;
                m = max(m, Ellipse(p, float2(0.0,  0.07), float2(0.22, 0.27), s)); // المقدّمة
                m = max(m, Ellipse(p, float2(0.0, -0.27), float2(0.15, 0.18), s)); // الكعب
                m = max(m, Ellipse(p, float2(0.0, -0.10), float2(0.11, 0.22), s)); // الرابط
                m = max(m, Ellipse(p, float2(-0.17, 0.35), float2(0.055, 0.065), s)); // الإبهام
                m = max(m, Ellipse(p, float2(-0.06, 0.40), float2(0.042, 0.052), s));
                m = max(m, Ellipse(p, float2( 0.03, 0.41), float2(0.038, 0.048), s));
                m = max(m, Ellipse(p, float2( 0.11, 0.39), float2(0.032, 0.042), s));
                m = max(m, Ellipse(p, float2( 0.18, 0.35), float2(0.028, 0.036), s));
                return saturate(m);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float mask;
                if (_Procedural > 0.5)
                {
                    mask = FootMask(i.uv);
                }
                else
                {
                    fixed4 c = tex2D(_MainTex, i.uv);
                    float lum = max(c.r, max(c.g, c.b));
                    float texMask = lerp(lum, c.a, _UseAlpha);
                    mask = lerp(texMask, 1.0 - texMask, _Invert); // قلب لو القدم غامقة على خلفية فاتحة
                }

                if (mask < _Cutoff) discard;

                float pulse = 1.0 + _PulseAmount * sin(_Time.y * _PulseSpeed);
                float3 col = _Color.rgb * mask * pulse;
                float  a   = saturate(mask) * _Alpha * _Color.a;
                return fixed4(col, a);
            }
            ENDCG
        }
    }
}
