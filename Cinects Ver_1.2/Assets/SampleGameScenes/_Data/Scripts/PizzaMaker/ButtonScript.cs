using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ButtonScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	public void LoadLevel2(){
		SceneManager.LoadScene(7);
	}

	public void LoadLevel3(){
		SceneManager.LoadScene(8);		
	}

	public void LoadMenu(){
		SceneManager.LoadScene (2);
	}
}
