using UnityEngine;
using System.Collections;

public class CaptureDepth : MonoBehaviour
{
	public RenderTexture depthTexture;
    public Matrix4x4 lightProjecionMatrix;


    private Camera lightCamera;
	private Shader mSampleDepthShader ;

	void Awake()
	{
		lightCamera = GetComponent<Camera>();
		Camera mainCam = Camera.main;
		Light mainLight = lightCamera.GetComponentInParent<Light>();
        lightProjecionMatrix = Utils.CalcBaseShadowMapMatrix(mainLight, lightCamera, mainCam);

        mSampleDepthShader = Shader.Find("ShadowMap/CaptureDepth");

		lightCamera.backgroundColor = Color.white;
		lightCamera.clearFlags = CameraClearFlags.Color; ;
		lightCamera.targetTexture = depthTexture;
		lightCamera.enabled = false;
        Shader.SetGlobalMatrix("_LightProjection", lightProjecionMatrix);
        Shader.SetGlobalTexture ("_LightDepthTex", depthTexture);
		lightCamera.RenderWithShader(mSampleDepthShader, "RenderType");
	
	}

	void Start()
	{
		//if (mCam != null) 
		{
			
		//	mCam.SetReplacementShader (mSampleDepthShader, "RenderType");		
		}
	}
}