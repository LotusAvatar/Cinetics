using UnityEngine;
using System.Collections;

public class Bazuca : MonoBehaviour 
{
	public GameObject tiro1;
	public GameObject tiro2;
	public GameObject tiro3;
	public Transform baseDoTiro;
	public Transform direcao;
	public Transform alvo;
	public float speed;
	float limiteHorizontal;
	float limiteVertical;
	GameObject tiroAtual;


	// Update is called once per frame
	void Update () 
	{
		transform.LookAt (alvo.position);

		if (Input.GetKeyDown(KeyCode.A)) 
		{
			Atirar(tiro1);
		}
		if (Input.GetKeyDown(KeyCode.S)) 
		{
			Atirar(tiro2);
		}
		if (Input.GetKeyDown(KeyCode.D)) 
		{
			Atirar(tiro3);
		}
		Movimento ();
		//Rotacao ();
	}

	void Rotacao()
	{
		if (Input.GetKey(KeyCode.LeftArrow)) 
		{
			transform.Rotate(Vector3.up, -2f);
		}
		if (Input.GetKey(KeyCode.RightArrow)) 
		{
			transform.Rotate(Vector3.up, 2f);
		}
	}

	void Movimento()
	{
		Vector3 pos = Input.mousePosition;
		pos.z = 0f;
		alvo.position = pos;

//		if (Input.GetKey(KeyCode.DownArrow)) 
//		{
//			Vector3 pos = alvo.position;
//			pos.y -= Time.deltaTime * speed;
//			alvo.position = pos;
//		}
//		if (Input.GetKey(KeyCode.UpArrow)) 
//		{
//			Vector3 pos = alvo.position;
//			pos.y += Time.deltaTime * speed;
//			alvo.position = pos;
//		}
//		if (Input.GetKey(KeyCode.LeftArrow)) 
//		{
//			Vector3 pos = alvo.position;
//			pos.x -= Time.deltaTime * speed;
//			alvo.position = pos;
//		}
//		if (Input.GetKey(KeyCode.RightArrow)) 
//		{
//			Vector3 pos = alvo.position;
//			pos.x += Time.deltaTime * speed;
//			alvo.position = pos;
//		}
	}

	void Atirar(GameObject tiroCorrente)
	{
		GameObject tiro = Instantiate (tiroCorrente, 
		baseDoTiro.position, baseDoTiro.rotation) as GameObject;
	
		Vector3 dir = direcao.position - baseDoTiro.position;
		dir = Vector3.Normalize (dir);
		tiro.GetComponent<Tiro> ().Initialize(dir);
	}
}






