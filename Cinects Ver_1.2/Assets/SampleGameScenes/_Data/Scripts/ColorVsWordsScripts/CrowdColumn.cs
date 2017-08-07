using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CrowdColumn : MonoBehaviour 
{
	Transform[] fanPositions;
	FanUnity[] fans;
	Object[] prefabFans;
	GameObject fan;

	// Use this for initialization

	public void FillCrowd()
	{
		fanPositions = new Transform[transform.childCount];
		fans = new FanUnity[transform.childCount];
		for (int i = 0; i < transform.childCount; i++) 
		{
			fanPositions[i] = transform.GetChild(i);
		}
		string path = "SoccerColorVsWords/Fans";
		
		// Use this for initialization
		prefabFans = Resources.LoadAll (path);
		
		
		for (int i = 0; i < fanPositions.Length; i++) 
		{
			int rand = Random.Range(0, prefabFans.Length);
			fan = Instantiate(prefabFans[rand], fanPositions[i].position, fanPositions[i].rotation) as GameObject;
			fan.transform.SetParent(fanPositions[i]);
			fans[i] = fan.GetComponent<FanUnity>();
		}
	}

	public void MissDefence()
	{
		for (int i = 0; i < fans.Length; i++) 
		{
			fans[i].SetCurrentState(FanUnity.FanState.MissDefence);
		}
	}

	public void Celebrate()
	{
		int rand =  Random.Range(0,2);
		
		if (rand == 0) 
		{
			for (int i = 0; i < fans.Length; i++) 
				fans[i].SetCurrentState(FanUnity.FanState.Thrilled);
		}
		else
			for (int i = 0; i < fans.Length; i++) 
				fans[i].SetCurrentState(FanUnity.FanState.Clapping);
	}

	void OnTriggerEnter(Collider hit) 
	{
		if (hit.tag == "OlaTrigger") 
		{
			for (int i = 0; i < fans.Length; i++) 
			{
				fans[i].SetCurrentState(FanUnity.FanState.Ola);
			}
		}
	}
}
