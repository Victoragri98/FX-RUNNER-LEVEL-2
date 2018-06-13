// Upgrade NOTE: upgraded instancing buffer 'Props' to new syntax.

Shader "Curved/Standard (Specular setup)"
{
	Properties
	{
		[Toggle(_ALPHATEST_ON)] _AlphaTest ("Alpha test?", Int) = 0
		_Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Cutoff("Alpha Cutoff", Range(0,1)) = 0.5
		_Specular ("Specular", Color) = (0,0,0,0)
        _SpecGlossMap ("Specular", 2D) = "white" {}
		_Glossiness("Smoothness", Range(0,1)) = 0.5
        [Normal] _BumpMap ("Normal", 2D) = "bump" {}
		_Emission("Emission", Range(0,1)) = 0
		_EmissionMap("Emission", 2D) = "white" {}
	}
	
	SubShader
	{
		Tags { "Queue"="Geometry" "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf StandardSpecular vertex:vert fullforwardshadows addshadow
		#pragma target 3.0
		#pragma shader_feature _ _ALPHATEST_ON
		
		// Global Shader values
		uniform float2 _BendAmount;
		uniform float3 _BendOrigin;
		uniform float _BendFalloff;
		
		sampler2D _MainTex;
		sampler2D _SpecGlossMap;
		sampler2D _BumpMap;
		sampler2D _EmissionMap;

		fixed4 _Color;
		float _Cutoff;
		fixed4 _Specular;
		float _Glossiness;
		fixed _Emission;
		
		struct Input
		{
			float2 uv_MainTex;
		};
		
		UNITY_INSTANCING_BUFFER_START(Props)
		UNITY_INSTANCING_BUFFER_END(Props)
		
		float4 Curve(float4 v)
		{
			//HACK: Considerably reduce amount of Bend
			_BendAmount *= .0001;

			float4 world = mul(unity_ObjectToWorld, v);

			float dist = length(world.xz-_BendOrigin.xz);

			dist = max(0, dist-_BendFalloff);

			// Distance squared
			dist = dist*dist;

			world.xy += dist*_BendAmount;
			
			return mul(unity_WorldToObject, world);
		}
		
		void vert(inout appdata_full v)
		{
			v.vertex = Curve(v.vertex);
		}
		
		void surf (Input IN, inout SurfaceOutputStandardSpecular o)
		{
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			
			#if defined(_ALPHATEST_ON)
				clip (c.a - _Cutoff);
			#endif
			
			fixed4 specgloss = tex2D(_SpecGlossMap, IN.uv_MainTex);
			
			o.Albedo = c.rgb;
			o.Specular = specgloss.rgb * _Specular.rgb;
			o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_MainTex));
			o.Smoothness = specgloss.a * _Glossiness;
			o.Alpha = c.a;
			o.Emission = tex2D(_EmissionMap, IN.uv_MainTex).rgb * _Emission;
		}
		ENDCG
	}
	
	FallBack "StandardSpecular"
	// CustomEditor "StandardShaderGUI"
}
