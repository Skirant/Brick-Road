using UnityEngine;

public class EnenyAndObstacles : MonoBehaviour
{
	[Header("Настройки")]
	[Tooltip("Сбор персонажей в кучку после прохода этого препятствия")]
	[SerializeField]
	private bool FormatStickMan = true;

	[Tooltip("Насколько далеко от врага нужно отойти, чтобы считалось, что толпа его прошла ")]
	[SerializeField]
	private float offsetAfterSpike;

	private bool hasPassed;

	private void Update()
	{
		if (!hasPassed && PlayerManager.PlayerManagerInstance != null && PlayerManager.PlayerManagerInstance.GetEndOfCrowdPosition().z > base.transform.position.z + offsetAfterSpike + 4f)
		{
			hasPassed = true;
			if (FormatStickMan)
			{
				PlayerManager.PlayerManagerInstance.FormatStickMan();
			}
		}
	}
}
