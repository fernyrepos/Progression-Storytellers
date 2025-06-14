using HarmonyLib;
using Verse;

namespace ProgressionStorytellers
{
	public class ProgressionStorytellersMod : Mod
	{
		public ProgressionStorytellersMod(ModContentPack pack) : base(pack)
		{
			new Harmony("ProgressionStorytellersMod").PatchAll();
		}
	}
}