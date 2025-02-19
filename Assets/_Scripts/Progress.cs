using System;
using UnityEngine;
using YG;

[Serializable]
public class PlayerInfo
{
    [Header("������� ���������")]
    public int Money = 0;
    public int LevelNumber = 1;

    [Header("����/������ � ���������")]
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
            LoadProgress(); // ��������� ������ ��� �������
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ����� ���������� ���������
    public void SaveProgress()
    {
        YG2.saves.playerInfo = PlayerInfo;
        YG2.SaveProgress();
    }

    // ����� �������� ���������
    public void LoadProgress()
    {
        PlayerInfo = YG2.saves.playerInfo ?? new PlayerInfo();
    }
}
