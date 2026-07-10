// ============================================================
//  Hidden/TrailStamp  —  شيدر التراكم (ping-pong)
//  لا يُسند يدوياً لأي مادة. SurfaceTrailPainter.cs يلقاه بنفسه
//  عبر Shader.Find("Hidden/TrailStamp").
//
//  الفكرة:
//   - يقرأ بفر الفريم السابق (_MainTex) ويضربه في _DecayRate ليخبو تدريجياً.
//   - لو _StampActive مفعّل، يضيف ختم عند _StampUV:
//       * إما بقعة دائرية ناعمة (الوضع الافتراضي).
//       * أو شكل قدم من تكستشر _FootTex (لو _UseFootTex مفعّل)،
//         مع تدوير حسب اتجاه المشي (_StampAngle) وقلب يمين/يسار (_StampFlip).
//   - يخرج "الحرارة" (شدة الأثر) في كل القنوات RGBA.
//  العرض النهائي يتكفّل فيه Custom/TrailGlowOverlay.
// ============================================================
Shader "Hidden/TrailStamp"
{
    Properties
    {
        _MainTex     ("Prev Buffer", 2D)   = "black" {}
        _StampColor  ("Stamp Color", Color) = (1, 1, 1, 1)
        _StampUV     ("Stamp UV", Vector)   = (0, 0, 0, 0)
        _StampRadius ("Stamp Radius", Float) = 0.04
        _StampActive ("Stamp Active", Float) = 0
        _DecayRate   ("Decay Rate", Float)   = 0.992

        // --- وضع القدم ---
        _FootTex     ("Foot Mask", 2D)       = "black" {}
        _UseFootTex  ("Use Foot Tex", Float)  = 0
        _StampAngle  ("Stamp Angle", Float)   = 0     // راديان، اتجاه المشي
        _StampFlip   ("Stamp Flip", Float)    = 1     // +1 يمين / -1 يسار (قلب أفقي)
        _StampAspect ("Stamp Aspect", Float)  = 0.5   // عرض القدم ÷ طولها
    }
    SubShader
    {
        // شيدر تراكم على RenderTexture: لا عمق، لا حذف، استبدال كامل (بدون مزج).
        Cull Off  ZWrite Off  ZTest Always  Blend Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _FootTex;
            float4    _StampColor;
            float4    _StampUV;
            float     _StampRadius;
            float     _StampActive;
            float     _DecayRate;
            float     _UseFootTex;
            float     _StampAngle;
            float     _StampFlip;
            float     _StampAspect;

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f     { float2 uv : TEXCOORD0; float4 vertex : SV_POSITION; };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 1) الأثر القديم يخبو شوي كل فريم.
                fixed4 prev = tex2D(_MainTex, i.uv) * _DecayRate;

                // 2) أضف الختم الجديد (لو فيه خطوة هذا الفريم).
                if (_StampActive > 0.5)
                {
                    float stamp = 0.0;

                    if (_UseFootTex > 0.5)
                    {
                        // ندوّر الإحداثيات لفضاء القدم (toe يشير لاتجاه المشي).
                        float2 local = i.uv - _StampUV.xy;
                        float s = sin(_StampAngle);
                        float c = cos(_StampAngle);
                        float2 fs = float2(local.x * c + local.y * s,
                                          -local.x * s + local.y * c);

                        // قياس: نصف الطول = _StampRadius، العرض = طول × _StampAspect.
                        fs.x /= max(_StampRadius * _StampAspect, 1e-5);
                        fs.y /= max(_StampRadius, 1e-5);
                        fs.x *= _StampFlip; // قلب يمين/يسار

                        float2 fuv = fs * 0.5 + 0.5;
                        if (fuv.x >= 0.0 && fuv.x <= 1.0 && fuv.y >= 0.0 && fuv.y <= 1.0)
                            stamp = tex2D(_FootTex, fuv).r;
                    }
                    else
                    {
                        float d = distance(i.uv, _StampUV.xy);
                        stamp = 1.0 - smoothstep(0.0, _StampRadius, d);
                    }

                    // max = تراكم بدون تشبّع؛ الأثر الجديد يطغى على القديم في مركزه.
                    prev = max(prev, _StampColor * stamp);
                }

                return prev;
            }
            ENDCG
        }
    }
}
