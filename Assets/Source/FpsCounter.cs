using UnityEngine;

namespace Quinn
{
	public class FpsCounter : MonoBehaviour
	{
		[SerializeField]
		private float SmoothingTime = 0.5f;

		private float _smoothedDeltaTime;

		private readonly FrameTiming[] _frameTimings = new FrameTiming[1];

		private void Update()
		{
			_smoothedDeltaTime += (Time.unscaledDeltaTime - _smoothedDeltaTime) * (1f / SmoothingTime) * Time.unscaledDeltaTime;
		}

		private void OnGUI()
		{
			FrameTimingManager.CaptureFrameTimings();
			uint numFrames = FrameTimingManager.GetLatestTimings((uint)_frameTimings.Length, _frameTimings);

			float fps = _smoothedDeltaTime > 0f ? 1f / _smoothedDeltaTime : 0f;
			float ms = _smoothedDeltaTime * 1000f;

			string cpuMain = "-";
			string renderThread = "-";
			string gpu = "-";

			if (numFrames > 0)
			{
				cpuMain = $"{_frameTimings[0].cpuMainThreadFrameTime:F1} ms";
				renderThread = $"{_frameTimings[0].cpuRenderThreadFrameTime:F1} ms";
				gpu = $"{_frameTimings[0].gpuFrameTime:F1} ms";
			}

			var style = new GUIStyle(GUI.skin.label)
			{
				fontSize = 24,
				normal = { textColor = Color.yellow }
			};

			string text =
				$"{fps:F0} fps ({ms:F1} ms)\n" +
				$"CPU main: {cpuMain} | RT: {renderThread} | GPU: {gpu}\n" +
				$"{SystemInfo.graphicsDeviceType} | Quality: {QualitySettings.names[QualitySettings.GetQualityLevel()]} | {Screen.width}x{Screen.height}";

			GUI.Label(new Rect(10, 10, 800, 100), text, style);
		}
	}
}
