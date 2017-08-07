using UnityEngine;
using System.Collections;

public class BezierTraveler : MonoBehaviour {

    public Bezier[] beziers;
    public float speed = 1f;
    public bool move = false;
	protected float progress = 0f;

	protected void Update()
    {
		if (move)
        {
			if (progress < 1f * beziers.Length)
			{
				transform.position = beziers[Mathf.FloorToInt(progress)].GetBezierPosition(progress - Mathf.Floor(progress));
				progress += Time.deltaTime * speed;
			}
			else
			{
				move = false;
				DoAction ();
			}
        }
    }

	public void Play(Bezier[] _beziers)
    {
		beziers = _beziers;
        progress = 0f;
		move = true;
    }

	protected virtual void DoAction ()
	{

	}
}
