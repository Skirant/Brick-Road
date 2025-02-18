using UnityEngine;

public class rotatAnim : MonoBehaviour
{
	public float rotationSpeed = 360f;

	private void Update()
	{
		base.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.Self);
	}
}
