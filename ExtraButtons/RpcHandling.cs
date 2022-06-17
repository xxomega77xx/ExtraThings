using HarmonyLib;
using Hazel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static ExtraButtons.ExtraButtonsPlugin;

namespace ExtraButtons
{
    public static class RpcHandling
    {
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
        public static class HandleRpc
        {
            public static void Postfix([HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
            {
                //if (callId >= 43) //System.Console.WriteLine("Received " + callId);
                byte readByte, readByte1, readByte2;
                sbyte readSByte, readSByte2;
                switch ((CustomRpcCalls)callId)
                {
                    case CustomRpcCalls.setOverlay:
                        readSByte = reader.ReadSByte();
                        GetPlayersinMeeting.values = MeetingHud.Instance.playerStates;
                        break;
                }
            }
        }
    }
}
