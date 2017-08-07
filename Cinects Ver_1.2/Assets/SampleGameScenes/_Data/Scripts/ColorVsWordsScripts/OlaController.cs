using UnityEngine;
using System.Collections;

public class OlaController : MonoBehaviour 
{
	public Transform[] olaPathLower;
	public Transform olaTriggerLower;
    bool canMove;
	float speed = 2.5f;
	int countPath;

	// Use this for initialization

	public void StartOla()
	{
		if (canMove) return;

        olaTriggerLower.position = olaPathLower[0].position;
		countPath = 1;
		canMove = true;
	}

	void OlaMovement()
	{
        olaTriggerLower.LookAt (olaPathLower[countPath].position);
        olaTriggerLower.Translate (Vector3.forward * speed);
        float distance = Vector3.Distance (olaTriggerLower.position, olaPathLower[countPath].position);

		if (distance <= 1.5f && countPath < olaPathLower.Length-1) 
			countPath++;
		else if (distance <= 1.5f && countPath == olaPathLower.Length-1) 
			canMove = false;
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (canMove) 
			OlaMovement();
	}
}
