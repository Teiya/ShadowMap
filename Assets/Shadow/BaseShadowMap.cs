using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseShadowMap : MonoBehaviour
{
	private Camera LightCamera;

	public Vector3[] Corners;



	

	void Awake()
	{
		LightCamera = gameObject.GetComponentInChildren<Camera> ();
	}

	void Start () 
	{
		Matrix4x4 lightProjecionMatrix = GetLightProjectMatrix (LightCamera);
		Shader.SetGlobalMatrix ("_LightProjection", lightProjecionMatrix);
	}


	Matrix4x4 GetLightProjectMatrix(Camera lightCam)
	{
        Matrix4x4 posToUV = new Matrix4x4();
        posToUV.SetRow(0, new Vector4(0.5f,    0,    0, 0.5f));
        posToUV.SetRow(1, new Vector4(   0, 0.5f,    0, 0.5f));
        posToUV.SetRow(2, new Vector4(   0,    0,    1,    0));
        posToUV.SetRow(3, new Vector4(   0,    0,    0,    1));


        Matrix4x4 worldToView = lightCam.worldToCameraMatrix;
        Matrix4x4 projection  = GL.GetGPUProjectionMatrix(lightCam.projectionMatrix, false);

 		return   projection * worldToView;
	}

}
