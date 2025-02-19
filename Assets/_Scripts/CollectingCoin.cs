using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class CollectingCoin : MonoBehaviour
{
    [Header("Основные настройки")]
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private Transform coinParent;
    [SerializeField] private Transform spawnLocation;
    [SerializeField] private Transform endPosition;
    [SerializeField] private TextMeshProUGUI _coinText;
    [SerializeField] private float duration = 1f;
    [SerializeField] private int coinAmount = 20;

    [Header("Случайное смещение спавна")]
    [SerializeField] private float minX;
    [SerializeField] private float maxX;
    [SerializeField] private float minY;
    [SerializeField] private float maxY;

    private readonly List<GameObject> coins = new List<GameObject>();
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
            int coinValue = baseCoinValue + (remainder-- > 0 ? 1 : 0);
            GameObject coinInstance = Instantiate(coinPrefab, coinParent);

            float x = spawnLocation.position.x + Random.Range(minX, maxX);
            float y = spawnLocation.position.y + Random.Range(minY, maxY);

            coinInstance.transform.position = new Vector3(x, y);
            coinInstance.transform.DOPunchPosition(new Vector3(0f, 30f, 0f), Random.Range(0.1f, 1f)).SetEase(Ease.InOutElastic);

            coins.Add(coinInstance);
            StartCoroutine(MoveCoin(coinInstance, coinValue));
            yield return new WaitForSeconds(0.05f);
        }
    }

    private IEnumerator MoveCoin(GameObject coinInstance, int coinValue)
    {
        coinInstance.transform.DOMove(endPosition.position, duration).SetEase(Ease.InBack).OnComplete(() =>
        {
            coins.Remove(coinInstance);
            if (coinInstance != null)
            {
                Destroy(coinInstance);
            }
            ReactToCollectionCoin();
            SetCoin(coin + coinValue);
        });
        yield return null;
    }

    private void ReactToCollectionCoin()
    {
        if (coinReactionTween == null || !coinReactionTween.IsActive())
        {
            coinReactionTween = endPosition.DOPunchScale(new Vector3(0.5f, 0.5f, 0.5f), 0.1f).SetEase(Ease.InOutElastic);
            SoundManager.Instance?.PlaySound("Coin");
        }
    }

    private void SetCoin(int value)
    {
        coin = value;
        _coinText.text = coin.ToString();
    }
}
