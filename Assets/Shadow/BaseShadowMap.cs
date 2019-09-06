using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseShadowMap : MonoBehaviour
{
	private Camera LightCamera;

	public Vector3[] Corners;



	

	void Awake()
	{
	}

	void Start () 
	{
        //Light light = GetComponent<Light>();
        //LightCamera = GetComponentInChildren<Camera>();
        //Matrix4x4 a = Utils.CalcBaseShadowMapMatrix(light, LightCamera, Camera.main, false);
        //Matrix4x4 b = GetLightProjectMatrix(LightCamera);
        CaptureDepth cd = GetComponentInChildren<CaptureDepth>();
        Shader.SetGlobalMatrix ("_LightProjection", cd.lightProjecionMatrix);
	}


	Matrix4x4 GetLightProjectMatrix(Camera lightCam)
	{
        Matrix4x4 worldToView = lightCam.worldToCameraMatrix;
        Matrix4x4 projection  = GL.GetGPUProjectionMatrix(lightCam.projectionMatrix, false);

 		return   projection * worldToView;
	}

}
