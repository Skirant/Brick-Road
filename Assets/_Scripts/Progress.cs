using System;
using UnityEngine;
using YG;

[Serializable]
public class PlayerInfo
{
    [Header("Игровые параметры")]
    public int Money = 0;
    public int LevelNumber = 1;

    [Header("Цены/Уровни в магазинах")]
    public int StartUnits = 100;
    public int Income = 100;
    public int StartUnitsLevel = 1;
    public int IncomeLevel = 1;
}

public class Progress : MonoBehaviour
{
    public static Progress Instance { get; private set; }
    public PlayerInfo PlayerInfo;

    private void Awake()
    {
        if (Instance == null)
        {
            DontDestroyOnLoad(gameObject);
            Instance = this;
            LoadProgress(); // Загружаем данные при запуске
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Метод сохранения прогресса
    public void SaveProgress()
    {
        YG2.saves.playerInfo = PlayerInfo;
        YG2.SaveProgress();
    }

    // Метод загрузки прогресса
    public void LoadProgress()
    {
        PlayerInfo = YG2.saves.playerInfo ?? new PlayerInfo();
    }
}
