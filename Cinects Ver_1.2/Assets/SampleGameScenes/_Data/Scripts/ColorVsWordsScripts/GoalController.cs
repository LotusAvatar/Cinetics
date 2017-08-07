using UnityEngine;
using System.Collections;

public class GoalController : MonoBehaviour {

	public GoalGradient gradient;

	public void UpdateGoal(int totalKicks, int currentKick)
	{
		if (currentKick <= totalKicks) 
		{
			float percentage = ((float)currentKick / (float)totalKicks);
			gradient.UpdateGradient(percentage);
			percentage *= 100f;
			int amount = (int)percentage;
			string number = amount.ToString(); 
			if (amount < 10) 
				number = "0"+amount.ToString();

			GetComponent<TextMesh> ().text = "Meta "+ number + "%";
		}
	}
}
