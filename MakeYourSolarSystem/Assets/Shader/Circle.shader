Shader "Custom/Circle"
{
    Properties
    {
        [Gamma] _Color     ("Color",     Color)          = (1, 0, 0, 0)
        _Thickness         ("Thickness", Range(0, 0.5))  = 0.05
        _Radius            ("Radius",    Range(0, 0.5))  = 0.4
        _Dropoff           ("Dropoff",   Range(0.01, 4)) = 0.1
    }

    SubShader
    {
        Tags { "Queue" = "Transparent" "IgnoreProjector" = "true" "RenderType" = "Transparent" }

        Pass
        {
            Tags { "Queue" = "Transparent" "IgnoreProjector" = "true" "RenderType" = "Transparent" }

            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _Color;
            float  _Thickness;
            float  _Radius;
            float  _Dropoff;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv  : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv  = v.uv - 0.5;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float band = _Thickness * _Dropoff;
                float d    = length(i.uv);
                float diff = d - _Radius;

                float offset_inner = diff - _Thickness * 0.5;
                float offset_outer = diff + _Thickness * 0.5;
                float alpha_outer  = -(offset_outer * offset_outer) / (band * band) + 1.0;
                float alpha_inner  = -(offset_inner * offset_inner) / (band * band) + 1.0;

                float alpha = 1.0;

                if ((_Radius + _Thickness * 0.5) < d)
                    alpha = alpha_outer;

                if (d < (_Radius - _Thickness * 0.5))
                    alpha = alpha_inner;

                return fixed4(_Color.rgb, _Color.a * alpha);
            }
            ENDCG
        }
    }
}
