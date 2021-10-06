Shader "Custom/Outline" {
    Properties {
        _Width ("Width", Range(0, 10)) = 10.0
        _Hue ("Hue Shift", Range(0.0, 1.0)) = 0.0
        _Saturation ("Saturation Shift", Range(0.0, 1.0)) = 0.0
        _Value ("Value Shift", Range(0.0, 1.0)) = 0.0
        _NoiseTex ("Noise Tex", 2D) = "white" {}
        _NoiseFreq ("Noise Frequency", Float) = 10.0
        _NoiseAmpl ("Noise Amplitude", Float) = 0.01
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

            /// the outline hue shift
            float _Hue;

            /// the outline saturation shift
            float _Saturation;

            /// the outline value shift
            float _Value;

            /// the outline noise texture
            sampler2D _NoiseTex;

            /// the speed the outline wiggles
            float _NoiseFreq;

            /// the amount the outline wiggles
            float _NoiseAmpl;

            // -- types --
            struct VertIn {
                float4 pos : POSITION;
                float3 normal : NORMAL;
                float4 color : COLOR;
            };

            struct VertOut {
                float4 pos : SV_POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
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

                // get offset of the outline
                float2 offset = normalize(cNormal.xy); // along the normal
                offset /= _ScreenParams.xy;            // in pixels
                offset *= _Width;                      // by the outline width
                offset *= 2.0;
                offset *= cPos.w;                      // adjust for perspective

                // add the offset to the pos
                cPos.xy += offset;

                // get the shifted color for this vert
                float3 color = IntoHsv(i.color);
                color.x = frac(color.x + _Hue);
                color.y = frac(color.y + _Saturation);
                color.z = frac(color.z + _Value);
                color = IntoRgb(color);

                // build output
                VertOut o;
                o.pos = cPos;
                o.uv = i.pos.xy;
                o.color = float4(color, 0.0);

                return o;
            }

            float4 DrawFrag(VertOut o) : SV_TARGET {
                float2 uv;
                uv.x = frac(o.uv.x + cos(_Time * _NoiseFreq) * 0.5 * _NoiseAmpl);
                uv.y = frac(o.uv.y + sin(_Time * _NoiseFreq) * 0.5 * _NoiseAmpl);

                if (tex2D(_NoiseTex, uv).r > 0.5) {
                    discard;
                }

                return o.color;
            }

            // float4 DrawFrag(VertOut o) : SV_TARGET {
            //     return o.color;
            // }

            ENDCG
        }
    }
}