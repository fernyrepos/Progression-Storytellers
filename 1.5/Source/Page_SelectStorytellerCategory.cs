using System;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace ProgressionStorytellers
{
	[HotSwappable]
	[StaticConstructorOnStartup]
	public class Page_SelectStorytellerCategory : Page
	{
		public override string PageTitle => "What kind of story would interest you?";
		private readonly StorytellerCategory[] storytellerCategories = Enum.GetValues(typeof(StorytellerCategory))
		.Cast<StorytellerCategory>().Where(x => x != StorytellerCategory.Undefined).ToArray();
		private Page_CreateWorldParams nextPage;
		public Page_SelectStorytellerCategory(Page_CreateWorldParams nextPage)
		{
			this.nextPage = nextPage;
		}

		public override void DoWindowContents(Rect rect)
		{
			Text.Anchor = TextAnchor.UpperCenter;
			rect.y += 45;
			rect.height -= 45;
			Text.Font = GameFont.Medium;

			var fontIndex = 2;
			var oldStyle = Text.fontStyles[fontIndex];
			var newStyle = new GUIStyle(oldStyle);
			Text.fontStyles[fontIndex] = newStyle;
			newStyle.fontSize = 20;
			Widgets.Label(new Rect(rect.x, rect.y - 30, rect.width, 30f), PageTitle);
			Text.fontStyles[fontIndex] = oldStyle;
			Text.Font = GameFont.Small;
			Text.Anchor = TextAnchor.MiddleCenter;

			Widgets.BeginGroup(rect);
			var scale = 0.9f;
			float boxWidth = 300 * scale;
			float boxHeight = 350 * scale;
			float xSpacing = 60;
			float ySpacing = 15;

			Text.Font = GameFont.Medium;
			var yOffset = 55 - 50;
			float boxScale = 30;
			int columns = 3;
			for (int i = 0; i < storytellerCategories.Length; i++)
			{
				int row = i / columns;
				int column = i % columns;
				float xOffset = (row == 1) ? 20 : 0;
				if (row == 1)
				{
					xSpacing = 40;
					boxHeight = 300 * scale;
					yOffset = 110 - 50;
					boxScale = 45;
				}
				Rect boxRect = new Rect(column * (boxWidth + xSpacing) + 20 + xOffset, 
				yOffset + row * (boxHeight + ySpacing) + 10, boxWidth, boxHeight);
				DrawStorytellerCategoryBox(boxRect, storytellerCategories[i], boxScale);
			}

			Text.Font = GameFont.Small;
			Text.Anchor = TextAnchor.UpperLeft;
			Widgets.EndGroup();

			float y = rect.y + rect.height - 38f;
			Text.Font = GameFont.Small;
			string label = "Back".Translate();
			var backSize = 200;
			if ((Widgets.ButtonText(new Rect(((rect.x + rect.width) / 2f) - (backSize / 2), y, backSize, BottomButSize.y), label) || KeyBindingDefOf.Cancel.KeyDownEvent) && CanDoBack())
			{
				DoBack();
			}
		}

		private void DrawStorytellerCategoryBox(Rect boxRect, StorytellerCategory category, float boxScale)
		{
			Widgets.DrawBox(boxRect, 1, BaseContent.GreyTex);
			var iconRect = boxRect.ContractedBy(boxScale);
			iconRect.y -= 35;
			if (Widgets.ButtonImageFitted(iconRect, GetIcon(category)))
			{
				StorytellerUI_DrawStorytellerSelectionInterface_Patch.selectedCategory = category;
				if (StorytellerUI_DrawStorytellerSelectionInterface_Patch.FilteredStorytellers(DefDatabase<StorytellerDef>.AllDefs).Any())
				{
					this.Close();
					var page = new Page_SelectStoryteller();
					page.next = nextPage;
					nextPage.prev = page;
					page.prev = this;
					Find.WindowStack.Add(page);
				}
				else
				{
					StorytellerUI_DrawStorytellerSelectionInterface_Patch.selectedCategory = StorytellerCategory.Undefined;
					Messages.Message("There are no available storytellers in that category.", MessageTypeDefOf.RejectInput, historical: false);
				}
			}
			Text.Font = GameFont.Medium;
			var categoryLabel = new Rect(boxRect.x, boxRect.y + (boxRect.height - 80), boxRect.width, 30);
			Widgets.Label(categoryLabel, GetCategoryLabel(category));
			Text.Font = GameFont.Small;
			GUI.color = new ColorInt(240, 240, 240).ToColor;
			var oldStyle = Text.fontStyles[1];
			var newStyle = new GUIStyle(oldStyle);
			Text.fontStyles[1] = newStyle;
			newStyle.fontSize = 17;
			Widgets.Label(new Rect(boxRect.x + 10, categoryLabel.yMax - 5, boxRect.width - 20, 50), GetCategoryDescription(category));
			Text.fontStyles[1] = oldStyle;
			GUI.color = Color.white;
		}

		private static Texture2D GetIcon(StorytellerCategory category)
		{
			switch (category)
			{
				case StorytellerCategory.SlowPaced: return DefsOf.Phoebe.portraitLargeTex;
				case StorytellerCategory.Engaging: return StorytellerDefOf.Cassandra.portraitLargeTex;
				case StorytellerCategory.Chaotic: return DefsOf.Randy.portraitLargeTex;
			}
			return ContentFinder<Texture2D>.Get("UI/Icons/" + category.ToString());
		}
		private string GetCategoryLabel(StorytellerCategory category)
		{
			return category switch
			{
				StorytellerCategory.SlowPaced => "Slow-paced",
				_ => category.ToString()
			};
		}
		private string GetCategoryDescription(StorytellerCategory category)
		{
			return category switch
			{
				StorytellerCategory.SlowPaced => "A story that lets you relax between major events.",
				StorytellerCategory.Engaging => "A story with present momentum, adjusting to your highs and lows.",
				StorytellerCategory.Chaotic => "A story with no regard for consistency, utter chaos.",
				StorytellerCategory.Consistent => "A story with predictable rates of events and triggers.",
				StorytellerCategory.Brutal => "A bloody test of patience, skill, and strategy.",
				StorytellerCategory.Bizarre => "Stories told in new formats, often feeling completely new.",
				_ => "Unknown category."
			};
		}
	}
}