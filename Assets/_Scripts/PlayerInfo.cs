using System;
using UnityEngine;

[Serializable]
public class PlayerInfo
{
	[Header("Игровые параметры")]
	public int Money;

	public int LevelNumber = 1;

	[Header("Цены/Уровни в магазинах")]
	public int StartUnits = 100;

	public int Income = 100;

	public int StartUnitsLevel = 1;

	public int IncomeLevel = 1;
}
