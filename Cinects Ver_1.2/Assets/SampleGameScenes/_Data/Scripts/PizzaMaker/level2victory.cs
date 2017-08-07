using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class level2victory : MonoBehaviour {

	// Use this for initialization
	void Start () {
		objOven.GetComponent<OvenCollider> ().level2 ();
	}

	public GameObject Kitchen;
	public GameObject Texto; // MainCamera
	public GameObject objOven;

	public void Victory(){
		if (Kitchen.GetComponent<IngredientsController> ().Bacon >= 0 && Kitchen.GetComponent<IngredientsController> ().Pepperoni >= 0)
		{
			if (Kitchen.GetComponent<IngredientsController> ().Cheese >= 5 && Kitchen.GetComponent<IngredientsController> ().Shrimp >= 2) {
				Texto.GetComponent<Timer> ().vitoria ();
			} else {
				Kitchen.GetComponent<IngredientsController> ().zerar ();
			}

		}

	}
}
