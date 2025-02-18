using TMPro;
using UnityEngine;

public class WallNumbering : MonoBehaviour
{
	public GameObject[] walls;

	private void Start()
	{
		for (int i = 0; i < walls.Length; i++)
		{
			TextMeshPro componentInChildren = walls[i].GetComponentInChildren<TextMeshPro>();
			if (componentInChildren != null)
			{
				componentInChildren.text = "#" + (i + 1);
			}
		}
	}
}
