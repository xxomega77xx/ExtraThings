﻿using HarmonyLib;
using Hazel;
using ExtraButtons.Networking.MethodRpc;
using System.Linq;


namespace ExtraButtons
{
    public static class CustomRpcMethods
    {
        public static void RpcSetOverlay(PlayerControl player, MeetingHud meeting)
        {
            var playerstate = meeting.playerStates.FirstOrDefault(x => x.TargetPlayerId == player.PlayerId);
            playerstate.gameObject.SetActive(true);
            playerstate.Overlay.gameObject.SetActive(true);
            playerstate.Overlay.sprite = ExtraButtonsPlugin.MeetingOverlay;
            SoundManager.Instance.PlaySound(ExtraButtonsPlugin.GetAudioClip("AlarmClip"), false, 10);
        }

        public static void RpcRemoveOverlay(PlayerControl player, MeetingHud meeting)
        {
            var playerstate = meeting.playerStates.FirstOrDefault(x => x.TargetPlayerId == player.PlayerId);
            playerstate.Overlay.gameObject.SetActive(false);
        }
        public static void RpcPlayCustomAudio(PlayerControl player, MeetingHud meeting, string CustomClipName)
        {
            SoundManager.Instance.PlaySound(ExtraButtonsPlugin.GetAudioClip(CustomClipName), false, 10);
        }
    }

}
