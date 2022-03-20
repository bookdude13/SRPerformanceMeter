using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using MelonLoader;
using UnityEngine;

namespace PerformanceMeter
{
    // Called when updating the life/energy bar
    [HarmonyPatch(typeof(GameControlManager), "UpdateLifesBar")]
    class Patched_GameControlManager__UpdateLifesBar
    {
        static void Postfix(GameControlManager __instance)
        {
            MainMod.OnUpdateLifesBar(__instance.PlayTimeMS, Synth.Utils.LifeBarHelper.GetScalePercent());
        }
    }

    // Called on game end
    [HarmonyPatch(typeof(GameControlManager), nameof(GameControlManager.GameOver))]
    class Patched_GameControlManager__GameOver
    {
        static void Postfix(GameControlManager __instance)
        {
            // Add final frame for life pct calculations
            MainMod.Log("GameOver at " + __instance.PlayTimeMS);
            MainMod.OnUpdateLifesBar(__instance.PlayTimeMS, Synth.Utils.LifeBarHelper.GetScalePercent());
        }
    }
}
