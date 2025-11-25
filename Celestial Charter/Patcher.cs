using Brutal.GlfwApi;
using RenderCore;
using HarmonyLib;
using KSA;
using Brutal.Logging;

namespace Celestial_Charter
{
    [HarmonyPatch]
    public static class Patcher
    {
        [HarmonyPatch(typeof(Vehicle), nameof(Vehicle.OnKey))]
        [HarmonyPrefix]
        public static bool KeyInput(RenderCore.Input.GlfwKeyEvent keyEvent)
        {
            GlfwKey key = keyEvent.Key;
            GlfwKeyAction action = keyEvent.Action;

            // When Switching vehicles reopen the Celestial Charter window
            if (key == GlfwKey.RightBracket && action == GlfwKeyAction.Release)
            {
                CelestialCharter.ShowWindow = true;

                // Return true to continue with the other key checks
                return true;
            }
            // Return true to continue with the other key checks
            return true;
            // Return false to skip all the other key checks
        }
    }
}
