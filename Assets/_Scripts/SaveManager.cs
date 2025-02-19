using System.Collections.Generic;
using UnityEngine;
using YG;

public class SaveManager : MonoBehaviour
{
	private List<KeyCode> keySequence = new List<KeyCode>
	{
		KeyCode.Alpha2,
		KeyCode.Alpha4,
		KeyCode.Alpha8,
		KeyCode.Alpha8
	};

	private int keyIndex;

	private void Update()
	{
		if (!Input.anyKeyDown)
		{
			return;
		}
		if (Input.GetKeyDown(keySequence[keyIndex]))
		{
			keyIndex++;
			if (keyIndex >= keySequence.Count)
			{
				ResetSaves();
				keyIndex = 0;
			}
		}
		else
		{
			keyIndex = 0;
		}
	}

	private void ResetSaves()
	{
		Debug.Log("Сохранения удалены и перезаписаны.");
		YG2.SetDefaultSaves();
		YG2.SaveProgress();
	}
}
