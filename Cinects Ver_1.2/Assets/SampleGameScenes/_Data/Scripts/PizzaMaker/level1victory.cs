using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class level1victory : MonoBehaviour {

	// Use this for initialization
	void Start () {
		objOven.GetComponent<OvenCollider> ().level1 ();
	}

	public GameObject Kitchen;
	public GameObject Texto; // MainCamera
	public GameObject objOven;

	public void Victory(){
		if (Kitchen.GetComponent<IngredientsController> ().Bacon >= 2 || Kitchen.GetComponent<IngredientsController> ().Pepperoni >= 2)
		{
			if (Kitchen.GetComponent<IngredientsController> ().Cheese >= 3 && Kitchen.GetComponent<IngredientsController> ().RedPepper >= 2) {
				Texto.GetComponent<Timer> ().vitoria ();

			} else {
				Kitchen.GetComponent<IngredientsController> ().zerar ();
			}

		}

	}
}
