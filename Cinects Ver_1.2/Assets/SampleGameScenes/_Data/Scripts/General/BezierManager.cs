using UnityEngine;
using System.Collections;

public class BezierManager : MonoBehaviour 
{
	public Bezier[] beziers;
	Bezier[] randomBeziers = new Bezier[1];
	bool multiBezier;

	public Bezier[] GetRandomBezier(Vector3 ballPosition)
	{
		int rand = Random.Range (0, beziers.Length);
		beziers [rand].points [0].position = ballPosition;
		randomBeziers [0] = beziers [rand];
		if (multiBezier) 
		{

		}
		return randomBeziers;
	}


}
