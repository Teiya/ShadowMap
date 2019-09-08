using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utils  
{
	static public Vector3[] GetCorners (Camera theCamera)
	{
		Vector3[] corners = new Vector3[ 8 ];
        float fov = theCamera.fieldOfView;
        float asp = theCamera.aspect;
        float yf = Mathf.Tan(fov/2 * Mathf.Deg2Rad);
        float xf = yf * asp;

        Matrix4x4 l2w = theCamera.transform.localToWorldMatrix;
        Vector3 f0 = l2w * new Vector3(-xf,-yf,1);
        Vector3 f1 = l2w * new Vector3(-xf, yf,1);
        Vector3 f2 = l2w * new Vector3( xf,-yf,1);
        Vector3 f3 = l2w * new Vector3( xf, yf,1);

        float fcp = theCamera.farClipPlane;
        float ncp = theCamera.nearClipPlane;
        Vector3 cpt = theCamera.transform.position;

        corners[0] = cpt + fcp * f0;
        corners[1] = cpt + fcp * f1;
        corners[2] = cpt + fcp * f2;
        corners[3] = cpt + fcp * f3;

        corners[4] = cpt + ncp * f0;
        corners[5] = cpt + ncp * f1;
        corners[6] = cpt + ncp * f2;
        corners[7] = cpt + ncp * f3;
		
		return corners;
	}

	static public void GetAABB(Vector3[] points, out Vector3 min, out Vector3 max)
	{
		min = points[0];
		max = points[points.Length-1];
		foreach(Vector3 point in points)
		{
			if(point.x < min.x)min.x = point.x;
			if(point.y < min.y)min.y = point.y;
			if(point.z < min.z)min.z = point.z;

			if(point.x > max.x)max.x = point.x;
			if(point.y > max.y)max.y = point.y;
			if(point.z > max.z)max.z = point.z;
		}
	}

    public static Matrix4x4 CreateLookAt(Vector3 cameraPosition, Vector3 cameraTarget, Vector3 cameraUpVector)
    {
        Matrix4x4 result = Matrix4x4.LookAt(cameraPosition, cameraTarget, cameraUpVector);
        result = Matrix4x4.TRS(cameraPosition, result.rotation, new Vector3(1.0f, 1.0f, -1.0f));
        result = Matrix4x4.Inverse(result);

        return result;
    }

	static public Matrix4x4 CalcBaseShadowMapMatrix(Light light, Camera lightCamera, Camera mainCamera, bool targetTexture = true)
	{
		//算出视锥体的5个顶点
		Vector3[] corners = Utils.GetCorners(mainCamera);
		//转换到lightview空间，计算AABB
		Matrix4x4 mat = lightCamera.worldToCameraMatrix;//Matrix4x4.LookAt(light.transform.position, to, light.transform.up);
        for (int i = 0; i < corners.Length; i++)
		{
			corners[i] = mat.MultiplyPoint(corners[i]);
		}
		Vector3 max = new Vector3();
		Vector3 min = new Vector3();
		Utils.GetAABB(corners, out min, out max);
		Vector3 center = (max + min) * 0.5f;
		center.z = max.z;
		center = mat.inverse.MultiplyPoint(center);
		light.transform.position = center;
		Vector3 boxSize = max - min;

        Vector3 lightDir = light.transform.forward;
        Vector3 lightTo = center + lightDir;
        Vector3 viewDir = Camera.main.transform.forward;
        Vector3 temp = Vector3.Cross(viewDir, lightDir);
        Vector3 lightUp = Vector3.Cross(temp, lightDir);
        lightUp.Normalize();
        Matrix4x4 lightView = CreateLookAt(center, lightTo, -light.transform.up);//这里up为负只是想翻转一下，和后面的阴影图做对比，实际结果不影响
		//Matrix4x4 lightProj = Matrix4x4.Ortho(-boxSize.x*0.5f, boxSize.x*0.5f, boxSize.y*0.5f, -boxSize.y*0.5f, 0.0f, 300.0f);

		lightCamera.orthographicSize = boxSize.y/2.0f;
		lightCamera.aspect = boxSize.x/boxSize.y;
        Matrix4x4 worldToView = lightCamera.worldToCameraMatrix;
        Matrix4x4 projection  = GL.GetGPUProjectionMatrix(lightCamera.projectionMatrix, targetTexture);

        return projection * lightView;
	}

    static public Matrix4x4 CalcPersPectiveShadowMapMatrix(Light light, Camera lightCamera, Camera mainCamera, bool targetTexture = true)
    {
        const float Z_BACK_OFF = 10.0f;//黑科技，往后拉一下效果更好
        Matrix4x4 projection = Matrix4x4.Perspective(Camera.main.fieldOfView, Camera.main.aspect, Camera.main.nearClipPlane+Z_BACK_OFF, Camera.main.farClipPlane+Z_BACK_OFF );//CreateGLUPerspective(Mathf.Deg2Rad * Camera.main.fieldOfView, Camera.main.aspect, Camera.main.nearClipPlane, Camera.main.farClipPlane);
        //projection = GL.GetGPUProjectionMatrix(projection, false);
        Matrix4x4 view = Camera.main.worldToCameraMatrix;
        view.m23 -= Z_BACK_OFF;
        Matrix4x4 viewProj = projection * view;
        Vector3 lightDir = -light.transform.forward;//注意这里是光照的反方向
        Vector4 persLight = new Vector4(lightDir.x, lightDir.y, lightDir.z, 0.0f);

        persLight = viewProj * persLight;//.MultiplyPoint(persLight);
        if(persLight.w < 0.0f)
        {
            persLight.z = - persLight.z;
            //todo 这里其实要再算一下回拉距离的
        }

        persLight /= persLight.w;
        //通过后透视空间的点光位置算出后透视空间的lightViewMatrix
        Matrix4x4 lightViewPers = CreateLookAt(persLight, new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 0.0f, 1.0f));
        
        //转换后的正方体[-1,-1,-1]->[1,1,1]
        float radius = Mathf.Sqrt(3.0f);
		float dist = Mathf.Sqrt( Vector3.Dot( persLight, persLight ) );
		float fov = 2.0f * Mathf.Atan( radius / dist ) * Mathf.Rad2Deg;
		float fNear = Mathf.Max( dist - radius, 0.001f );
		float fFar = dist + radius;
        //算出后透视空间的lightProjMatrix
        Matrix4x4 lightProjPers = Matrix4x4.Perspective(fov, 1.0f, fNear, fFar);//CreateGLUPerspective(fov, 1.0f, fNear, fFar);
        lightProjPers = GL.GetGPUProjectionMatrix(lightProjPers, true);
        Matrix4x4 lightViewProj = lightProjPers * lightViewPers;

       return lightViewProj * viewProj;
    }

    static public Matrix4x4 CalcLightSpaceShadowMapMatrix(Light light, Camera lightCamera, Camera mainCamera, bool targetTexture = true)
    {
        Vector3 lightDir = light.transform.forward;
        Vector3 viewDir = Camera.main.transform.forward;
        float angle = Mathf.Acos(Vector3.Dot(lightDir, viewDir)/(lightDir.magnitude * viewDir.magnitude));
        float sinAngle = Mathf.Sin(angle);//Mathf.Sqrt(1.0f - cosAngle * cosAngle);
        
        //算出up
        Vector3 left = Vector3.Cross(viewDir, lightDir);
        Vector3 up = Vector3.Cross( lightDir, left);//fix up switch
        //一个临时view，用来算AABB
        Vector3 eye = Camera.main.transform.position;
        Matrix4x4 lightView = CreateLookAt(eye, eye+lightDir, up);

        Vector3[] corners = GetCorners(Camera.main);
        for(int i = 0; i < corners.Length; i++)
        {
            corners[i] = lightView.MultiplyPoint(corners[i]);
        }
        Vector3 max, min;
        GetAABB(corners, out min, out max);
        //来自论文，目的是算出一个最优的n
        float factor = 1.0f / sinAngle;
        float z_n = factor * Camera.main.nearClipPlane;
        float d = max.y - min.y;
        float z0 = - z_n;
	    float z1 = - ( z_n + d * sinAngle );
	    float n = d / ( Mathf.Sqrt( z1 / z0 ) - 1.0f );

        float f = n + d;
        Vector3 lightPos = eye - up * (n - Camera.main.nearClipPlane);
        //真正的view
        lightView = CreateLookAt(lightPos, lightPos + lightDir, up);
        Matrix4x4 lightProj = Matrix4x4.identity;
        //这个proj比较特殊，只是为了压缩y方向的深度值
        lightProj.m11 = f /( f - n );
        lightProj.m31 = 1.0f;
        lightProj.m13 = f * n /( f - n );//fix minus
        lightProj.m33 = 0.0f;
        Matrix4x4 lightViewProj = lightProj * lightView;
        Vector3[] corners2 = GetCorners(Camera.main);
        for(int i = 0; i < corners.Length; i++)
        {
            corners2[i] = lightViewProj.MultiplyPoint(corners2[i]);
        }
        GetAABB(corners2, out min, out max);
        Matrix4x4 clip = GetUnitCubeClipMatrix(min, max);
        lightProj = clip * lightProj;//GL.GetGPUProjectionMatrix(clip * lightProj, false);

        lightViewProj = lightProj * lightView;

        return lightViewProj;
    }


    static public Matrix4x4 GetUnitCubeClipMatrix(Vector3 min, Vector3 max)
    {
        Matrix4x4 result;

        result.m00 = 2.0f / ( max.x - min.x );
        result.m01 = 0.0f;
        result.m02 = 0.0f;
        result.m03 = -( max.x + min.x ) / ( max.x - min.x );

        result.m10 = 0.0f;
        result.m11 = 2.0f / ( max.y - min.y );
        result.m12 = 0.0f;
        result.m13 = -( max.y + min.y ) / ( max.y - min.y );

        result.m20 = 0.0f;
        result.m21 = 0.0f;
        result.m22 = 1.0f / ( max.z - min.z );
        result.m23 = - min.z / ( max.z - min.z );

        result.m30 = 0.0f;
        result.m31 = 0.0f;
        result.m32 = 0.0f;
        result.m33 = 1.0f;

        return result;
    }
    
}
