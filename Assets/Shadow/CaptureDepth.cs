using UnityEngine;
using System.Collections;

public class CaptureDepth : MonoBehaviour
{
	public RenderTexture depthTexture;

	private Camera lightCamera;
	private Shader mSampleDepthShader ;

	void Awake()
	{
		Light l = this.GetComponentInParent<Light>();
		lightCamera = GetComponent<Camera>();
		Camera mainCam = Camera.main;
		//算出视锥体的5个顶点
		Vector3[] corners = Utils.GetCorners(mainCam);
		//转换到lightview空间
		Vector3 to = l.transform.position + l.transform.forward;
		Matrix4x4 mat = Matrix4x4.LookAt(l.transform.position, to, l.transform.up);
		for(int i = 0; i < corners.Length; i++)
		{
			Debug.Log(i+" ====="+corners[i]);
			
			corners[i] = mat * corners[i];
			//corners[i] = lightCamera.transform.TransformVector(corners[i] );
		}
		Vector3 max, min;
		Utils.GetAABB(corners, out min, out max);
		Vector3 center = (max + min) * 0.5f;
		center.z = max.z;
		center = mat.inverse * center;
		Debug.Log("AAAAAAAAAA"+center);
		l.transform.position = center;
		Vector3 boxSize = max - min;

		lightCamera.orthographicSize = boxSize.x/2.0f;
		//lightCamera.orthographicSize *= 2.0f;
		lightCamera.aspect = boxSize.x/boxSize.y;



		Light mainLight = lightCamera.GetComponentInParent<Light>();
		mSampleDepthShader = Shader.Find("ShadowMap/CaptureDepth");

		if (lightCamera != null) 
		{
			lightCamera.backgroundColor = Color.white;
			lightCamera.clearFlags = CameraClearFlags.Color; ;
			lightCamera.targetTexture = depthTexture;
			lightCamera.enabled = false;

			Shader.SetGlobalTexture ("_LightDepthTex", depthTexture);
			lightCamera.RenderWithShader(mSampleDepthShader, "RenderType");

		}
	}

	void Start()
	{
		//if (mCam != null) 
		{
			
		//	mCam.SetReplacementShader (mSampleDepthShader, "RenderType");		
		}
	}
}