Shader "Custom/GhostGlitch"
{
    // شيدر سيلويت أسود مع تأثير غلايتش (تشويش رأسي + ارتجاف/قطع لحظي + تلوين خفيف)
    // يصمم لـ URP. يضاف على ماتيريال جديد، والماتيريال يطبق على Mesh الوحش.

    Properties
    {
        _Color ("اللون الأساسي (أسود غالباً)", Color) = (0,0,0,1)

        [Header(Vertex Glitch)]
        _GlitchAmount ("قوة تشويش الفيرتكس", Range(0, 1)) = 0.05
        _GlitchSpeed ("سرعة تغيّر التشويش", Range(0.1, 60)) = 12
        _GlitchChance ("احتمالية حدوث قفزة تشويش", Range(0, 1)) = 0.15

        [Header(Flicker Cutout)]
        _FlickerSpeed ("سرعة الارتجاف", Range(0.1, 60)) = 18
        _FlickerChance ("احتمالية الاختفاء اللحظي", Range(0, 1)) = 0.08

        [Header(Color Glitch Tint)]
        _TintAmount ("قوة التلوين وقت الغلايتش", Range(0, 1)) = 0.25
        _TintColorA ("تلوين 1", Color) = (1,0,0.1,1)
        _TintColorB ("تلوين 2", Color) = (0,0.6,1,1)

        [Header(Master Control)]
        _GlitchIntensity ("شدة الغلايتش الكلية (يتحكم بيها سكربت)", Range(0, 1)) = 0.3
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }
        Cull Back

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS  : TEXCOORD0;
                float  glitchSeed  : TEXCOORD1;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float _GlitchAmount;
                float _GlitchSpeed;
                float _GlitchChance;
                float _FlickerSpeed;
                float _FlickerChance;
                float _TintAmount;
                float4 _TintColorA;
                float4 _TintColorB;
                float _GlitchIntensity;
            CBUFFER_END

            // هاش بسيط (0..1) من قيمة float، يستخدم بدل Random حقيقي
            float Hash1(float n)
            {
                return frac(sin(n) * 43758.5453123);
            }

            float3 HashOffset(float3 seedPos, float t)
            {
                float n = dot(seedPos, float3(12.9898, 78.233, 37.719)) + t;
                float x = Hash1(n) - 0.5;
                float y = Hash1(n * 1.37 + 1.7) - 0.5;
                float z = Hash1(n * 2.13 + 3.1) - 0.5;
                return float3(x, y, z);
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                // وقت "مقطّع" (Stepped) عشان التشويش يصير قفزات مفاجئة مثل الداتاموش، مو حركة سلسة
                float steppedTime = floor(_Time.y * _GlitchSpeed);

                float3 positionWS = TransformObjectToWorld(IN.positionOS.xyz);

                float trigger = Hash1(steppedTime * 7.13);
                float glitchActive = step(1.0 - (_GlitchChance * max(_GlitchIntensity, 0.05)), trigger);

                float3 jitter = HashOffset(positionWS, steppedTime) * _GlitchAmount * glitchActive * (0.5 + _GlitchIntensity);

                positionWS += jitter;

                OUT.positionWS = positionWS;
                OUT.positionHCS = TransformWorldToHClip(positionWS);
                OUT.glitchSeed = steppedTime;

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float steppedTimeFlicker = floor(_Time.y * _FlickerSpeed);
                float flickerNoise = Hash1(steppedTimeFlicker * 3.71 + IN.glitchSeed * 0.01);

                // قطع لحظي (الوحش يختفي فجأة لجزء من الثانية) - يعتمد على شدة الغلايتش العامة
                float flickerActive = step(1.0 - (_FlickerChance * max(_GlitchIntensity, 0.05)), flickerNoise);
                clip(flickerActive > 0.5 ? -1 : 1);

                // تلوين خفيف وقت لحظات الغلايتش (يحاكي انفصال ألوان بدائي بدون Grab Pass)
                float tintNoise = Hash1(steppedTimeFlicker * 5.21);
                float tintActive = step(1.0 - (_TintAmount * max(_GlitchIntensity, 0.05)), tintNoise);

                half4 baseColor = _Color;

                half4 tintColor = (frac(steppedTimeFlicker * 0.5) < 0.5) ? _TintColorA : _TintColorB;
                baseColor = lerp(baseColor, tintColor, tintActive * 0.6);

                return baseColor;
            }
            ENDHLSL
        }
    }

    FallBack "Universal Render Pipeline/Unlit"
}
