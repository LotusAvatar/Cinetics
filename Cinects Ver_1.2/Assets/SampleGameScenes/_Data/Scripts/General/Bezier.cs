using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Bezier : MonoBehaviour {
    
    public int iterations;
	public Transform[] points;
	List<Vector3> pointsList = new List<Vector3> ();

    public Vector3 GetBezierPosition(float progress)
    {
        if (progress < 0 && progress > 1f)
        {
            print("error");
            return Vector3.zero;
        }
        else
        {
            float x = progress;
			Vector3 p = new Vector3();
			pointsList.Clear();
			for (int i = 0; i < points.Length-1; i++) 
			{
				p = Vector3.Lerp(points[i].position, points[i+1].position, x);
				pointsList.Add(p);
			}

			do 
			{
				for (int i = 0; i < pointsList.Count-1; i++) 
					pointsList[i] = Vector3.Lerp(pointsList[i], pointsList[i+1], x);

				pointsList.RemoveAt(pointsList.Count-1);

				if (pointsList.Count == 1) 
					p = pointsList[0];

			} while (pointsList.Count > 1);
            
            return p;
        }
    }

    private void OnDrawGizmos()
    {
		bool isNull = false;

		for (int i = 0; i < points.Length; i++) 
		{
			if (points[i] ==  null) 
			{
				isNull = true;
				break;
			}
		}

		if (isNull) return;
        
        Gizmos.color = Color.red;
		Gizmos.DrawSphere(points[0].position, 0.1f);
		Gizmos.DrawSphere(points[1].position, 0.1f);
		Gizmos.DrawLine(points[0].position, points[1].position);

        Gizmos.color = Color.blue;
		Gizmos.DrawSphere(points[2].position, 0.1f);
		Gizmos.DrawSphere(points[3].position, 0.1f);
		Gizmos.DrawLine(points[2].position, points[3].position);

        float stepping = 1.0f / iterations;
 
        for (float x = 0.0f; x <= 1.0f; x += stepping)
        {
            Gizmos.color = Color.Lerp(Color.red, Color.blue, x);
            Gizmos.DrawSphere(GetBezierPosition(x), 0.1f);
        }
    }
}
