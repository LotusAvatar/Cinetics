using UnityEngine;
using System.Collections;

[RequireComponent(typeof(ParticleSystem))]
public class CFX_AutoDestructShurikenSoccer : CFX_AutoDestructShuriken
{
	Vector3 hidePosition;

	protected override void OnEnable ()
	{
		hidePosition = ColorManager.Instance.HideBallPos;
		base.OnEnable ();
	}

	protected override IEnumerator CheckIfAlive ()
	{
		while(true)
		{
			yield return new WaitForSeconds(0.5f);
			if(!GetComponent<ParticleSystem>().IsAlive(true))
			{
				if(OnlyDeactivate)
				{
					this.gameObject.SetActive(false);
					transform.position = hidePosition;
				}
				else
					GameObject.Destroy(this.gameObject);
				break;
			}
		}
	}
}
