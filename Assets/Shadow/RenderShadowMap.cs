using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderShadowMap : MonoBehaviour {
	public Shader shadowMapshader;
	private Material shadowMapMaterial;
	public RenderTexture depthTexture;

	void Awake()
	{
		shadowMapMaterial = new Material(shadowMapshader);
		shadowMapMaterial.SetTexture("_LightDepthTex", depthTexture);
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	
	void OnRenderImage(RenderTexture src,RenderTexture dest)
	{
		Graphics.Blit(src, dest, shadowMapMaterial);
	}
}
