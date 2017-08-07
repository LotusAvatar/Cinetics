using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class level3victory : MonoBehaviour {

	// Use this for initialization
	void Start () {
		objOven.GetComponent<OvenCollider> ().level3 ();
	}

	public GameObject Kitchen;
	public GameObject Texto; // MainCamera
	public GameObject objOven;

	public void Victory(){
		if (Kitchen.GetComponent<IngredientsController> ().Bacon >= 0 || Kitchen.GetComponent<IngredientsController> ().Onion >= 0)
		{
			if (Kitchen.GetComponent<IngredientsController> ().Cheese >= 3 && Kitchen.GetComponent<IngredientsController> ().Olive >= 2 && Kitchen.GetComponent<IngredientsController> ().Shrimp >= 2 && Kitchen.GetComponent<IngredientsController> ().Pepperoni >= 2 && Kitchen.GetComponent<IngredientsController> ().Tomato >= 2){
				Texto.GetComponent<Timer> ().vitoria ();
			} else {
				Kitchen.GetComponent<IngredientsController> ().zerar ();
			}

		}

	}
}
