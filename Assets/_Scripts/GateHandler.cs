using UnityEngine;

public class GateHandler : MonoBehaviour
{
	[Header("Настройки времени между бонусами")]
	[SerializeField]
	private float bonusCooldown = 1f;

	private float lastBonusTime = -1f;

	private void OnTriggerEnter(Collider other)
	{
		if (!other.CompareTag("Player") || Time.time - lastBonusTime < bonusCooldown)
		{
			return;
		}
		lastBonusTime = Time.time;
		PlayerManager component = other.GetComponent<PlayerManager>();
		if (component == null)
		{
			return;
		}
		GateManager componentInChildren = GetComponentInChildren<GateManager>();
		if (!(componentInChildren == null))
		{
			int stickmanCount = component.GetStickmanCount();
			int num = (componentInChildren.multiply ? (stickmanCount * componentInChildren.randomNumber) : (stickmanCount + componentInChildren.randomNumber)) - stickmanCount;
			if (num > 0)
			{
				component.MakeStickMan(component.GetStickmanCount() + num);
			}
			BoxCollider[] componentsInChildren = GetComponentsInChildren<BoxCollider>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = false;
			}
		}
	}
}
