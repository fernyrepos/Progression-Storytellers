using System;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace ProgressionStorytellers
{
    /// <summary>
    /// Custom page for selecting storyteller categories in the game setup flow.
    /// Replaces the standard storyteller selection page with a category-based approach.
    /// </summary>
    [HotSwappable]
    [StaticConstructorOnStartup]
    public class Page_SelectStorytellerCategory : Page
    {
        // Gets the page title from translation keys for localization support
        public override string PageTitle => "ProgressionStorytellers.PageTitle".Translate();

        // All available storyteller categories (excluding Undefined)
        private readonly StorytellerCategory[] storytellerCategories = Enum.GetValues(typeof(StorytellerCategory))
            .Cast<StorytellerCategory>()
            .Where(x => x != StorytellerCategory.Undefined)
            .ToArray();

        private Page_CreateWorldParams nextPage;

        /// <summary>
        /// Initializes a new instance with reference to the next page in the setup flow
        /// </summary>
        public Page_SelectStorytellerCategory(Page_CreateWorldParams nextPage)
        {
            this.nextPage = nextPage;
        }

        /// <summary>
        /// Main method for rendering the window contents
        /// </summary>
        public override void DoWindowContents(Rect rect)
        {
            // Setup title area
            Text.Anchor = TextAnchor.UpperCenter;
            rect.y += 45;
            rect.height -= 45;
            Text.Font = GameFont.Medium;

            // Custom font styling for page title
            var fontIndex = 2; // Medium font index
            var oldStyle = Text.fontStyles[fontIndex];
            var newStyle = new GUIStyle(oldStyle);
            Text.fontStyles[fontIndex] = newStyle;
            newStyle.fontSize = 20;

            Widgets.Label(new Rect(rect.x, rect.y - 30, rect.width, 30f), PageTitle);

            Text.fontStyles[fontIndex] = oldStyle;
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleCenter;

            // Begin category grid layout
            Widgets.BeginGroup(rect);

            // Grid layout parameters
            var scale = 0.9f;
            float boxWidth = 300 * scale;
            float boxHeight = 350 * scale;
            float xSpacing = 60;
            float ySpacing = 15;

            Text.Font = GameFont.Medium;
            var yOffset = 55 - 50; // Base vertical offset
            float boxScale = 30; // Icon scaling factor
            int columns = 3; // 3-column grid layout

            // Render each category in a grid
            for (int i = 0; i < storytellerCategories.Length; i++)
            {
                int row = i / columns;
                int column = i % columns;
                float xOffset = (row == 1) ? 20 : 0;

                // Adjust layout for middle row (different category count arrangement)
                if (row == 1)
                {
                    xSpacing = 40;
                    boxHeight = 300 * scale;
                    yOffset = 110 - 50;
                    boxScale = 45;
                }

                // Calculate position for this category box
                Rect boxRect = new Rect(
                    column * (boxWidth + xSpacing) + 20 + xOffset,
                    yOffset + row * (boxHeight + ySpacing) + 10,
                    boxWidth,
                    boxHeight
                );

                DrawStorytellerCategoryBox(boxRect, storytellerCategories[i], boxScale);
            }

            // Reset UI state
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;
            Widgets.EndGroup();

            // Add back button at the bottom
            float y = rect.y + rect.height - 38f;
            Text.Font = GameFont.Small;
            string label = "Back".Translate();
            var backSize = 200;

            if ((Widgets.ButtonText(new Rect(((rect.x + rect.width) / 2f) - (backSize / 2), y, backSize, BottomButSize.y), label) ||
                 KeyBindingDefOf.Cancel.KeyDownEvent) && CanDoBack())
            {
                DoBack();
            }
        }

        /// <summary>
        /// Renders an individual category selection box
        /// </summary>
        private void DrawStorytellerCategoryBox(Rect boxRect, StorytellerCategory category, float boxScale)
        {
            // Draw background box
            Widgets.DrawBox(boxRect, 1, BaseContent.GreyTex);

            // Calculate icon position and size
            var iconRect = boxRect.ContractedBy(boxScale);
            iconRect.y -= 35;

            // Make the icon clickable to select this category
            if (Widgets.ButtonImageFitted(iconRect, GetIcon(category)))
            {
                StorytellerUI_DrawStorytellerSelectionInterface_Patch.selectedCategory = category;

                // Check if there are storytellers in this category
                if (StorytellerUI_DrawStorytellerSelectionInterface_Patch.FilteredStorytellers(DefDatabase<StorytellerDef>.AllDefs).Any())
                {
                    // Proceed to storyteller selection within this category
                    this.Close();
                    var page = new Page_SelectStoryteller();
                    page.next = nextPage;
                    nextPage.prev = page;
                    page.prev = this;
                    Find.WindowStack.Add(page);
                }
                else
                {
                    // No storytellers in this category - show error message
                    StorytellerUI_DrawStorytellerSelectionInterface_Patch.selectedCategory = StorytellerCategory.Undefined;
                    Messages.Message(
                        "ProgressionStorytellers.NoStorytellersInCategory".Translate(),
                        MessageTypeDefOf.RejectInput,
                        historical: false
                    );
                }
            }

            // Display category name
            Text.Font = GameFont.Medium;
            var categoryLabel = new Rect(boxRect.x, boxRect.y + (boxRect.height - 80), boxRect.width, 30);
            Widgets.Label(categoryLabel, GetCategoryLabel(category));

            // Display category description with adaptive text sizing
            Text.Font = GameFont.Small;
            GUI.color = new ColorInt(240, 240, 240).ToColor;

            Rect descriptionRect = new Rect(
                boxRect.x + 10,
                categoryLabel.yMax + 5,
                boxRect.width - 20,
                boxRect.y + boxRect.height - categoryLabel.yMax - 15
            );

            DrawAdaptiveText(descriptionRect, GetCategoryDescription(category), GameFont.Small);

            GUI.color = Color.white;
        }

        /// <summary>
        /// Renders text that automatically adjusts font size to fit within the given rectangle
        /// Falls back to smaller fonts (Medium -> Small -> Tiny) if text doesn't fit
        /// </summary>
        private void DrawAdaptiveText(Rect rect, string text, GameFont baseFont = GameFont.Small)
        {
            // Save current text settings
            GameFont originalFont = Text.Font;
            bool originalWordWrap = Text.WordWrap;
            var originalStyle = Text.fontStyles[(int)GameFont.Small]; // Style for Small font

            Text.WordWrap = true;

            // Try different font sizes: Medium -> Small -> Tiny
            GameFont selectedFont = GameFont.Small;
            foreach (var fontSize in new[] { GameFont.Medium, GameFont.Small, GameFont.Tiny })
            {
                Text.Font = fontSize;
                float textHeight = Text.CalcHeight(text, rect.width);

                // Use this font if text fits or if we've reached the smallest font
                if (textHeight <= rect.height || fontSize == GameFont.Tiny)
                {
                    selectedFont = fontSize;
                    break;
                }
            }

            // Adjust style for the selected font
            if (selectedFont == GameFont.Small)
            {
                var newStyle = new GUIStyle(Text.fontStyles[(int)GameFont.Small]);
                newStyle.fontSize = 15; // Slightly smaller than default for better fit
                Text.fontStyles[(int)GameFont.Small] = newStyle;
            }

            // Draw the text with selected font
            Text.Font = selectedFont;
            Widgets.Label(rect, text);

            // Add tooltip with full text if it doesn't fit completely
            if (Text.CalcHeight(text, rect.width) > rect.height)
            {
                TooltipHandler.TipRegion(rect, text);
            }

            // Restore original text settings
            Text.fontStyles[(int)GameFont.Small] = originalStyle;
            Text.Font = originalFont;
            Text.WordWrap = originalWordWrap;
        }

        /// <summary>
        /// Returns the appropriate icon texture for a given storyteller category
        /// Uses vanilla storyteller portraits for some categories, custom icons for others
        /// </summary>
        private static Texture2D GetIcon(StorytellerCategory category)
        {
            switch (category)
            {
                case StorytellerCategory.SlowPaced:
                    return DefsOf.Phoebe.portraitLargeTex;
                case StorytellerCategory.Engaging:
                    return StorytellerDefOf.Cassandra.portraitLargeTex;
                case StorytellerCategory.Chaotic:
                    return DefsOf.Randy.portraitLargeTex;
                default:
                    // Fall back to custom icons in UI/Icons folder
                    return ContentFinder<Texture2D>.Get("UI/Icons/" + category.ToString());
            }
        }

        /// <summary>
        /// Gets the translated display name for a storyteller category
        /// </summary>
        private string GetCategoryLabel(StorytellerCategory category)
        {
            return ("ProgressionStorytellers.Category." + category.ToString()).Translate();
        }

        /// <summary>
        /// Gets the translated description for a storyteller category
        /// </summary>
        private string GetCategoryDescription(StorytellerCategory category)
        {
            return ("ProgressionStorytellers.Category." + category.ToString() + ".Description").Translate();
        }
    }
}