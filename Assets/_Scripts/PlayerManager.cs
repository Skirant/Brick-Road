using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using Unity.Mathematics;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
	[Header("Основные ссылки и переменные")]
	public Transform player;

	[SerializeField]
	private TextMeshPro CounterText;

	[SerializeField]
	private GameObject CounterLabel;

	[SerializeField]
	private GameObject stickMan;

	[Range(0f, 1f)]
	[SerializeField]
	private float DistanceFactor = 0.2f;

	[Range(0f, 1f)]
	[SerializeField]
	private float Radius = 0.5f;

	[Header("Скорости движения")]
	public float playerSpeed = 5f;

	public float idelSpeed = 2f;

	public float fightSpeed = 1f;

	[HideInInspector]
	public float roadSpeed;

	[Header("Ссылки на Road, Enemy и т.д.")]
	[SerializeField]
	private Transform road;

	[SerializeField]
	private Transform enemy;

	private int numberOfStickmans;

	private int numberOfEnemyStickmans;

	private Camera mainCamera;

	private Vector3 mouseStartPos;

	private Vector3 playerStartPos;

	private bool moveByTouch;

	private bool attack;

	public bool gameState;

	[HideInInspector]
	public Vector3 cachedEndPosition;

	public bool isEndPositionDirty;

	private bool isGameOver;

	[Header("Скорость ускорения и проверка на одноразовость")]
	[SerializeField]
	private float speedMultiplier = 2f;

	private bool speedBust = true;

	private HashSet<stickManManager> jumpers = new HashSet<stickManManager>();

	[Header("Границы от человечков")]
	[SerializeField]
	private float distance50;

	[SerializeField]
	private float distance300;

	[SerializeField]
	private float centerMoveDuration = 1f;

	[SerializeField]
	private float slowDownRate = 1f;

	private bool isMovingToCenterEndGame;

	private float endGameCenterTimer;

	private float centerMoveTimer;

	private bool isMovingToCenter;

	private bool trigerEndGame;

	[Header("Тест")]
	public int testMankeStikmants;

	private float lastGateBonusTime = -1f;

	private float gateBonusCooldown = 1f;

	public static PlayerManager PlayerManagerInstance { get; private set; }

	private void Awake()
	{
		if (PlayerManagerInstance == null)
		{
			PlayerManagerInstance = this;
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	private void Start()
	{
		player = base.transform;
		numberOfStickmans = player.childCount - 1;
		CounterText.text = numberOfStickmans.ToString();
		mainCamera = Camera.main;
		gameState = true;
		MakeStickMan(testMankeStikmants + Progress.Instance.PlayerInfo.StartUnitsLevel);
	}

	private void Update()
	{
		Attack();
		if (gameState)
		{
			road.Translate(-road.forward * Time.deltaTime * roadSpeed);
		}
		if (numberOfStickmans <= 5)
		{
			GameOver();
		}
		if (isMovingToCenterEndGame)
		{
			endGameCenterTimer += Time.deltaTime;
			float t = Mathf.Clamp01(endGameCenterTimer / centerMoveDuration);
			float x = Mathf.Lerp(base.transform.position.x, 0f, t);
			base.transform.position = new Vector3(x, base.transform.position.y, base.transform.position.z);
			playerSpeed = 0f;
		}
	}

	private void LateUpdate()
	{
		if (gameState)
		{
			float min = ((numberOfStickmans < 250) ? (0f - distance50) : (0f - distance300));
			float max = ((numberOfStickmans < 250) ? distance50 : distance300);
			Vector3 position = base.transform.position;
			position.x = Mathf.Clamp(position.x, min, max);
			base.transform.position = position;
		}
	}

	private void Attack()
	{
		if (attack)
		{
			roadSpeed = Mathf.Lerp(10f, 1f, Time.deltaTime * slowDownRate);
			if (isMovingToCenter)
			{
				centerMoveTimer += Time.deltaTime;
				float num = Mathf.Clamp01(centerMoveTimer / centerMoveDuration);
				float x = Mathf.Lerp(base.transform.position.x, 0f, num);
				base.transform.position = new Vector3(x, base.transform.position.y, base.transform.position.z);
				if (num >= 1f)
				{
					isMovingToCenter = false;
				}
			}
			Vector3 forward = new Vector3(enemy.position.x, base.transform.position.y, enemy.position.z) - base.transform.position;
			for (int i = 1; i < base.transform.childCount; i++)
			{
				base.transform.GetChild(i).rotation = Quaternion.Slerp(base.transform.GetChild(i).rotation, Quaternion.LookRotation(forward, Vector3.up), Time.deltaTime * 3f);
			}
			if ((float)enemy.GetChild(1).childCount > 1f)
			{
				for (int j = 0; j < base.transform.childCount; j++)
				{
					if (j < enemy.GetChild(1).childCount && (enemy.GetChild(1).GetChild(j).position - base.transform.GetChild(j).position).magnitude < 1f)
					{
						base.transform.GetChild(j).position = Vector3.Lerp(base.transform.GetChild(j).position, new Vector3(enemy.GetChild(1).GetChild(j).position.x, base.transform.GetChild(j).position.y, enemy.GetChild(1).GetChild(j).position.z), Time.deltaTime * 5f);
					}
				}
			}
			else
			{
				roadSpeed = idelSpeed;
				attack = false;
				FormatStickMan();
				for (int k = 1; k < base.transform.childCount; k++)
				{
					base.transform.GetChild(k).rotation = Quaternion.identity;
				}
				enemy.gameObject.SetActive(value: false);
				isEndPositionDirty = true;
			}
			if (base.transform.childCount == 1)
			{
				enemy.transform.GetChild(1).GetComponent<EnemyManager>().StopAttacking();
				base.gameObject.SetActive(value: false);
			}
		}
		else
		{
			MoveThePlayer();
		}
	}

	private void MoveThePlayer()
	{
		if (Input.GetMouseButtonDown(0) && gameState)
		{
			moveByTouch = true;
			Plane plane = new Plane(Vector3.up, 0f);
			Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
			if (plane.Raycast(ray, out var enter))
			{
				mouseStartPos = ray.GetPoint(enter + 1f);
				playerStartPos = base.transform.position;
			}
		}
		if (Input.GetMouseButtonUp(0))
		{
			moveByTouch = false;
		}
		if (!moveByTouch)
		{
			return;
		}
		Plane plane2 = new Plane(Vector3.up, 0f);
		Ray ray2 = mainCamera.ScreenPointToRay(Input.mousePosition);
		if (plane2.Raycast(ray2, out var enter2))
		{
			Vector3 vector = ray2.GetPoint(enter2 + 1f) - mouseStartPos;
			Vector3 vector2 = playerStartPos + vector;
			if (numberOfStickmans < 250)
			{
				vector2.x = Mathf.Clamp(vector2.x, 0f - distance50, distance50);
			}
			else if (numberOfStickmans > 250)
			{
				vector2.x = Mathf.Clamp(vector2.x, 0f - distance300, distance300);
			}
			base.transform.position = new Vector3(Mathf.Lerp(base.transform.position.x, vector2.x, Time.deltaTime * playerSpeed), base.transform.position.y, base.transform.position.z);
		}
	}

	public void FormatStickMan()
	{
		if (player.childCount - 1 <= 0)
		{
			return;
		}
		float radius = Radius;
		float num = 10f;
		int num2 = 8;
		int num3 = 0;
		int num4 = 0;
		for (int i = 1; i < player.childCount; i++)
		{
			if (num4 >= num2 * (num3 + 1))
			{
				num3++;
				num2 += 4;
			}
			float num5 = radius + (float)num3 * DistanceFactor;
			float f = (360f / (float)num2 * (float)num4 + UnityEngine.Random.Range(0f - num, num)) * ((float)Math.PI / 180f);
			float x = Mathf.Cos(f) * num5;
			float z = Mathf.Sin(f) * num5;
			player.transform.GetChild(i).DOLocalMove(new Vector3(x, 0f, z), 0.5f).SetEase(Ease.OutBack);
			num4++;
		}
	}

	public void MakeStickMan(int number)
	{
		for (int i = numberOfStickmans; i < number; i++)
		{
			UnityEngine.Object.Instantiate(stickMan, base.transform.position, quaternion.identity, base.transform);
		}
		numberOfStickmans = base.transform.childCount - 1;
		CounterText.text = numberOfStickmans.ToString();
		FormatStickMan();
		isEndPositionDirty = true;
	}

	public Vector3 GetEndOfCrowdPosition()
	{
		if (isEndPositionDirty)
		{
			if (base.transform.childCount <= 1)
			{
				cachedEndPosition = base.transform.position;
			}
			else
			{
				cachedEndPosition = base.transform.GetChild(1).position;
				for (int i = 1; i < base.transform.childCount; i++)
				{
					if (base.transform.GetChild(i).position.z < cachedEndPosition.z)
					{
						cachedEndPosition.z = base.transform.GetChild(i).position.z;
					}
				}
			}
			isEndPositionDirty = false;
		}
		return cachedEndPosition;
	}

	public void RemoveStickMen(int count)
	{
		int num = Mathf.Min(count, base.transform.childCount - 1);
		List<Transform> list = new List<Transform>(num);
		for (int i = 0; i < num; i++)
		{
			if (i + 1 < base.transform.childCount)
			{
				list.Add(base.transform.GetChild(i + 1));
			}
		}
		foreach (Transform item in list)
		{
			if (item != null)
			{
				UnityEngine.Object.Destroy(item.gameObject);
			}
		}
		StartCoroutine(UpdateStickManCountDelayed());
	}

	private IEnumerator UpdateStickManCountDelayed()
	{
		yield return new WaitForSeconds(0.1f);
		UpdateStickManCount();
	}

	public void UpdateStickManCount()
	{
		numberOfStickmans = base.transform.childCount - 1;
		CounterText.text = numberOfStickmans.ToString();
	}

	public void OnStickmanLanded(stickManManager stickman)
	{
		jumpers.Remove(stickman);
		if (jumpers.Count == 0)
		{
			FormatStickMan();
		}
	}

	public void OnStickmanHitJump(stickManManager stickman)
	{
		jumpers.Add(stickman);
		stickman.DoJump();
	}

	private void GameOver()
	{
		if (isGameOver)
		{
			return;
		}
		UpdateStickManCount();
		if (numberOfStickmans <= 0)
		{
			isGameOver = true;
			roadSpeed = 0f;
			CounterLabel.SetActive(value: false);
			if (trigerEndGame)
			{
				WinGame();
			}
			else
			{
				GameManager.Instance.GameOver();
			}
		}
	}

	public int GetStickmanCount()
	{
		return base.transform.childCount - 1;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Gate"))
		{
			if (Time.time - lastGateBonusTime < gateBonusCooldown)
			{
				return;
			}
			lastGateBonusTime = Time.time;
			GateManager component = other.GetComponent<GateManager>();
			int stickmanCount = GetStickmanCount();
			int num = (component.multiply ? (stickmanCount * component.randomNumber) : (stickmanCount + component.randomNumber)) - stickmanCount;
			if (num > 0)
			{
				MakeStickMan(GetStickmanCount() + num);
			}
			SoundManager.Instance.PlaySound("passageGate");
		}
		if (other.CompareTag("Enemy"))
		{
			enemy = other.transform;
			attack = true;
			isMovingToCenter = true;
			centerMoveTimer = 0f;
			GetStickmanCount();
			other.transform.GetChild(1).GetComponent<EnemyManager>().AttackThem(base.transform);
		}
		if (other.CompareTag("TrigerEndGame") && speedBust)
		{
			if (speedBust)
			{
				roadSpeed *= speedMultiplier;
				speedBust = false;
			}
			isMovingToCenterEndGame = true;
			endGameCenterTimer = 0f;
			trigerEndGame = true;
		}
		if (other.CompareTag("TrigerOpenChest"))
		{
			roadSpeed = 0f;
			CounterLabel.SetActive(value: false);
			WinGame();
		}
		if (other.CompareTag("Jump"))
		{
			SoundManager.Instance.PlaySound("Jump");
		}
	}

	public void BuyStickMan(int number)
	{
		MakeStickMan(GetStickmanCount() + number);
	}

	public void StartGame()
	{
		roadSpeed = idelSpeed;
		playerSpeed = idelSpeed;
	}

	public void WinGame()
	{
		GameManager.Instance.WinGame();
		int coinsBad = 100 + (int)(100f * ((float)numberOfStickmans / 100f));
		GameManager.Instance.CoinManager(coinsBad);
		SoundManager.Instance.PlaySound("Win");
	}
}
