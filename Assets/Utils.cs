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
		max = points[0];
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

}
