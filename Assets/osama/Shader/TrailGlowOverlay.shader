// ============================================================
//  Custom/TrailGlowOverlay  —  شيدر العرض المتوهّج
//  ينحط على مادة أوبجكت "الأوفرلي" (نسخة السطح المرفوعة قليلاً، بدون كولايدر).
//  يقرأ لوحة الحرارة (_TrailTex) اللي يغذّيها SurfaceTrailPainter
//  ويحوّلها لأثر قدم بنفسجي متوهّج: نواة حارقة + هالة + ارتعاش + نبض.
//
//  يعتمد على الـ alpha (مو إضافة لونية) عشان يبان واضح على أي سطح:
//  أرض، جدار، أو سقف — بنفس المادة.
//
//  ميزة "الكشف بالفلاش": لو فعّلها سكربت TrailRevealLight، الأثر يبان فقط
//  داخل مخروط الفلاش البنفسجي (لكل بكسل) — بقية الوقت يكون خافت/مخفي.
//  افتراضياً معطّلة (_RevealEnabled = 0) فالشيدر يشتغل عادي بدون السكربت.
// ============================================================
Shader "Custom/TrailGlowOverlay"
{
    Properties
    {
        _TrailTex      ("Trail (Heat)", 2D)   = "black" {}
        [HDR] _CoreColor ("Core Color", Color) = (1.6, 0.4, 1.7, 1)
        [HDR] _GlowColor ("Glow Color", Color) = (0.35, 0.03, 0.6, 1)
        _GlowIntensity ("Glow Intensity", Float) = 2.5
        _FlickerSpeed  ("Flicker Speed", Float)  = 9
        _Threshold     ("Edge Threshold", Float) = 0.02
        _Softness      ("Edge Softness", Float)   = 0.35

        // --- الكشف بالفلاش (يضبطها TrailRevealLight.cs) ---
        _RevealEnabled ("Reveal By Light", Float) = 0
        _AmbientReveal ("Ambient Reveal", Range(0,1)) = 0
        _LightPos      ("Light Pos", Vector)   = (0,0,0,0)
        _LightDir      ("Light Dir", Vector)   = (0,0,1,0)
        _LightRange    ("Light Range", Float)  = 12
        _LightCosAngle ("Light Cos HalfAngle", Float) = 0.92
        _LightOn       ("Light On", Float)     = 0
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

            sampler2D _TrailTex;
            float4 _CoreColor;
            float4 _GlowColor;
            float  _GlowIntensity;
            float  _FlickerSpeed;
            float  _Threshold;
            float  _Softness;

            float  _RevealEnabled;
            float  _AmbientReveal;
            float4 _LightPos;
            float4 _LightDir;
            float  _LightRange;
            float  _LightCosAngle;
            float  _LightOn;

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f
            {
                float2 uv   : TEXCOORD0;
                float3 wpos : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.wpos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            // كم هذا البكسل مكشوف بالفلاش (0 = مخفي، 1 = مكشوف بالكامل).
            float ComputeReveal (float3 wpos)
            {
                if (_RevealEnabled < 0.5) return 1.0;          // الميزة مطفية → عرض عادي.

                float3 toPix = wpos - _LightPos.xyz;
                float  dist  = length(toPix);
                float3 dir   = toPix / max(dist, 1e-4);

                float cosA   = dot(dir, normalize(_LightDir.xyz));
                float inCone = smoothstep(_LightCosAngle, _LightCosAngle + 0.06, cosA);
                float atten  = saturate(1.0 - dist / max(_LightRange, 1e-4));

                float lit = inCone * atten * _LightOn;
                return lerp(_AmbientReveal, 1.0, lit);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 t = tex2D(_TrailTex, i.uv);

                // الحرارة = أقوى قناة (يشتغل سواء كان الأثر أبيض أو ملوّن من الـStamp).
                float heat = max(t.r, max(t.g, t.b));
                if (heat < _Threshold) discard;

                // تدرّج من الهالة الغامقة (أطراف) إلى النواة الحارقة (المركز).
                float core = smoothstep(_Softness, 1.0, heat);
                float3 col = lerp(_GlowColor.rgb, _CoreColor.rgb, core);

                // ارتعاش غير منتظم (شمعة) + نبض بطيء (تنفّس).
                float flicker = 0.78 + 0.22 * sin(_Time.y * _FlickerSpeed)
                                              * sin(_Time.y * _FlickerSpeed * 0.37);
                float pulse   = 0.85 + 0.15 * sin(_Time.y * 1.7);

                col *= _GlowIntensity * flicker * pulse;

                // الشفافية تتبع الحرارة → يبان قوي في القلب ويذوب على الأطراف.
                float alpha = saturate(heat * 2.0) * flicker;

                // الكشف بالفلاش: يخفّت/يخفي الأثر خارج مخروط الضوء.
                float reveal = ComputeReveal(i.wpos);
                alpha *= reveal;
                if (alpha < 0.001) discard;

                return fixed4(col, alpha);
            }
            ENDCG
        }
    }
}
