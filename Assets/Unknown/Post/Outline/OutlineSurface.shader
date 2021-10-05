Shader "Custom/OutlineSurface" {
    Properties {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0

        _OutlineColor ("Outline Color", Color) = (0, 0, 0, 1)
        _OutlineWidth ("Outline Width", Range(0, 10)) = 10.0
    }

    SubShader {
        Tags {
            "RenderType" = "Opaque"
        }

        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

        sampler2D _MainTex;
        struct Input {
            float2 uv_MainTex;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        UNITY_INSTANCING_BUFFER_START(Props)
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o) {
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG

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
                float4 position : POSITION;
                float3 normal : NORMAL;
            };

            struct VertOut {
                float4 vPos : SV_POSITION;
            };

            // -- program --
            // https://www.videopoetics.com/tutorials/pixel-perfect-outline-shaders-unity
            VertOut DrawVert(VertIn i) {
                VertOut o;

                // get clip space normal
                float3 cNormal = mul((float3x3)UNITY_MATRIX_VP, mul((float3x3)UNITY_MATRIX_M, i.normal));

                // get clip space pos
                o.vPos = UnityObjectToClipPos(i.position);

                // get offset of the outline
                float2 offset = normalize(cNormal.xy); // along the normal
                offset /= _ScreenParams.xy;            // in pixels
                offset *= _OutlineWidth;               // by the outline width
                offset *= 2.0;
                offset *= o.vPos.w;                    // adjust for perspective

                // add the offset to the pos
                o.vPos.xy += offset;

                return o;
            }

            half4 DrawFrag() : SV_TARGET {
                return _OutlineColor;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
