using TMPro;
using UnityEngine;

public class GateManager : MonoBehaviour
{
	[Header("UI Element")]
	public TextMeshPro GateNo;

	[Header("Gate Properties")]
	public int randomNumber;

	public bool multiply;

	[Tooltip("Отметьте, если эти ворота являются первыми.")]
	public bool isFirstGate;

	private static bool wasFirstGateNumberBelow100;

	private void Start()
	{
		if (isFirstGate)
		{
			multiply = false;
			int minInclusive = 10;
			int num = 40;
			randomNumber = 5 * Random.Range(minInclusive, num + 1);
			wasFirstGateNumberBelow100 = randomNumber < 100;
			GateNo.text = "+" + randomNumber;
			return;
		}
		multiply = Random.value > 0.5f;
		if (multiply)
		{
			randomNumber = 2;
			GateNo.text = "X" + randomNumber;
			return;
		}
		int num2 = (wasFirstGateNumberBelow100 ? 30 : 10);
		int num3 = 100;
		int minInclusive2 = num2 / 5;
		int num4 = num3 / 5;
		randomNumber = 5 * Random.Range(minInclusive2, num4 + 1);
		GateNo.text = "+" + randomNumber;
	}
}
