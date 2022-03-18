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
    // Notes added to active list as they spawn in
    [HarmonyPatch(typeof(GameControlManager), nameof(GameControlManager.AddNoteToActiveList), new Type[] { typeof(Game_Note) })]
    class Patched_GameControlManager__AddNoteToActiveList
    {
        static void Postfix(Game_Note _note)
        {
            MainMod.OnAddNoteToActiveList(_note);
            MainMod.Add();
        }
    }

    // Called for all notes except those on rails
    [HarmonyPatch(typeof(GameControlManager), nameof(GameControlManager.RemoveNoteFromActiveList), new Type[] { typeof(Game_Note) })]
    class Patched_GameControlManager__RemoveNoteFromActiveList
    {
        static void Postfix(Game_Note _note)
        {
            
        }
    }

    // Called when updating the life/energy bar
    [HarmonyPatch(typeof(GameControlManager), "UpdateLifesBar")]
    class Patched_GameControlManager__UpdateLifesBar
    {
        static void Postfix(GameControlManager __instance)
        {
            MainMod.OnUpdateLifesBar(__instance.PlayTimeMS, Synth.Utils.LifeBarHelper.GetScalePercent());
        }
    }

    // Called on hits. Doesn't capture misses (or rail notes???)
    [HarmonyPatch(typeof(GameControlManager), nameof(GameControlManager.OnScore), new Type[] {
        typeof(int), typeof(bool), typeof(GameObject), typeof(Game_Note.ContactState), typeof(bool)
    })]
    class Patched_GameControlManager__OnScore
    {
        static void Postfix(int points, bool isCombo, GameObject controller, Game_Note.ContactState cstate, bool isLine)
        {
            MainMod.Log(string.Format(
                "OnScore: {0} points. Combo? {1}, Line? {4}, ContactState: {3}, controller {2}",
                points, isCombo, controller, cstate, isLine
            ));
            MainMod.Remove();
        }
    }

    // Called when combo fails. Called when multiple failures in a row???
    [HarmonyPatch(typeof(GameControlManager), nameof(GameControlManager.OnComboBreak), new Type[] {
        typeof(bool), typeof(bool), typeof(int), typeof(bool)
    })]
    class Patched_GameControlManager__OnComboBreak
    {
        static void Postfix(bool wrongHand, bool wasNotHandsClose, int eventID, bool removeSpecial)
        {
            MainMod.Log(string.Format(
                "OnComboBreak: wrongHand? {0}, wasNotHandsClose? {1}, removeSpecial? {3}, eventID: {2}",
                wrongHand, wasNotHandsClose, eventID, removeSpecial
            ));
            MainMod.Remove();
        }
    }

    // Called on game end
    [HarmonyPatch(typeof(GameControlManager), nameof(GameControlManager.GameOver))]
    class Patched_GameControlManager__GameOver
    {
        static void Postfix(GameControlManager __instance)
        {
            // Add final frame for life pct calculations
            MainMod.Log("GameOver at " + __instance.PlayTimeMS + ". PlayDelay: " + __instance.PlayDelay());
            MainMod.OnUpdateLifesBar(__instance.PlayTimeMS, Synth.Utils.LifeBarHelper.GetScalePercent());
        }
    }

    // Called when special note hit, regardless of if it was a good hit or not.
    // Can be a miss if a 2h is hit by only one hand.
    [HarmonyPatch(typeof(GameControlManager), nameof(GameControlManager.OnSpecialNoteHit), new Type[] {
        typeof(string), typeof(bool), typeof(int)
    })]
    class Patched_GameControlManager__OnSpecialNoteHit
    {
        static void Postfix(string alias, bool type2 = false, int eventID = -1)
        {
            MainMod.Log(string.Format(
                "OnSpecialNoteHit: alias {0}, type2 {1}, eventID {2}",
                alias, type2, eventID
            ));
        }
    }

    // Called when a special (1h/2h) last note in the sequence(?) is hit
    [HarmonyPatch(typeof(GameControlManager), nameof(GameControlManager.OnSpecialEnd), new Type[] {
        typeof(bool), typeof(int), typeof(bool)
    })]
    class Patched_GameControlManager__OnSpecialEnd
    {
        static void Postfix(bool wasFailed, int eventID, bool forceSuccess)
        {
            MainMod.Log(string.Format(
                "OnSpecialEnd: wasFailed? {0}, forceSuccess? {2}, eventID {1}",
                wasFailed, eventID, forceSuccess
            ));
        }
    }
}
