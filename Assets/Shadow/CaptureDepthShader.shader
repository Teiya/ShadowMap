Shader "ShadowMap/CaptureDepth"
{
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct v2f
			{
				float4 vertex : SV_POSITION;
			};

			float4x4 _LightViewProjMatrix;

			v2f vert (appdata_base v)
			{
				v2f o;
				float4 worldPos = mul(UNITY_MATRIX_M, v.vertex);
				o.vertex = mul(_LightViewProjMatrix, worldPos);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				return EncodeFloatRGBA(i.vertex.z) ;
			}
			ENDCG
		}
	}
}