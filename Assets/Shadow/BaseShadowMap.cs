using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseShadowMap : MonoBehaviour
{
	private Camera LightCamera;

	public Vector3[] Corners;


	void Update()
	{
		CaptureDepth cd = GetComponentInChildren<CaptureDepth>();
        Shader.SetGlobalMatrix ("_LightViewProjMatrix", cd.lightViewProjMatrix);
	}

}
