Shader "Custom/Outline" {
    Properties {
        _Width ("Outline width", Range(0, 10)) = 10.0
        _Hue ("Hue shift", Range(0.0, 1.0)) = 0.0
        _Saturation ("Saturation shift", Range(0.0, 1.0)) = 0.0
        _Value ("Value shift", Range(0.0, 1.0)) = 0.0
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
                float4 vPos : POSITION;
                float4 vColor : COLOR;
            };

            struct VertOut {
                float4 vPos : SV_POSITION;
                float4 vColor : COLOR;
                UNITY_FOG_COORDS(1)
            };

            // -- program --
            VertOut DrawVert(VertIn v) {
                VertOut o;
                o.vPos = UnityObjectToClipPos(v.vPos);
                o.vColor = v.vColor;
                UNITY_TRANSFER_FOG(o, o.vPos);
                return o;
            }

            fixed4 DrawFrag(VertOut i) : SV_Target {
                fixed4 c = i.vColor;
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

            // -- types --
            struct VertIn {
                float4 vPos : POSITION;
                float3 vNormal : NORMAL;
                float4 vColor : COLOR;
            };

            struct VertOut {
                float4 cPos : SV_POSITION;
                float4 fColor : COLOR;
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
                VertOut o;

                // offset the outline from the edge of the model in clip space
                o.cPos = UnityObjectToClipPos(i.vPos);

                // get clip space normal
                float3 cNormal = mul((float3x3)UNITY_MATRIX_VP, mul((float3x3)UNITY_MATRIX_M, i.vNormal));

                // get offset of the outline
                float2 offset = normalize(cNormal.xy); // along the normal
                offset /= _ScreenParams.xy;            // in pixels
                offset *= _Width;                      // by the outline width
                offset *= 2.0;
                offset *= o.cPos.w;                    // adjust for perspective

                // add the offset to the pos
                o.cPos.xy += offset;

                // get the shifted color for this vert
                float3 color = IntoHsv(i.vColor);
                color.x = frac(color.x + _Hue);
                color.y = frac(color.y + _Saturation);
                color.z = frac(color.z + _Value);

                // set the fragment color
                o.fColor = float4(IntoRgb(color), 0.0);

                return o;
            }

            float4 DrawFrag(VertOut o) : SV_TARGET {
                return o.fColor;
            }
            ENDCG
        }
    }
}