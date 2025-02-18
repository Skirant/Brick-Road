using UnityEngine;

public class TrigerOpenChest : MonoBehaviour
{
	[Header("Настройки анимации")]
	[SerializeField]
	private Animator animator;

	[SerializeField]
	private string animationTrigger = "Win";

	[Header("Игровые параметры")]
	public bool Win { get; private set; }

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			ActivateWinState();
		}
	}

	private void ActivateWinState()
	{
		if (animator != null)
		{
			animator.SetTrigger(animationTrigger);
		}
		Win = true;
	}
}
