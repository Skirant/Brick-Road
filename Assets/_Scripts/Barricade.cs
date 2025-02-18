using UnityEngine;

public class Barricade : MonoBehaviour
{
	[Header("Настройки баррикады")]
	[SerializeField]
	private int stickmanLoss = 10;

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Blue"))
		{
			PlayerManager.PlayerManagerInstance.RemoveStickMen(stickmanLoss);
			Object.Destroy(base.gameObject);
			SoundManager.Instance.PlaySound("Barricade");
		}
	}
}
