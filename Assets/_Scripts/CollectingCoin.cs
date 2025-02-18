using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class CollectingCoin : MonoBehaviour
{
	[Header("Основные настройки")]
	[SerializeField]
	private GameObject coinPrefab;

	[SerializeField]
	private Transform coinParent;

	[SerializeField]
	private Transform spawnLocation;

	[SerializeField]
	private Transform endPosition;

	[SerializeField]
	private TextMeshProUGUI _coinText;

	[SerializeField]
	private float duration = 1f;

	[SerializeField]
	private int coinAmount = 20;

	[Header("Случайное смещение спавна")]
	[SerializeField]
	private float minX;

	[SerializeField]
	private float maxX;

	[SerializeField]
	private float minY;

	[SerializeField]
	private float maxY;

	private readonly List<GameObject> coins = new List<GameObject>();

	private readonly List<int> coinValues = new List<int>();

	private Tween coinReactionTween;

	private int coin;

	public void CollectCoinsWithVisual(int startMoney, int winCoin)
	{
		coin = startMoney;
		_coinText.text = coin.ToString();
		StartCoroutine(SpawnAndMoveCoins(winCoin));
	}

	private IEnumerator SpawnAndMoveCoins(int totalWinCoins)
	{
		int baseCoinValue = totalWinCoins / coinAmount;
		int remainder = totalWinCoins % coinAmount;
		for (int i = 0; i < coinAmount; i++)
		{
			int num = baseCoinValue;
			if (remainder > 0)
			{
				num++;
				remainder--;
			}
			coinValues.Add(num);
			GameObject gameObject = Object.Instantiate(coinPrefab, coinParent);
			float x = spawnLocation.position.x + Random.Range(minX, maxX);
			float y = spawnLocation.position.y + Random.Range(minY, maxY);
			gameObject.transform.position = new Vector3(x, y);
			gameObject.transform.DOPunchPosition(new Vector3(0f, 30f, 0f), Random.Range(0.1f, 1f)).SetEase(Ease.InOutElastic);
			coins.Add(gameObject);
			yield return new WaitForSeconds(0.001f);
		}
		yield return StartCoroutine(MoveCoins());
	}

	private IEnumerator MoveCoins()
	{
		for (int i = coins.Count - 1; i >= 0; i--)
		{
			MoveCoin(coins[i], coinValues[i]);
			yield return new WaitForSeconds(0.05f);
		}
		coinValues.Clear();
	}

	private void MoveCoin(GameObject coinInstance, int coinValue)
	{
		coinInstance.transform.DOMove(endPosition.position, duration).SetEase(Ease.InBack).OnComplete(delegate
		{
			coins.Remove(coinInstance);
			Object.Destroy(coinInstance);
			ReactToCollectionCoin();
			SetCoin(coin + coinValue);
		});
	}

	private void ReactToCollectionCoin()
	{
		if (coinReactionTween == null || !coinReactionTween.IsActive())
		{
			coinReactionTween = endPosition.DOPunchScale(new Vector3(0.5f, 0.5f, 0.5f), 0.1f).SetEase(Ease.InOutElastic);
			SoundManager.Instance.PlaySound("Coin");
		}
	}

	private void SetCoin(int value)
	{
		coin = value;
		_coinText.text = coin.ToString();
	}
}
