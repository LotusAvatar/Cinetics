using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GoalGradient : MonoBehaviour 
{
	public TextMesh[] gradient;
	Dictionary<int, Color> ColorPalette = new Dictionary<int, Color>();
	float unity;

	// Use this for initialization
	void Start () 
	{
		unity = 1f / (float)gradient.Length;
		ColorGradient ();
	}

	void ColorGradient()
	{
		float unityTotal = 0f;
		Color auxColor = new Color ();
		for (int i = 0; i < gradient.Length; i++) 
		{
			float amount = 0f;
			unityTotal += unity;
			if (unityTotal <= 0.5f) 
			{
				amount = Mathf.InverseLerp(0f, 0.5f, unityTotal);
				auxColor = Color.Lerp(Color.red, Color.yellow, amount);
			}
			else
			{
				amount = Mathf.InverseLerp(0.5f, 1f, unityTotal);
				auxColor = Color.Lerp(Color.yellow, Color.green, amount);
//				Debug.Log("Amount: "+amount+ "Color: "+ auxColor);
			}
			ColorPalette.Add(i, auxColor);
			gradient[i].color = Color.black;
		}
	}

	public void UpdateGradient(float percentage)
	{
		int count = 0;
		for (float i = 0f; i <= percentage; i += unity) 
		{
			gradient[count].color = ColorPalette[count];
			count++;
		}
	}
}
