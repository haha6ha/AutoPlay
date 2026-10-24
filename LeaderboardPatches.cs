using HarmonyLib;

namespace AutoReplay
{
    [HarmonyPatch(typeof(LevelFilteringNavigationController), "DidActivate")]
    public static class LevelFilteringNavigationControllerPatch
    {
        public static void Postfix(LevelFilteringNavigationController __instance)
        {
            PepeButtonController.AttachToLevelFiltering(__instance);
        }
    }
}






