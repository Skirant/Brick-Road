using UnityEngine;

public class WallColorRandom : MonoBehaviour
{
	private void Start()
	{
		Renderer component = GetComponent<Renderer>();
		if (component != null)
		{
			Material material = component.material;
			Color color = material.color;
			Color color2 = GenerateBrightColor();
			color2.a = color.a;
			material.color = color2;
		}
	}

	private Color GenerateBrightColor()
	{
		float h = Random.Range(0f, 1f);
		float s = Random.Range(0.7f, 1f);
		float v = Random.Range(0.7f, 1f);
		return Color.HSVToRGB(h, s, v);
	}
}
