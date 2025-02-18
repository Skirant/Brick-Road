using System.Collections;
using UnityEngine;

public class Destroy : MonoBehaviour
{
	private IEnumerator Start()
	{
		yield return new WaitForSecondsRealtime(1.5f);
		Object.Destroy(base.gameObject);
	}
}
