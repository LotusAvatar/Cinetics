using UnityEngine;
using System.Collections;

public class BallBezierTravaler : BezierTraveler 
{
	protected override void DoAction ()
	{
		ColorManager.Instance.ExplodeBall ();
	}
}
