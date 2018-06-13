// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Simplified Additive Particle shader. Differences from regular Additive Particle one:
// - no Tint color
// - no Smooth particle support
// - no AlphaTest
// - no ColorMask

Shader "Curved/Particles/Additive" {
Properties {
	_MainTex ("Particle Texture", 2D) = "white" {}
}

Category {
	Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
	Blend SrcAlpha One
	Cull Off Lighting Off ZWrite Off Fog { Color (0,0,0,0) }
	
	BindChannels {
		Bind "Color", color
		Bind "Vertex", vertex
		Bind "TexCoord", texcoord
	}
	
	SubShader {
		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			
			// Global Shader values
			uniform float2 _BendAmount;
			uniform float3 _BendOrigin;
			uniform float _BendFalloff;
			
			sampler2D _MainTex;
			
			struct Input
			{
				float2 uv_MainTex;
			};
			
			struct appdata
            {
                float4 vertex : POSITION;
				fixed4 color : COLOR;
                float2 uv : TEXCOORD0;
            };
			
			struct v2f
            {
                float4 vertex : SV_POSITION;
				fixed4 color : COLOR;
                float2 uv : TEXCOORD0;
            };
			
			float4 _MainTex_ST;
			
			float4 Curve(float4 v)
			{
				_BendAmount *= .0001;
				float4 world = mul(unity_ObjectToWorld, v);
				float dist = length(world.xz-_BendOrigin.xz);
				dist = max(0, dist-_BendFalloff);
				world.xy += dist*dist*_BendAmount;
				
				return mul(unity_WorldToObject, world);
			}
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(Curve(v.vertex));
				o.color = v.color;
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				return 2 * i.color * tex2D(_MainTex, i.uv);
			}
			ENDCG
		}
	}
}
}