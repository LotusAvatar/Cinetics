using UnityEngine;
using System.Collections;

public class CrowdController : MonoBehaviour 
{
	public CrowdColumn[] crowndColumns;

	// Use this for initialization
	void Start () 
	{
		for (int i = 0; i < crowndColumns.Length; i++) 
		{
			crowndColumns[i].FillCrowd();
		}
	}

	public void MissDefence()
	{
		for (int i = 0; i < crowndColumns.Length; i++) 
		{
			crowndColumns[i].MissDefence();
		}
	}

	public void Celebrate()
	{
		for (int i = 0; i < crowndColumns.Length; i++) 
			crowndColumns[i].Celebrate(); 
	}
}
