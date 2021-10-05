Shader "Custom/Outline" {
    Properties {
        _OutlineWidth ("Outline Width", Range(0, 10)) = 10.0
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
            half _OutlineWidth;
            float4 _OutlineWidthScale;
            half4 _OutlineColor;

            // -- types --
            struct VertIn {
                float4 vPos : POSITION;
                float3 vNormal : NORMAL;
                float4 vColor : COLOR;
            };

            struct VertOut {
                float4 cPos : SV_POSITION;
                float4 vColor : COLOR;
            };

            // -- program --
            // https://www.videopoetics.com/tutorials/pixel-perfect-outline-shaders-unity
            VertOut DrawVert(VertIn i) {
                VertOut o;
                o.cPos = UnityObjectToClipPos(i.vPos);
                o.vColor = i.vColor;

                // get clip space normal
                float3 cNormal = mul((float3x3)UNITY_MATRIX_VP, mul((float3x3)UNITY_MATRIX_M, i.vNormal));

                // get offset of the outline
                float2 offset = normalize(cNormal.xy); // along the normal
                offset /= _ScreenParams.xy;            // in pixels
                offset *= _OutlineWidth;               // by the outline width
                offset *= 2.0;
                offset *= o.cPos.w;                    // adjust for perspective

                // add the offset to the pos
                o.cPos.xy += offset;

                return o;
            }

            float4 DrawFrag(VertOut o) : SV_TARGET {
                return o.vColor + float4(0.3, 0.3, 0.3, 0.0);
            }
            ENDCG
        }
    }
}