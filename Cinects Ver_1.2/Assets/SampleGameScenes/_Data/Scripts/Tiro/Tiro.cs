using UnityEngine;
using System.Collections;

public class Tiro : MonoBehaviour 
{
	public string tagName = "Alvo";
	public float speed = 1f;
	Vector3 direction;
	bool podeAndar;

	public void Initialize(Vector3 _direction)
	{
		Debug.Log ("Direcao: " + _direction);
		direction = _direction;
		podeAndar = true;
	}

	// Update is called once per frame
	void Update () 
	{
		if (podeAndar == true)
		{
			transform.position += direction * speed;
		}
	}

	void OnTriggerEnter(Collider hit)
	{
		if (hit.tag == tagName) 
		{
			Destroy(this.gameObject);
		}
	}
}
