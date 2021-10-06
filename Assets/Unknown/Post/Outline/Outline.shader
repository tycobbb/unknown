Shader "Custom/Outline" {
    Properties {
        _Width ("Width", Range(0, 10)) = 10.0
        _WidthFreq ("Width Frequency", Range(0.0, 15.0)) = 10.0
        _WidthAmpl ("Width Amplitude", Range(0.0, 1.0)) = 0.01
        _Hue ("Hue Shift", Range(0.0, 1.0)) = 0.0
        _Saturation ("Saturation Shift", Range(0.0, 1.0)) = 0.0
        _Value ("Value Shift", Range(0.0, 1.0)) = 0.0
        _NoiseTex ("Noise Tex", 2D) = "white" {}
        _NoiseFreq ("Noise Frequency", Range(0.0, 15.0)) = 10.0
        _NoiseAmpl ("Noise Amplitude", Range(0.0, 1.0)) = 0.01
    }

    SubShader {
        Tags {
            "RenderType" = "Opaque"
        }

        LOD 100

        // -- vertex colors --
        Pass {
            CGPROGRAM
            // -- config --
            #pragma vertex DrawVert
            #pragma fragment DrawFrag
            #pragma multi_compile_fog

            // -- includes --
            #include "UnityCG.cginc"

            // -- types --
            struct VertIn {
                float4 pos : POSITION;
                float4 color : COLOR;
            };

            struct VertOut {
                float4 pos : SV_POSITION;
                float4 color : COLOR;
                UNITY_FOG_COORDS(1)
            };

            // -- program --
            VertOut DrawVert(VertIn v) {
                VertOut o;
                o.pos = UnityObjectToClipPos(v.pos);
                o.color = v.color;
                UNITY_TRANSFER_FOG(o, o.pos);
                return o;
            }

            fixed4 DrawFrag(VertOut i) : SV_Target {
                fixed4 c = i.color;
                UNITY_APPLY_FOG(i.fogCoord, c);
                return c;
            }
            ENDCG
        }

        // -- outline --
        Pass {
            // -- flags --
            // only draw verts that aren't facing the camera / obscured
            Cull Front

            CGPROGRAM
            // -- config --
            #pragma vertex DrawVert
            #pragma fragment DrawFrag

            // -- props --
            /// the outline width
            half _Width;

            /// the speed the outline wobbles
            float _WidthFreq;

            /// the amount the outline wobbles
            float _WidthAmpl;

            /// the outline hue shift
            float _Hue;

            /// the outline saturation shift
            float _Saturation;

            /// the outline value shift
            float _Value;

            /// the outline noise texture
            sampler2D _NoiseTex;

            /// the speed the outline tex shifts
            float _NoiseFreq;

            /// the amount the outline tex shifts
            float _NoiseAmpl;

            // -- types --
            struct VertIn {
                float4 pos : POSITION;
                float3 normal : NORMAL;
                float4 color : COLOR;
            };

            struct VertOut {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 hsv : TEXCOORD1;
            };

            // -- helpers --
            /// convert rgb color into hsv
            /// see: https://stackoverflow.com/questions/15095909/from-rgb-to-hsv-in-opengl-glsl
            float3 IntoHsv(float3 c) {
                float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
                float4 p = lerp(float4(c.bg, K.wz), float4(c.gb, K.xy), step(c.b, c.g));
                float4 q = lerp(float4(p.xyw, c.r), float4(c.r, p.yzx), step(p.x, c.r));

                float d = q.x - min(q.w, q.y);
                float e = 1.0e-10;
                return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
            }

            /// convert hsv color into rgb
            /// https://stackoverflow.com/questions/15095909/from-rgb-to-hsv-in-opengl-glsl
            float3 IntoRgb(float3 c) {
                float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
                float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
                return c.z * lerp(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
            }

            // -- program --
            VertOut DrawVert(VertIn i) {
                // offset the outline from the edge of the model in clip space
                float4 cPos = UnityObjectToClipPos(i.pos);

                // get clip space normal
                float3 cNormal = mul((float3x3)UNITY_MATRIX_VP, mul((float3x3)UNITY_MATRIX_M, i.normal));

                // get the offset wobble
                float wobble = sin((i.pos.x + _Time) * _WidthFreq) * _WidthAmpl;

                // get offset of the outline
                float2 offset = normalize(cNormal.xy); // along the normal
                offset /= _ScreenParams.xy;            // in pixels
                offset *= _Width + wobble;             // by the outline width & wobble
                offset *= 2.0;
                offset *= cPos.w;                      // adjust for perspective

                // add the offset to the pos
                cPos.xy += offset;

                // shift the color for this vert
                float3 hsv = IntoHsv(i.color);
                hsv.x = frac(hsv.x + _Hue);
                hsv.y = frac(hsv.y + _Saturation);
                hsv.z = frac(hsv.z + _Value);

                // build output
                VertOut o;
                o.pos = cPos;
                o.uv = i.pos.xy;
                o.hsv = hsv;

                return o;
            }

            float4 DrawFrag(VertOut o) : SV_TARGET {
                float2 uv;

                // sample a wobbling blend value from the noise texture
                uv.x = frac(o.uv.x + cos(_Time * _NoiseFreq) * 0.5 * _NoiseAmpl);
                uv.y = frac(o.uv.y + sin(_Time * _NoiseFreq) * 0.5 * _NoiseAmpl);

                // if the blend is above the threshold, discard it
                const float blend = tex2D(_NoiseTex, uv).r;
                if (blend > 0.5) {
                    discard;
                }

                // produce an rgb color
                return float4(IntoRgb(o.hsv), 1.0);
            }
            ENDCG
        }
    }
}