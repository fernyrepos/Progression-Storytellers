using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace ProgressionStorytellers
{
	[HarmonyPatch]
	public static class StorytellerUI_DrawStorytellerSelectionInterface_Patch
	{
		[HarmonyTargetMethods]
		public static IEnumerable<MethodBase> TargetMethods()
		{
			yield return AccessTools.Method(typeof(StorytellerUI), "DrawStorytellerSelectionInterface");
			yield return AccessTools.Method(typeof(Page_SelectStoryteller), "PreOpen");
		}
		public static StorytellerCategory selectedCategory;
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions)
		{
			var codes = codeInstructions.ToList();
			var get_AllDefsInfo = AccessTools.PropertyGetter(typeof(DefDatabase<StorytellerDef>), "AllDefs");

			foreach (var code in codes)
			{
				yield return code;
				if (code.Calls(get_AllDefsInfo))
				{
					yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(
						typeof(StorytellerUI_DrawStorytellerSelectionInterface_Patch),
						nameof(FilteredStorytellers)));
				}
			}
		}

		public static IEnumerable<StorytellerDef> FilteredStorytellers(IEnumerable<StorytellerDef> list)
		{
			if (selectedCategory == StorytellerCategory.Undefined || Find.WindowStack.IsOpen<Page_SelectStorytellerInGame>())
			{
				foreach (var c in list)
				{
					yield return c;
				}
				yield break;
			}
			
			foreach (var item in list)
			{
				var extension = item.GetModExtension<StorytellerExtension>();
				if (extension != null)
				{
					if (extension.category == selectedCategory)
					{
						yield return item;
					}
				}
				else if (selectedCategory == StorytellerCategory.Engaging)
				{
					yield return item;
				}
			}
		}
	}
}