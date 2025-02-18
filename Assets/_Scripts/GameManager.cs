using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using YG;

public class GameManager : MonoBehaviour
{
    [Header("Количество монет")]
    public int money;
    private int plusMoney;
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI plusMoneyText;

    [Header("Игровые параметры")]
    public int currentLevel = 1;
    public TextMeshProUGUI LevelText;

    [Header("Цены/Уровни в магазинах")]
    public int StartUnits = 100;
    public int Income = 100;
    public int StartUnitsLevel = 1;
    public int IncomeLevel = 1;
    public TextMeshProUGUI StartUnitsText, IncomeText, StartUnitsLevelText, IncomeLevelText;

    [Header("Реклама")]
    [SerializeField] private GameObject ImageCoinUpdatePlayer, ImageADUpdatePlayer;
    [SerializeField] private GameObject ImageCoinIncome, ImageADIncome;
    private int clickCounterStartUnits, clickCounterIncome;

    [Header("Экраны")]
    public GameObject StartPanel, WinPanel, GameOverPanel;

    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        LoadFromProgress();
    }

    private void UpdateUI()
    {
        LevelText.text = currentLevel.ToString();
        StartUnitsText.text = StartUnits.ToString();
        IncomeText.text = Income.ToString();
        moneyText.text = money.ToString();
        StartUnitsLevelText.text = StartUnitsLevel.ToString();
        IncomeLevelText.text = IncomeLevel.ToString();
    }

    public void UpgradeStartUnits()
    {
        if (money >= StartUnits && clickCounterStartUnits < 2)
        {
            money -= StartUnits;
            StartUnitsLevel++;
            StartUnits += 100;
            Progress.Instance.PlayerInfo.StartUnitsLevel = StartUnitsLevel;
            Progress.Instance.PlayerInfo.StartUnits = StartUnits;
            Progress.Instance.PlayerInfo.Money = money;
            PlayerManager.PlayerManagerInstance.BuyStickMan(1);
            clickCounterStartUnits++;
            UpdateUI();
            ToggleAdButton(ImageCoinUpdatePlayer, ImageADUpdatePlayer, clickCounterStartUnits);
            SoundManager.Instance.PlaySound("Click");
        }
    }

    public void UpgradeIncome()
    {
        if (money >= Income && clickCounterIncome < 2)
        {
            money -= Income;
            IncomeLevel++;
            Income += 100;
            Progress.Instance.PlayerInfo.IncomeLevel = IncomeLevel;
            Progress.Instance.PlayerInfo.Income = Income;
            Progress.Instance.PlayerInfo.Money = money;
            clickCounterIncome++;
            UpdateUI();
            ToggleAdButton(ImageCoinIncome, ImageADIncome, clickCounterIncome);
            SoundManager.Instance.PlaySound("Click");
        }
    }

    private void ToggleAdButton(GameObject coinImage, GameObject adImage, int counter)
    {
        if (counter >= 2)
        {
            coinImage.SetActive(false);
            adImage.SetActive(true);
        }
    }

    public void StartGame()
    {
        StartPanel.SetActive(false);
        PlayerManager.PlayerManagerInstance.StartGame();
        print("klick");
    }

    public void WinGame() => WinPanel.SetActive(true);
    public void GameOver() => GameOverPanel.SetActive(true);

    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        SoundManager.Instance.PlaySound("Click");
    }

    public void NextLevel()
    {
        currentLevel++;
        Progress.Instance.PlayerInfo.LevelNumber = currentLevel;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        SoundManager.Instance.PlaySound("Click");
    }

    private void LoadFromProgress()
    {
        var playerInfo = Progress.Instance.PlayerInfo;
        money = playerInfo.Money;
        currentLevel = playerInfo.LevelNumber;
        IncomeLevel = playerInfo.IncomeLevel;
        Income = playerInfo.Income;
        StartUnitsLevel = playerInfo.StartUnitsLevel;
        StartUnits = playerInfo.StartUnits;
        UpdateUI();
    }

    public void CoinManager(int coinsBad)
    {
        plusMoney = coinsBad + (int)(coinsBad * (currentLevel * IncomeLevel * 5f / 100f));
        Progress.Instance.PlayerInfo.Money += plusMoney;
        plusMoneyText.text = "+ " + plusMoney;
        UpdateUI();
    }
}
