using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DynamicResolution : MonoBehaviour
{
	private UniversalRenderPipelineAsset urp;

	[SerializeField]
	private float maxDPI = 0.95f;

	[SerializeField]
	private float minDPI = 0.55f;

	[SerializeField]
	private float dampen = 0.02f;

	[SerializeField]
	private float maxFps = 75f;

	[SerializeField]
	private float minFps = 55f;

	[SerializeField]
	private float renderScale = 1f;

	[SerializeField]
	private float refreshResolutionTime = 1f;

	private float timer;

	private float deltaTime;

	private void Start()
	{
		urp = (UniversalRenderPipelineAsset)GraphicsSettings.currentRenderPipeline;
		urp.renderScale = renderScale;
	}

	private void Update()
	{
		timer -= Time.deltaTime;
		if (timer < 0f)
		{
			ResolutionUpdate();
			timer = refreshResolutionTime;
		}
	}

	private void ResolutionUpdate()
	{
		float fps = GetFps();
		if (maxFps < fps)
		{
			ImproveResolution();
		}
		if (minFps > fps)
		{
			SubtructResolution();
		}
		renderScale = urp.renderScale;
	}

	private void ImproveResolution()
	{
		if (renderScale < maxDPI)
		{
			urp.renderScale += dampen;
		}
	}

	private void SubtructResolution()
	{
		if (renderScale > minDPI)
		{
			urp.renderScale -= dampen;
		}
	}

	private float GetFps()
	{
		deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
		return 1f / deltaTime;
	}
}
