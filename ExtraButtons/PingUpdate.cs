using HarmonyLib;

namespace ExtraButtons
{
    [HarmonyPatch(typeof(PingTracker), nameof(PingTracker.Update))]
    public static class PingUpdateTracker
    {
        public static void Postfix(PingTracker __instance)
        {
            if (!MeetingHud.Instance)
            {
                __instance.text.text += $"\n<color=#00fff3>ExtraButtons v{ExtraButtonsPlugin.Version} created by Om3ga</color>";

            }
        }
    }
}
