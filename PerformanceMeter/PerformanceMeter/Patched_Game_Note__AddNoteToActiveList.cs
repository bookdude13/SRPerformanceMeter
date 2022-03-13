using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using MelonLoader;

namespace PerformanceMeter
{
    // AddToNotesPool
    // AddToFullNotesPool
    // AddNoteToActiveList

    [HarmonyPatch(typeof(GameControlManager), "AddNoteToActiveList", new Type[] { typeof(Game_Note) })]
    class Patched_GameControlManager__AddNoteToActiveList
    {
        static void Prefix(ref Game_Note _note)
        {
            MainMod.OnAddNoteToActiveList(_note);
        }
    }

    [HarmonyPatch(typeof(GameControlManager), "RemoveNoteFromActiveList", new Type[] { typeof(Game_Note) })]
    class Patched_Game_Note__RemoveNoteFromActiveList
    {
        static void Prefix(ref Game_Note _note)
        {
            MainMod.OnRemoveNoteFromActiveList(_note);
        }
    }

    /*[HarmonyPatch(typeof(Game_Note), "OnDisableTrigger")]
    class Patched_Game_Note__OnDisableTrigger
    {
        static void Prefix(Game_Note __instance, bool wasWrongHand = false, bool wasNotHandsClose = false, float _delayGO = 0f)
        {
            MainMod.Log(" OnDisableTrigger. " + __instance.name + ", " + wasWrongHand + ", " + wasNotHandsClose + ", " + _delayGO);
            MainMod.OnRemoveNoteFromActiveList(__instance, _delayGO);
        }
    }*/
}
