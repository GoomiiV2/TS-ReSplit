// Upgrade NOTE: replaced tex2D unity_Lightmap with UNITY_SAMPLE_TEX2D

// Upgrade NOTE: upgraded instancing buffer 'Props' to new syntax.

Shader "Resplit/BaseLevelTransparent" {
	Properties{
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_ClampUVs("ClampUVs", Vector) = (0,0,0,0)
	}
		SubShader{
			Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
			Cull Off
			ZWrite on
			LOD 200

			CGPROGRAM
			#pragma surface surf Standard fullforwardshadows alpha:fade
			#pragma target 3.0

			sampler2D _MainTex;

			struct Input {
				float2 uv_MainTex;
				half4 color : COLOR;
			};

			half _Glossiness;
			half _Metallic;
			fixed4 _Color;

			uniform float _ClampUVsX;
			uniform float _ClampUVsY;

			UNITY_INSTANCING_BUFFER_START(Props)
			UNITY_INSTANCING_BUFFER_END(Props)

			void surf(Input IN, inout SurfaceOutputStandard o) {
				fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
				o.Albedo = c.rgb /** IN.color.rgb*/;
				o.Metallic = 0;
				o.Smoothness = 0;
				o.Alpha = c.a * 2.0f;

				//clip(c.a - 0.25f);

				// For decal clip out of range uvs
				clip(IN.uv_MainTex.x * _ClampUVsX);
				clip(IN.uv_MainTex.y * _ClampUVsY);

				clip(((IN.uv_MainTex.x * -1.0f) + 1.0f) * _ClampUVsX);
				clip(((IN.uv_MainTex.y * -1.0f) + 1.0f) * _ClampUVsY);
			}
			ENDCG
		}
			FallBack "Diffuse"
}