// Upgrade NOTE: replaced tex2D unity_Lightmap with UNITY_SAMPLE_TEX2D

// Upgrade NOTE: replaced tex2D unity_Lightmap with UNITY_SAMPLE_TEX2D

// Upgrade NOTE: replaced tex2D unity_Lightmap with UNITY_SAMPLE_TEX2D

Shader "Resplit/BaseLevel" {
	Properties{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0
	}
		SubShader{
			Tags { "RenderType" = "Opaque" }
			LOD 200

			CGPROGRAM
			#pragma surface surf Standard fullforwardshadows
			#pragma target 3.0

			sampler2D _MainTex;

			#include "UnityCG.cginc"

			struct Input {
				float2 uv_MainTex;
				half4 color : COLOR;
			};

			half _Glossiness;
			half _Metallic;
			fixed4 _Color;

			UNITY_INSTANCING_BUFFER_START(Props)
			UNITY_INSTANCING_BUFFER_END(Props)

			void surf(Input IN, inout SurfaceOutputStandard o) {
				fixed4 c     = tex2D(_MainTex, IN.uv_MainTex) * _Color;
				//fixed4 c = UNITY_SAMPLE_TEX2D(unity_Lightmap, IN.uv_unity_Lightmap);
				//c.rgb		*= DecodeLightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, IN.uv2_MainTex));
				o.Albedo     = c.rgb /** IN.color.rgb*/;
				o.Metallic   = 0;
				o.Smoothness = 0;
				o.Alpha      = 1.0f;
			}
			ENDCG
		}
			FallBack "Diffuse"
}