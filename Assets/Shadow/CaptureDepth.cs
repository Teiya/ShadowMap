using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CaptureDepth : MonoBehaviour
{
	public RenderTexture depthTexture;
    public Matrix4x4 lightViewProjMatrix;


    private Camera lightCamera;
	private Shader mSampleDepthShader ;

	private int sampleCnt = 0;
	private bool needRefresh = false;

	

	void Awake()
	{
		lightCamera = GetComponent<Camera>();
		Camera mainCam = Camera.main;
		Light mainLight = lightCamera.GetComponentInParent<Light>();
        lightViewProjMatrix = Utils.CalcBaseShadowMapMatrix(mainLight, lightCamera, mainCam);

        mSampleDepthShader = Shader.Find("ShadowMap/CaptureDepth");
		lightCamera.backgroundColor = Color.magenta;
		lightCamera.clearFlags = CameraClearFlags.Color; ;
		lightCamera.targetTexture = depthTexture;
		lightCamera.enabled = false;
        Shader.SetGlobalMatrix("_LightViewProjMatrix", lightViewProjMatrix);
        Shader.SetGlobalTexture ("_LightDepthTex", depthTexture);
		lightCamera.RenderWithShader(mSampleDepthShader, "RenderType");
	
	}

	void Start()
	{

	}

	void OnGUI()  
    {  
        if (Input.GetKeyUp(KeyCode.Space) && !needRefresh)  
        {  
			needRefresh = true;
			sampleCnt++;
			if(sampleCnt >= 3)
			{
				sampleCnt = 0;
			}
        }  
    }


	void Update()
	{
		if(needRefresh)
		{
			lightCamera = GetComponent<Camera>();
			Camera mainCam = Camera.main;
			Light mainLight = lightCamera.GetComponentInParent<Light>();
			Text text = GameObject.Find("Canvas/Text").GetComponent<Text>();
			switch(sampleCnt)
			{
				case 0:
					text.text = "Basic";
					lightViewProjMatrix = Utils.CalcBaseShadowMapMatrix(mainLight, lightCamera, mainCam);
					break;
				case 1:
					text.text = "PSM";
					lightViewProjMatrix = Utils.CalcPersPectiveShadowMapMatrix(mainLight, lightCamera, mainCam);
					break;
				case 2:
					text.text = "LiSPSM";
					lightViewProjMatrix = Utils.CalcLightSpaceShadowMapMatrix(mainLight, lightCamera, mainCam);
					break;
				default:
					break;
			}
			mSampleDepthShader = Shader.Find("ShadowMap/CaptureDepth");
			Shader.SetGlobalMatrix("_LightViewProjMatrix", lightViewProjMatrix);
			Shader.SetGlobalTexture ("_LightDepthTex", depthTexture);
			lightCamera.RenderWithShader(mSampleDepthShader, "RenderType");

			needRefresh = false;
		}
	}
}