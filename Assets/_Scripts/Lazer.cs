using UnityEngine;

public class Lazer : MonoBehaviour
{
	[SerializeField]
	private float offsetAfterSpike;

	private bool hasPassed;

	private void Update()
	{
		if (!hasPassed && PlayerManager.PlayerManagerInstance != null && PlayerManager.PlayerManagerInstance.GetEndOfCrowdPosition().z > base.transform.position.z + offsetAfterSpike)
		{
			hasPassed = true;
		}
	}
}
