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
    [StaticConstructorOnStartup]
    public static class DebugStartup
    {
        static DebugStartup()
        {
            Log.Message("[ProgressionStorytellers] Mod starting for RimWorld 1.6");

            // Проверим, загружен ли наш тип
            var type = GenTypes.GetTypeInAnyAssembly("ProgressionStorytellers.StorytellerExtension");
            if (type != null)
            {
                Log.Message($"[ProgressionStorytellers] Type found: {type.FullName}");
            }
            else
            {
                Log.Error("[ProgressionStorytellers] Type NOT found!");
            }
        }
    }
}
