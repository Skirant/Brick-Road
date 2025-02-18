using UnityEngine;
using YG;

public class Progress : MonoBehaviour
{
	public PlayerInfo PlayerInfo;

	public static Progress Instance { get; private set; }

	private void Awake()
	{
		if (Instance == null)
		{
			Object.DontDestroyOnLoad(base.gameObject);
			Instance = this;
			LoadProgress();
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
	}

	public void SaveProgress()
	{
		//YG2.saves.playerInfo = PlayerInfo;
		//YG2.SaveProgress();
	}

	public void LoadProgress()
	{
		//PlayerInfo = YG2.saves.playerInfo ?? new PlayerInfo();
	}
}
