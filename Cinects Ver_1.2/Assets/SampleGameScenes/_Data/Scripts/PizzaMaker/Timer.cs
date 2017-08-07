using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Timer : MonoBehaviour {


	public Text Tempo;
	private float startTime;
	public bool readyToStart = false;
	public bool Victory = false;
	public string tempo;
	public GameObject Button;
	// Use this for initialization
	void Start () {

		preparado();
	}

	// Update is called once per frame
	void Update () {
		if(readyToStart && Victory == false){
			float t = Time.time - startTime;
			float minutes = ((int)t / 60);

			float seconds = (t % 60);
			tempo=(minutes.ToString ("00") + (":") + seconds.ToString ("00"));
			Tempo.text = tempo;
		}
	}
	void delay(){
		startTime = Time.time;
		readyToStart = true;
	}
	void preparado(){
		Tempo.text = "Preparado?";
		Invoke ("VAI", 4);
	}
	void VAI(){
		Tempo.text = "VAI!!!";
		Invoke ("delay", 1);
	}
	public void vitoria(){
		Victory = true;
		Tempo.color = Color.yellow;
		//Button.SetActive(true);
		Invoke ("GoToMenu", 3);
	}
	public void GoToMenu(){
		SceneManager.LoadScene(2);
	}

}