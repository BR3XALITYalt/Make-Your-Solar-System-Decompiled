Shader "Custom/Circle" {
Properties {
 _Color ("Color", Color) = (1,0,0,0)
 _Thickness ("Thickness", Range(0,0.5)) = 0.05
 _Radius ("Radius", Range(0,0.5)) = 0.4
 _Dropoff ("Dropoff", Range(0.01,4)) = 0.1
}
	//DummyShaderTextExporter
	
	SubShader{
		Tags { "RenderType" = "Opaque" }
		LOD 200
		CGPROGRAM
#pragma surface surf Lambert
#pragma target 3.0
		sampler2D _MainTex;
		struct Input
		{
			float2 uv_MainTex;
		};
		void surf(Input IN, inout SurfaceOutput o)
		{
			float4 c = tex2D(_MainTex, IN.uv_MainTex);
			o.Albedo = c.rgb;
		}
		ENDCG
	}
}