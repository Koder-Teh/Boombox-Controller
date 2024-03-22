using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoomboxController
{
    public class GrabbleBoombox
    {
        //[HarmonyPatch(typeof(GrabbableObject), "RequireCooldown")]
        //[HarmonyPrefix]
        //private static bool GrabbleBoombox_RequireCooldown(GrabbableObject __instance, ref bool __result)
        //{
        //    __result = false;
        //    return false;
        //}

        // False True - Включенная музыка
        // True True - Выключенная музыка
        //[HarmonyPatch(typeof(GrabbableObject), "UseItemOnClient")]
        //[HarmonyPrefix]
        //private static void GrabbleBoombox_UseItemOnClient(GrabbableObject __instance, bool buttonDown)
        //{
        //    Plugin.instance.Log(__instance.isBeingUsed + " " + buttonDown);
        //}
    }
}
