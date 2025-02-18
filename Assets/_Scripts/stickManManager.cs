using DG.Tweening;
using UnityEngine;

public class stickManManager : MonoBehaviour
{
	[Header("FX (Effects)")]
	[SerializeField]
	private ParticleSystem blood;

	private bool hasJumped;

	private void Start()
	{
		DOTween.SetTweensCapacity(4000, 150);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Red") && other.transform.parent.childCount > 0)
		{
			Object.Destroy(other.gameObject);
			Object.Destroy(base.gameObject);
			Object.Instantiate(blood, base.transform.position, Quaternion.identity);
			PlayerManager.PlayerManagerInstance.UpdateStickManCount();
			EnemyManager component = other.transform.parent.GetComponent<EnemyManager>();
			if (component != null)
			{
				component.UpdateCounterText();
			}
			SoundManager.Instance.PlaySound("Kill");
		}
		if (other.CompareTag("Spike"))
		{
			base.transform.DOKill();
			Object.Destroy(base.gameObject);
			Object.Instantiate(blood, base.transform.position, Quaternion.identity);
			PlayerManager.PlayerManagerInstance.UpdateStickManCount();
			SoundManager.Instance.PlaySound("Kill");
		}
		if (other.CompareTag("Barricade"))
		{
			base.transform.DOKill();
			Object.Instantiate(blood, base.transform.position, Quaternion.identity);
		}
		if (other.CompareTag("Jump") && !hasJumped)
		{
			hasJumped = true;
			PlayerManager.PlayerManagerInstance.OnStickmanHitJump(this);
		}
	}

	public void DoJump()
	{
		base.transform.DOLocalJump(base.transform.localPosition, 3f, 1, 1.5f).SetEase(Ease.Flash).OnComplete(delegate
		{
			if (PlayerManager.PlayerManagerInstance != null)
			{
				PlayerManager.PlayerManagerInstance.OnStickmanLanded(this);
			}
		});
	}
}
