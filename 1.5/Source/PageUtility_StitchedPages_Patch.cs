using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;

namespace ProgressionStorytellers
{
	[HarmonyPatch(typeof(PageUtility), nameof(PageUtility.StitchedPages))]
	public static class PageUtility_StitchedPages_Patch
	{
		public static void Prefix(ref IEnumerable<Page> pages)
		{
			var list = pages.ToList();
			var selectStoryteller = list.FindIndex(x => x is Page_SelectStoryteller);
			list.RemoveAt(selectStoryteller);
			var worldParams = list.OfType<Page_CreateWorldParams>().First();
			list.Insert(selectStoryteller, new Page_SelectStorytellerCategory(worldParams));
			pages = list;
		}
	}
}