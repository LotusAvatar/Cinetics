using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Score : MonoBehaviour {
	
	public Text Points;
	public float Pontuation;

	// Use this for initialization
	void Start () {
		Points.text=(Pontuation.ToString ("00000"));
			
	}
	
	public void Refresh(){
		Points.text=(Pontuation.ToString ("00000"));
	}
	public void AddHard(){
		Pontuation += 50;
		Refresh ();
	}

	public void AddModerate(){
		Pontuation += 25;
		Refresh ();
	}

	public void AddEasy(){
		Pontuation += 12.5f;
		Refresh ();
	}
	public void AddExtra(){
		Pontuation += 5;
		Refresh ();
	}

	public void Remove(){
		Pontuation -= 25;
		Refresh ();
	}
	// Update is called once per frame
	void Update () {
		
	}
}
