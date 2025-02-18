using TMPro;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
	[SerializeField]
	public TextMeshPro CounterText;

	[SerializeField]
	private GameObject stickMan;

	[Range(0f, 1f)]
	[SerializeField]
	private float DistanceFactor;

	[Range(0f, 1f)]
	[SerializeField]
	private float Radius;

	public float radius;

	public float angle;

	public Transform enemy;

	public bool attack;

	private void Start()
	{
		SpawnStickMen();
		UpdateCounterText();
		FormatStickMan();
	}

	private void Update()
	{
		if (attack && base.transform.childCount > 1)
		{
			HandleAttack();
		}
	}

	private void FormatStickMan()
	{
		for (int i = 0; i < base.transform.childCount; i++)
		{
			Vector3 stickManLocalPosition = GetStickManLocalPosition(i);
			base.transform.GetChild(i).localPosition = stickManLocalPosition;
		}
	}

	private Vector3 GetStickManLocalPosition(int index)
	{
		float x = DistanceFactor * Mathf.Sqrt(index) * Mathf.Cos((float)index * Radius);
		float z = DistanceFactor * Mathf.Sqrt(index) * Mathf.Sin((float)index * Radius);
		return new Vector3(x, 0f, z);
	}

	private void HandleAttack()
	{
		new Vector3(enemy.position.x, base.transform.position.y, enemy.position.z);
		Vector3 forward = enemy.position - base.transform.position;
		for (int i = 0; i < base.transform.childCount; i++)
		{
			Transform child = base.transform.GetChild(i);
			child.rotation = Quaternion.Slerp(child.rotation, Quaternion.LookRotation(forward, Vector3.up), Time.deltaTime * 3f);
			if (enemy.childCount > 1 && i < enemy.GetChild(1).childCount && (enemy.GetChild(1).GetChild(i).position - child.position).magnitude < 5f)
			{
				child.position = Vector3.Lerp(child.position, enemy.GetChild(1).GetChild(i).position, Time.deltaTime * 2f);
			}
		}
		if (base.transform.childCount <= 1)
		{
			Debug.Log("Конец игры");
			StopAttacking();
		}
	}

	public void AttackThem(Transform enemyForce)
	{
		enemy = enemyForce;
		attack = true;
		for (int i = 0; i < base.transform.childCount; i++)
		{
		}
	}

	public void StopAttacking()
	{
		PlayerManager.PlayerManagerInstance.gameState = (attack = false);
		for (int i = 0; i < base.transform.childCount; i++)
		{
		}
	}

	private void SpawnStickMen()
	{
		int num = Random.Range(20, 50);
		for (int i = 0; i < num; i++)
		{
			Object.Instantiate(stickMan, base.transform.position, new Quaternion(0f, 180f, 0f, 1f), base.transform);
		}
	}

	public void UpdateCounterText()
	{
		int childCount = base.transform.childCount;
		CounterText.text = childCount.ToString();
	}
}
