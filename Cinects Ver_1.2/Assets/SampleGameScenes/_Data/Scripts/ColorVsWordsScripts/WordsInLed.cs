using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WordsInLed : MonoBehaviour {

	public LedColumn ledColumnPrefab;
	LedColumn[] ledColumns;
	GameObject nextLedColumn;

	int ledMovementLimit;
	int[,] redInLedMatrix;
	int[,] blueInLedMatrix;
	int[,] greenInLedMatrix;
	int[,] yellowInLedMatrix;
	int[,] currentLedMatrix;
	const int ledAmount = 56;
	const float ledColumnDistance = 0.13f;
	Vector3 nextLedColumnPos;
	Material currentColorMaterial;

	void Start () 
	{
		FillRedInLedMatrix ();
		FillGreenInLedMatrix ();
		FillYellowInLedMatrix ();
		FillBlueInLedMatrix ();
		FillLedPainel ();
	}

	public void UpdateNextRule(Material colorMaterial, int textIndex)
	{
		currentColorMaterial = colorMaterial;

		switch (textIndex) 
		{
		case 0: currentLedMatrix = redInLedMatrix; break;
		case 1: currentLedMatrix = blueInLedMatrix; break;
		case 2: currentLedMatrix = greenInLedMatrix; break;
		case 3: currentLedMatrix = yellowInLedMatrix; break;
		default: break;
		}

		ledMovementLimit = ((ledAmount - currentLedMatrix.GetLength (1)) / 2);
		StartCoroutine (LightOnLedPainel ());
	}

	IEnumerator LightOnLedPainel()
	{
		for (int i = ledAmount - 1; i >= ledMovementLimit; i--) 
		{
			int count = 0;
			for (int j = i; j < ledAmount; j++) 
			{
				for (int l = 0; l < currentLedMatrix.GetLength(0); l++) 
				{
					if (count < currentLedMatrix.GetLength(1)) 
					{
						if (currentLedMatrix[l,count] == 1) 
						{
							ledColumns[j].ledLampColumn[l].SetLedMaterial(currentColorMaterial);
						}
						else
						{
							ledColumns[j].ledLampColumn[l].TurnOff();
						}
					}
					else
					{
						ledColumns[j].ledLampColumn[l].TurnOff();
					}

				}
				count++;
			}

			if (i % 2 == 0) 
				yield return new WaitForEndOfFrame();
		}
		StopCoroutine(LightOnLedPainel());
	}

	public void RefreshLedPainel()
	{
		for (int i = 0; i < ledColumns.Length; i++) 
		{
			for (int j = 0; j < ledColumns[i].ledLampColumn.Length; j++) 
			{
				ledColumns[i].ledLampColumn[j].TurnOff();
			}
		}
	}

	void FillLedPainel()
	{
		ledColumns = new LedColumn[ledAmount];
		ledColumns [0] = ledColumnPrefab;

		for (int i = 1; i < ledAmount; i++) 
		{
			nextLedColumnPos = ledColumns[i-1].transform.position;
			nextLedColumnPos.x += ledColumnDistance;
			nextLedColumn = Instantiate(ledColumnPrefab.gameObject, nextLedColumnPos, ledColumnPrefab.transform.rotation) as GameObject;
			nextLedColumn.transform.SetParent(this.transform);
			ledColumns [i] = nextLedColumn.GetComponent<LedColumn>();
		}
	}

	void FillRedInLedMatrix()
	{
		int[] ledMatrixLine0 = {1,1,0,0,1,1,0,1,1,1,1,1,0,1,1,1,1,1,0,0,1,1,0,0,0,0,0,1,1,0,1,1,1,1,1,0,1,1,0,0,0,0,1,1,0,0,1,1,0,0,1,1,1,0};
		int[] ledMatrixLine1 = {1,1,0,0,1,1,0,1,1,0,0,0,0,1,1,0,0,1,1,0,1,1,1,0,0,0,1,1,1,0,1,1,0,0,0,0,1,1,0,0,0,0,1,1,0,0,1,1,0,1,1,1,1,1};
		int[] ledMatrixLine2 = {1,1,0,0,1,1,0,1,1,1,1,0,0,1,1,0,0,1,1,0,1,1,0,1,0,1,0,1,1,0,1,1,1,1,0,0,1,1,0,0,0,0,1,1,1,1,1,1,0,1,1,0,1,1};
		int[] ledMatrixLine3 = {1,1,0,0,1,1,0,1,1,1,1,0,0,1,1,1,1,1,0,0,1,1,0,0,1,0,0,1,1,0,1,1,1,1,0,0,1,1,0,0,0,0,1,1,1,1,1,1,0,1,1,0,1,1};
		int[] ledMatrixLine4 = {0,1,0,0,1,0,0,1,1,0,0,0,0,1,1,0,1,1,0,0,1,1,0,0,0,0,0,1,1,0,1,1,0,0,0,0,1,1,1,1,1,0,1,1,0,0,1,1,0,1,1,1,1,1};
		int[] ledMatrixLine5 = {0,0,1,1,0,0,0,1,1,1,1,1,0,1,1,0,0,1,1,0,1,1,0,0,0,0,0,1,1,0,1,1,1,1,1,0,1,1,1,1,1,0,1,1,0,0,1,1,0,0,1,1,1,0};
		List<int[]> ledList = new List<int[]> ();
		ledList.Add (ledMatrixLine0);
		ledList.Add (ledMatrixLine1);
		ledList.Add (ledMatrixLine2);
		ledList.Add (ledMatrixLine3);
		ledList.Add (ledMatrixLine4);
		ledList.Add (ledMatrixLine5);

		redInLedMatrix = new int[ledList.Count, ledMatrixLine0.Length];

		for (int i = 0; i < ledList.Count; i++) 
			for (int j = 0; j < redInLedMatrix.GetLength(1); j++) 
				redInLedMatrix[i, j] = ledList[i][j];
	}

	void FillGreenInLedMatrix()
	{
		int[] ledMatrixLine0 = {1,1,0,0,1,1,0,1,1,1,1,1,0,1,1,1,1,1,0,0,1,1,1,1,0,0,1,1,1,1,1};
		int[] ledMatrixLine1 = {1,1,0,0,1,1,0,1,1,0,0,0,0,1,1,0,0,1,1,0,1,1,0,1,1,0,1,1,0,0,0};
		int[] ledMatrixLine2 = {1,1,0,0,1,1,0,1,1,1,1,0,0,1,1,0,0,1,1,0,1,1,0,1,1,0,1,1,1,1,0};
		int[] ledMatrixLine3 = {1,1,0,0,1,1,0,1,1,1,1,0,0,1,1,1,1,1,0,0,1,1,0,1,1,0,1,1,1,1,0};
		int[] ledMatrixLine4 = {0,1,0,0,1,0,0,1,1,0,0,0,0,1,1,0,1,1,0,0,1,1,0,1,1,0,1,1,0,0,0};
		int[] ledMatrixLine5 = {0,0,1,1,0,0,0,1,1,1,1,1,0,1,1,0,0,1,1,0,1,1,1,1,0,0,1,1,1,1,1};
		List<int[]> ledList = new List<int[]> ();
		ledList.Add (ledMatrixLine0);
		ledList.Add (ledMatrixLine1);
		ledList.Add (ledMatrixLine2);
		ledList.Add (ledMatrixLine3);
		ledList.Add (ledMatrixLine4);
		ledList.Add (ledMatrixLine5);
		
		greenInLedMatrix = new int[ledList.Count, ledMatrixLine0.Length];
		
		for (int i = 0; i < ledList.Count; i++) 
			for (int j = 0; j < greenInLedMatrix.GetLength(1); j++) 
				greenInLedMatrix[i, j] = ledList[i][j];
	}

	void FillYellowInLedMatrix()
	{
		int[] ledMatrixLine0 = {0,0,1,1,0,0,0,1,1,0,0,0,0,0,1,1,0,0,0,1,1,0,0,0,1,1,1,1,1,0,0,1,1,1,1,1,0,1,1,0,0,0,0,0,1,1,1,0};
		int[] ledMatrixLine1 = {0,1,0,0,1,0,0,1,1,1,0,0,0,1,1,1,0,0,1,0,0,1,0,0,1,1,0,0,1,1,0,1,1,0,0,0,0,1,1,0,0,0,0,1,1,1,1,1};
		int[] ledMatrixLine2 = {1,1,0,0,1,1,0,1,1,0,1,0,1,0,1,1,0,1,1,0,0,1,1,0,1,1,0,0,1,1,0,1,1,1,1,0,0,1,1,0,0,0,0,1,1,0,1,1};
		int[] ledMatrixLine3 = {1,1,1,1,1,1,0,1,1,0,0,1,0,0,1,1,0,1,1,1,1,1,1,0,1,1,1,1,1,0,0,1,1,1,1,0,0,1,1,0,0,0,0,1,1,0,1,1};
		int[] ledMatrixLine4 = {1,1,0,0,1,1,0,1,1,0,0,0,0,0,1,1,0,1,1,0,0,1,1,0,1,1,0,1,1,0,0,1,1,0,0,0,0,1,1,1,1,1,0,1,1,1,1,1};
		int[] ledMatrixLine5 = {1,1,0,0,1,1,0,1,1,0,0,0,0,0,1,1,0,1,1,0,0,1,1,0,1,1,0,0,1,1,0,1,1,1,1,1,0,1,1,1,1,1,0,0,1,1,1,0};
		List<int[]> ledList = new List<int[]> ();
		ledList.Add (ledMatrixLine0);
		ledList.Add (ledMatrixLine1);
		ledList.Add (ledMatrixLine2);
		ledList.Add (ledMatrixLine3);
		ledList.Add (ledMatrixLine4);
		ledList.Add (ledMatrixLine5);
		
		yellowInLedMatrix = new int[ledList.Count, ledMatrixLine0.Length];
		
		for (int i = 0; i < ledList.Count; i++) 
			for (int j = 0; j < yellowInLedMatrix.GetLength(1); j++) 
				yellowInLedMatrix[i, j] = ledList[i][j];
	}

	void FillBlueInLedMatrix()
	{
		int[] ledMatrixLine0 = {0,0,1,1,0,0,0,1,1,1,1,1,0,1,1,0,0,1,1,0,1,1,0,0,0};
		int[] ledMatrixLine1 = {0,1,0,0,1,0,0,0,0,0,1,1,0,1,1,0,0,1,1,0,1,1,0,0,0};
		int[] ledMatrixLine2 = {1,1,0,0,1,1,0,0,0,1,1,0,0,1,1,0,0,1,1,0,1,1,0,0,0};
		int[] ledMatrixLine3 = {1,1,1,1,1,1,0,0,1,1,0,0,0,1,1,0,0,1,1,0,1,1,0,0,0};
		int[] ledMatrixLine4 = {1,1,0,0,1,1,0,1,1,0,0,0,0,1,1,0,0,1,1,0,1,1,1,1,1};
		int[] ledMatrixLine5 = {1,1,0,0,1,1,0,1,1,1,1,1,0,1,1,1,1,1,1,0,1,1,1,1,1};
		List<int[]> ledList = new List<int[]> ();
		ledList.Add (ledMatrixLine0);
		ledList.Add (ledMatrixLine1);
		ledList.Add (ledMatrixLine2);
		ledList.Add (ledMatrixLine3);
		ledList.Add (ledMatrixLine4);
		ledList.Add (ledMatrixLine5);
		
		blueInLedMatrix = new int[ledList.Count, ledMatrixLine0.Length];
		
		for (int i = 0; i < ledList.Count; i++) 
			for (int j = 0; j < blueInLedMatrix.GetLength(1); j++) 
				blueInLedMatrix[i, j] = ledList[i][j];
	}
}
