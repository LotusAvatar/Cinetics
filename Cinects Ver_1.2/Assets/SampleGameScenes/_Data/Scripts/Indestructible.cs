using UnityEngine;
using System.Collections;

public class Indestructible : MonoBehaviour 
{
	void Awake () 
	{
		DontDestroyOnLoad(transform.gameObject);
	}
}
