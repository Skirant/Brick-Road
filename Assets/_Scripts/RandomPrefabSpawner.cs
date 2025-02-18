using System.Collections.Generic;
using UnityEngine;

public class RandomPrefabSpawner : MonoBehaviour
{
	[Header("Префабы")]
	[Tooltip("Список префабов, которые нужно заспавнить.")]
	public List<GameObject> prefabs;

	[Header("Точки спавна")]
	[Tooltip("Список трансформов, на которых будут появляться объекты.")]
	public List<Transform> spawnPoints;

	[Header("Контейнер для заспавненных объектов")]
	[Tooltip("Объект (контейнер), внутрь которого будут помещаться все заспавненные объекты.")]
	public Transform spawnedObjectsContainer;

	private List<GameObject> currentPrefabsPool;

	private void Start()
	{
		if (spawnedObjectsContainer == null)
		{
			GameObject gameObject = new GameObject("SpawnedObjectsContainer");
			spawnedObjectsContainer = gameObject.transform;
		}
		currentPrefabsPool = new List<GameObject>(prefabs);
		for (int i = 0; i < spawnPoints.Count; i++)
		{
			if (currentPrefabsPool.Count == 0)
			{
				currentPrefabsPool.AddRange(prefabs);
			}
			int index = Random.Range(0, currentPrefabsPool.Count);
			Object.Instantiate(currentPrefabsPool[index], spawnPoints[i].position, spawnPoints[i].rotation, spawnedObjectsContainer);
			currentPrefabsPool.RemoveAt(index);
		}
	}
}
