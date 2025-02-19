using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using YG;  // Подключаем пространство имён YG2 для рекламы

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Количество монет")]
    public int money = 0;  // Текущее количество монет
    private int plusMoney = 0;
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI plusMoneyText;

    [Header("Игровые параметры")]
    public int currentLevel = 1;  // Текущий уровень
    public TextMeshProUGUI LevelText;

    [Header("Цены/Уровни в магазинах")]
    public int StartUnits = 100; // Цена одного человечка
    public TextMeshProUGUI StartUnitsText;
    public int Income = 100;     // Цена прокачки получения монет 
    public TextMeshProUGUI IncomeText;
    public int StartUnitsLevel = 1;
    public TextMeshProUGUI StartUnitsLevelText;
    public int IncomeLevel = 1;
    public TextMeshProUGUI IncomeLevelText;

    [Header("Реклама")]
    [SerializeField] private GameObject ImageCoinUpdatePlayer;
    [SerializeField] private GameObject ImageADUpdatePlayer;
    [SerializeField] private GameObject ImageCoinIncome;
    [SerializeField] private GameObject ImageADIncome;

    // Cчётчики нажатий на покупку (после первого требуется реклама)
    private int clickCounterStartUnits = 0;
    private int clickCounterIncome = 0;

    [Header("Экраны")]
    public GameObject StartPanel;
    public GameObject WinPanel;
    public GameObject GameOverPanel;

    // --------------------------------------------------------------------------------
    //                          ЖИЗНЕННЫЙ ЦИКЛ
    // --------------------------------------------------------------------------------

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        UpdateUI();
    }

    private void Start()
    {
        LoadFromProgress();
    }

    // --------------------------------------------------------------------------------
    //                          ОБНОВЛЕНИЕ ИГРОВОЙ UI
    // --------------------------------------------------------------------------------

    /// <summary>
    /// Обновляет всю UI-информацию (монеты, уровни, цены и т.д.).
    /// </summary>
    private void UpdateUI()
    {
        LevelText.text = currentLevel.ToString();
        StartUnitsText.text = StartUnits.ToString();
        IncomeText.text = Income.ToString();
        moneyText.text = money.ToString();
        StartUnitsLevelText.text = StartUnitsLevel.ToString();
        IncomeLevelText.text = IncomeLevel.ToString();
    }

    // --------------------------------------------------------------------------------
    //                          УЛУЧШЕНИЯ С ПЕРВЫМ ПОКУПКОЙ И ДАЛЕЕ РЕКЛАМОЙ
    // --------------------------------------------------------------------------------

    /// <summary>
    /// Улучшение стартовых юнитов.
    /// Первый раз — покупка за монеты,
    /// далее — только за просмотр рекламы (rewarded).
    /// </summary>
    public void UpgradeStartUnits()
    {
        // --- Покупка за монеты (первый раз) ---
        if (money >= StartUnits && clickCounterStartUnits < 2)
        {
            money -= StartUnits;
            StartUnitsLevel++;
            StartUnits += 100;

            Progress.Instance.PlayerInfo.StartUnitsLevel = StartUnitsLevel;
            Progress.Instance.PlayerInfo.StartUnits = StartUnits;
            Progress.Instance.PlayerInfo.Money = money;

            // Покупаем юнита:
            PlayerManager.PlayerManagerInstance.BuyStickMan(1);

            clickCounterStartUnits++;
            UpdateUI();
            if (clickCounterStartUnits >= 2)
            {
                // Скрываем иконку монет, показываем иконку рекламы
                ImageCoinUpdatePlayer.SetActive(false);
                ImageADUpdatePlayer.SetActive(true);
            }

            SoundManager.Instance.PlaySound("Click");
        }
        // --- Второй и последующие разы (реклама) ---
        else if (clickCounterStartUnits >= 2)
        {
            // ID вознаграждения для рекламы (должен быть уникальным для этого типа апгрейда)
            string rewardID = "UpgradeStartUnits";

            // Вызываем рекламу с коллбэком. Вознаграждение даём только после полного просмотра.
            YG2.RewardedAdvShow(rewardID, () =>
            {
                // Если ролик досмотрели до конца, выдаём вознаграждение
                StartUnitsLevel++;
                StartUnits += 100;

                PlayerManager.PlayerManagerInstance.BuyStickMan(1);

                // Сохраняем данные
                Progress.Instance.PlayerInfo.StartUnitsLevel = StartUnitsLevel;
                Progress.Instance.PlayerInfo.StartUnits = StartUnits;
                Progress.Instance.PlayerInfo.Money = money;

                SoundManager.Instance.PlaySound("Click");

                UpdateUI();
            });
        }

        YG2.SaveProgress();
    }

    /// <summary>
    /// Улучшение дохода.
    /// Первый раз — покупка за монеты,
    /// далее — только за просмотр рекламы (rewarded).
    /// </summary>
    public void UpgradeIncome()
    {
        // --- Покупка за монеты (первый раз) ---
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

            if (clickCounterIncome >= 2)
            {
                // Скрываем иконку монет, показываем иконку рекламы
                ImageCoinIncome.SetActive(false);
                ImageADIncome.SetActive(true);
            }

            SoundManager.Instance.PlaySound("Click");
        }
        // --- Второй и последующие разы (реклама) ---
        else if (clickCounterIncome >= 2)
        {
            // ID вознаграждения для рекламы (должен быть уникальным для этого типа апгрейда)
            string rewardID = "UpgradeIncome";

            // Показываем рекламный ролик, выдаём награду только при полном просмотре
            YG2.RewardedAdvShow(rewardID, () =>
            {
                // Вознаграждение за просмотр
                IncomeLevel++;
                Income += 100;

                Progress.Instance.PlayerInfo.IncomeLevel = IncomeLevel;
                Progress.Instance.PlayerInfo.Income = Income;
                Progress.Instance.PlayerInfo.Money = money;

                SoundManager.Instance.PlaySound("Click");

                UpdateUI();
            });
        }

        YG2.SaveProgress();
    }

    // --------------------------------------------------------------------------------
    //                          РАЗНЫЕ КНОПКИ И УПРАВЛЕНИЕ УРОВНЯМИ
    // --------------------------------------------------------------------------------

    /// <summary>
    /// Начало игры — скрываем стартовую панель.
    /// </summary>
    public void StartGame()
    {
        StartPanel.SetActive(false);
        PlayerManager.PlayerManagerInstance.StartGame();
    }

    /// <summary>
    /// Вызывается при победе.
    /// </summary>
    public void WinGame()
    {
        currentLevel++;
        Progress.Instance.PlayerInfo.LevelNumber = currentLevel;

        YG2.SaveProgress();
        WinPanel.SetActive(true);
    }

    /// <summary>
    /// Перезапуск текущего уровня.
    /// </summary>
    public void ResetartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

        SoundManager.Instance.PlaySound("Click");
    }

    /// <summary>
    /// Переход на следующий уровень.
    /// </summary>
    public void NextLevel()
    {        
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

        SoundManager.Instance.PlaySound("Click");
    }

    /// <summary>
    /// Проигрыш — показываем экран конца игры.
    /// </summary>
    public void GameOver()
    {
        GameOverPanel.SetActive(true);
    }

    // --------------------------------------------------------------------------------
    //                          ЗАГРУЗКА / СОХРАНЕНИЕ ПРОГРЕССА
    // --------------------------------------------------------------------------------

    /// <summary>
    /// Загружаем данные из локального прогресса (Progress.Instance).
    /// </summary>
    private void LoadFromProgress()
    {
        money = Progress.Instance.PlayerInfo.Money;
        currentLevel = Progress.Instance.PlayerInfo.LevelNumber;
        IncomeLevel = Progress.Instance.PlayerInfo.IncomeLevel;
        Income = Progress.Instance.PlayerInfo.Income;
        StartUnitsLevel = Progress.Instance.PlayerInfo.StartUnitsLevel;
        StartUnits = Progress.Instance.PlayerInfo.StartUnits;
        UpdateUI();
    }

    // --------------------------------------------------------------------------------
    //                          УПРАВЛЕНИЕ МОНЕТАМИ В БОЮ
    // --------------------------------------------------------------------------------

    /// <summary>
    /// Начисляем монеты за врагов (или другие действия) с учётом уровня и IncomeLevel.
    /// </summary>
    /// <param name="coinsBad">Базовая награда, далее умножаем на бонус.</param>
    public void CoinManager(int coinsBad)
    {
        // Формула доп. бонуса: (coinsBad * (currentLevel * IncomeLevel * 5) / 100)
        plusMoney = coinsBad + (int)(coinsBad * (((float)currentLevel * (float)IncomeLevel * 5) / 100));

        int startMoney = Progress.Instance.PlayerInfo.Money; // Текущее число монет

        // Запускаем анимацию сбора монет
        CollectingCoin coinCollector = FindFirstObjectByType<CollectingCoin>();
        if (coinCollector != null)
        {
            coinCollector.CollectCoinsWithVisual(startMoney, plusMoney); // Передаем стартовые монеты и выигрыш
        }

        Progress.Instance.PlayerInfo.Money = startMoney + plusMoney;
        plusMoneyText.text = "+ " + plusMoney.ToString();

        YG2.SaveProgress();

        UpdateUI();
    }
}
