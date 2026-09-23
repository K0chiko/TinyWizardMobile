namespace Quinn
{
	public static class SoundMaterialTypeNames
	{
		private static readonly string[] s_Labels =
		{
			nameof(SoundMaterialType.None),
			nameof(SoundMaterialType.Stone),
			nameof(SoundMaterialType.Carpet),
			nameof(SoundMaterialType.HealingPuddle),
			nameof(SoundMaterialType.Snow)
		};

		public static string ToLabel(SoundMaterialType mat) => s_Labels[(int)mat];
	}
}
