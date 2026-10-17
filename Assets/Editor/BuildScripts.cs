using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Quinn.Editor
{
	public static class BuildScripts
	{
		private const string OutputRoot = "Builds";

		[MenuItem("Tools/TinyWizard/Build WebGL")]
		public static void BuildWebGL()
		{
			PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
			PlayerSettings.WebGL.decompressionFallback = true;
			PlayerSettings.WebGL.dataCaching = true;
			PlayerSettings.WebGL.initialMemorySize = 256;

			Build(BuildTarget.WebGL, BuildTargetGroup.WebGL, Path.Combine(OutputRoot, "WebGL"));
		}

		[MenuItem("Tools/TinyWizard/Build Android APK")]
		public static void BuildAndroid()
		{
			EditorUserBuildSettings.buildAppBundle = false;
			Build(BuildTarget.Android, BuildTargetGroup.Android, Path.Combine(OutputRoot, "Android", "TinyWizard.apk"));
		}

		private static void Build(BuildTarget target, BuildTargetGroup group, string location)
		{
			var options = new BuildPlayerOptions
			{
				scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
				locationPathName = location,
				target = target,
				targetGroup = group,
				options = BuildOptions.None
			};

			BuildReport report = BuildPipeline.BuildPlayer(options);
			BuildSummary summary = report.summary;

			Debug.Log($"[Build] {target}: {summary.result}, {summary.totalSize / (1024 * 1024)} MB, {summary.totalTime}, errors {summary.totalErrors}");

			if (Application.isBatchMode && summary.result != BuildResult.Succeeded)
				EditorApplication.Exit(1);
		}
	}
}
