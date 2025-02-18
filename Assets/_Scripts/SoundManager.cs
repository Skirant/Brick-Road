using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
	[Serializable]
	public class Sound
	{
		public string soundName;

		public AudioClip audioClip;

		[Range(0f, 1f)]
		public float volume = 1f;

		public bool preventOverlap;

		[HideInInspector]
		public bool isPlaying;
	}

	public static SoundManager Instance;

	[Header("Список звуковых эффектов")]
	public List<Sound> sounds;

	private Dictionary<string, Sound> soundDictionary;

	private Dictionary<string, AudioSource> activeSources;

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		Instance = this;
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		soundDictionary = new Dictionary<string, Sound>();
		activeSources = new Dictionary<string, AudioSource>();
		foreach (Sound sound in sounds)
		{
			if (!soundDictionary.ContainsKey(sound.soundName))
			{
				soundDictionary.Add(sound.soundName, sound);
			}
			else
			{
				Debug.LogWarning("Дублирующееся имя звука: " + sound.soundName);
			}
		}
	}

	public void PlaySound(string soundName)
	{
		if (soundDictionary.ContainsKey(soundName))
		{
			Sound sound = soundDictionary[soundName];
			if (!sound.preventOverlap || !sound.isPlaying)
			{
				GameObject obj = new GameObject("Sound_" + soundName);
				AudioSource audioSource = obj.AddComponent<AudioSource>();
				audioSource.clip = sound.audioClip;
				audioSource.volume = sound.volume;
				audioSource.Play();
				sound.isPlaying = true;
				activeSources[soundName] = audioSource;
				UnityEngine.Object.Destroy(obj, sound.audioClip.length);
				StartCoroutine(ResetSoundStatus(sound, sound.audioClip.length));
			}
		}
		else
		{
			Debug.LogWarning("Звук '" + soundName + "' не найден!");
		}
	}

	private IEnumerator ResetSoundStatus(Sound sound, float duration)
	{
		yield return new WaitForSeconds(duration);
		sound.isPlaying = false;
	}

	public void SetSoundVolume(string soundName, float volume)
	{
		if (soundDictionary.ContainsKey(soundName))
		{
			soundDictionary[soundName].volume = Mathf.Clamp(volume, 0f, 1f);
			if (activeSources.ContainsKey(soundName))
			{
				activeSources[soundName].volume = soundDictionary[soundName].volume;
			}
		}
		else
		{
			Debug.LogWarning("Звук '" + soundName + "' не найден для изменения громкости!");
		}
	}

	public float GetSoundVolume(string soundName)
	{
		if (soundDictionary.ContainsKey(soundName))
		{
			return soundDictionary[soundName].volume;
		}
		Debug.LogWarning("Звук '" + soundName + "' не найден для получения громкости!");
		return -1f;
	}
}
