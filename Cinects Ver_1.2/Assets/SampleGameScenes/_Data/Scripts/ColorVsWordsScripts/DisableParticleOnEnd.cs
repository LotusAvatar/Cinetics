using UnityEngine;
using System.Collections;

public class DisableParticleOnEnd : MonoBehaviour 
{

	bool checkEnd;
	float timer = 0f;
	Vector3 hidePosition;

	// Use this for initialization
	void Enable () 
	{
		checkEnd = true;
		timer = 0f;
		gameObject.GetComponent<ParticleSystem> ().Play ();
		hidePosition = ColorManager.Instance.HideBallPos;
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (checkEnd) 
		{
			timer += Time.deltaTime;
			if (timer >= gameObject.GetComponent<ParticleSystem>().duration) 
			{
				checkEnd = false;
				transform.position = hidePosition;
				gameObject.SetActive(false);
			}
		}
	}
}
