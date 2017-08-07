using UnityEngine;
using System.Collections;

public class CrowdManager : MonoBehaviour 
{
	public OlaController olaController; 
	CrowdController[] crowdControllers;

	static CrowdManager _instance;
	
	public static CrowdManager Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = FindObjectOfType<CrowdManager>() as CrowdManager;
			}
			return _instance;
		}
	}

	void OnEnable()
	{
		ColorManager.FeedbackEvent += OnFeedbackEvent;
	}
	
	void OnDestroy()
	{
		ColorManager.FeedbackEvent -= OnFeedbackEvent;
	}

	// Use this for initialization
	void Start () 
	{
		crowdControllers = FindObjectsOfType<CrowdController> ();
	}
	
	void OnFeedbackEvent(FeedbackTypes feedback)
	{
		if (feedback == FeedbackTypes.Gain) 
		{
			int rand =  Random.Range(0,2);
			
			if (rand == 0) 
				olaController.StartOla();
			else
				foreach (CrowdController crowd in crowdControllers)
					crowd.Celebrate();
		}
		else
			foreach (CrowdController crowd in crowdControllers)
				crowd.MissDefence();
	}
}
