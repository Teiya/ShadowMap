Shader "Hidden/RenderShadowMapShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_LightDepthTex("Texture", 2D) = "white"{}
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			sampler2D _MainTex;

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				// just invert the colors
				//col.rgb = 1 - col.rgb;
				return col;
			}
			ENDCG
		}

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.vertex /= o.vertex.w;
				o.vertex.xy = o.vertex * 0.5 + float2(0.5, -0.5);
				o.uv = v.uv;
				return o;
			}
			
			uniform sampler2D_float _LightDepthTex;
			sampler2D _MainTex;

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 depthRGBA = tex2D(_LightDepthTex,i.uv);
				float depth = DecodeFloatRGBA(depthRGBA);
           		return fixed4(depth,depth,depth,1);
			}
			ENDCG
		}
	}
}
