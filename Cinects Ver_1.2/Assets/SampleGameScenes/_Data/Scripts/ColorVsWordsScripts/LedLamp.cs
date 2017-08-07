using UnityEngine;
using System.Collections;

public class LedLamp : MonoBehaviour 
{
	public MeshRenderer ledMaterial;
	Material blackMaterial;
	Material currentMaterial;

	void Start () 
	{
		blackMaterial = ledMaterial.material;
	}
	
	public void SetLedMaterial(Material nextMaterial)
	{
		currentMaterial = nextMaterial;
		ledMaterial.material = currentMaterial;
	}

	public void TurnOff()
	{
		ledMaterial.material = blackMaterial;
	}
}
