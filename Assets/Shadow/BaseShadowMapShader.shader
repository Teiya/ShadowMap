// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Shadow/BaseShadowMapShader"
{
    Properties
    {
		_MainTex ("Base (RGB)", 2D) = "white" {}
    }

	SubShader
	{
		Tags
		{
		 	"RenderType"="Opaque" 
	 	}
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex object_vert
			#pragma fragment object_frag
		
			#include "UnityCG.cginc"

            uniform half4 _MainTex_TexelSize;
            sampler2D _MainTex;

            sampler2D _LightDepthTex;

            float4x4 _LightViewProjMatrix;

			struct appdata
			{
				float4 vertex : POSITION;
				float4 worldPos: TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float4 worldPos: TEXCOORD0; 
			};
			
			v2f object_vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.worldPos = mul(UNITY_MATRIX_M, v.vertex);

				return o;
			}
			
			fixed4 object_frag (v2f i) : SV_Target
			{
				// convert to light camera space
				fixed4 lightClipPos = mul(_LightViewProjMatrix , i.worldPos);
			    lightClipPos.xyz = lightClipPos.xyz / lightClipPos.w ;
				float2 shadowUV = lightClipPos.xy * float2(0.5, -0.5) + float2(0.5, 0.5);

//				//get depth
				fixed4 depthRGBA = tex2D(_LightDepthTex, shadowUV);
				float depth = DecodeFloatRGBA(depthRGBA);


				if(lightClipPos.z + 0.005 < depth  )
				{
					return fixed4(0,0,0,1);
				}
				else
				{
					return fixed4(0.5,0.5,0.5,1);
				}
			}
			ENDCG
		}
	}

	FallBack "Diffuse"
}
