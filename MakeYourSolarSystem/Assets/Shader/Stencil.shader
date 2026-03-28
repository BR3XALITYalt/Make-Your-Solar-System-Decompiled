Shader "Unlit/Stencil"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }

    SubShader
    {
        LOD 100
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }


        Pass
        {
            Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }

            Stencil
            {
                Comp   Equal
                Pass   IncrSat
                Fail   IncrSat
            }

            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return fixed4(0.0, 0.5, 1.0, 0.0);
            }
            ENDCG
        }

        Pass
        {
            Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }

            Stencil
            {
                Ref  1
                Comp Less
            }

            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return fixed4(0.0, 1.0, 0.0, 0.15);
            }
            ENDCG
        }


        Pass
        {
            Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }

            Stencil
            {
                Ref  2
                Comp Less
            }

            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return fixed4(0.2, 0.0, 0.2, 0.1);
            }
            ENDCG
        }


        Pass
        {
            Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }

            Stencil
            {
                Ref  3
                Comp Less
            }

            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return fixed4(0.12, 0.0, 0.0, 1.0);
            }
            ENDCG
        }

        Pass
        {
            Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }

            Stencil
            {
                Ref  4
                Comp Less
            }

            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return fixed4(0.12, 0.0, 0.0, 1.0);
            }
            ENDCG
        }
    }
}
