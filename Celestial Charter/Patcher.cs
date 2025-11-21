using Brutal.GlfwApi;
using RenderCore;
using HarmonyLib;
using KSA;
using Brutal.Logging;

namespace Celestial_Charter
{
    [HarmonyPatch]
    internal static class Patcher
    {
        private static Harmony? MHarmony = new Harmony("Celestial Charter");

        public static void Patch()
        {
            MHarmony?.PatchAll(typeof(Patcher).Assembly);
        }

        public static void Unload()
        {
            MHarmony?.UnpatchAll(MHarmony.Id);
            MHarmony = null;
        }

        [HarmonyPatch(typeof(Vehicle), nameof(Vehicle.OnKey))]
        [HarmonyPrefix]
        public static bool KeyInput(RenderCore.Input.GlfwKeyEvent keyEvent)
        {
            GlfwKey key = keyEvent.Key;
            GlfwKeyAction action = keyEvent.Action;

            // This can be optimized with a switch case
            if (key == GlfwKey.RightBracket && action == GlfwKeyAction.Release)
            {
                Main.ShowWindow = true;
                return true;
            }

            return false;
        }
    }
}
