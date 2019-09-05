using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utils  
{
	static public Vector3[] GetCorners (Camera theCamera)
	{
		Vector3[] corners = new Vector3[ 5 ];
		Transform tx = theCamera.transform;
		float distance = theCamera.farClipPlane;
		
		float halfFOV = ( theCamera.fieldOfView * 0.5f ) * Mathf.Deg2Rad;
		float aspect = theCamera.aspect;
		
		float height = distance * Mathf.Tan( halfFOV );
		float width = height * aspect;
		
		// UpperLeft
		corners[ 0 ] = tx.position - ( tx.right * width );
		corners[ 0 ] += tx.up * height;
		corners[ 0 ] += tx.forward * distance;
		
		// UpperRight
		corners[ 1 ] = tx.position + ( tx.right * width );
		corners[ 1 ] += tx.up * height;
		corners[ 1 ] += tx.forward * distance;
		
		// LowerLeft
		corners[ 2 ] = tx.position - ( tx.right * width );
		corners[ 2 ] -= tx.up * height;
		corners[ 2 ] += tx.forward * distance;
		
		// LowerRight
		corners[ 3 ] = tx.position + ( tx.right * width );
		corners[ 3 ] -= tx.up * height;
		corners[ 3 ] += tx.forward * distance;

		corners[4] = tx.position;
		
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

	static public Matrix4x4 CalcBaseShadowMapMatrix(Light light, Camera lightCamera, Camera mainCamera)
	{
		//算出视锥体的5个顶点
		Vector3[] corners = Utils.GetCorners(mainCamera);
		//转换到lightview空间
		Vector3 to = light.transform.position + light.transform.forward;
		Matrix4x4 mat = Matrix4x4.LookAt(light.transform.position, to, light.transform.up);
		for(int i = 0; i < corners.Length; i++)
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
		Debug.Log("center========="+center);

		Vector3 lightTo = center + light.transform.forward;
		Matrix4x4 lightView = Matrix4x4.LookAt(center, lightTo, light.transform.up);
		Matrix4x4 lightProj = Matrix4x4.Ortho(-boxSize.x*0.5f, boxSize.x*0.5f, boxSize.y*0.5f, -boxSize.y*0.5f, 0.0f, 300.0f);

		lightCamera.orthographicSize = boxSize.x/2.0f;
		lightCamera.aspect = boxSize.x/boxSize.y;
        Matrix4x4 worldToView = lightCamera.worldToCameraMatrix;
        Matrix4x4 projection  = GL.GetGPUProjectionMatrix(lightCamera.projectionMatrix, false);

 		return   projection * worldToView;
	}

}
