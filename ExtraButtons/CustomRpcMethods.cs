using Reactor;
using Reactor.Networking.MethodRpc;
using System.Linq;
using UnityEngine;

namespace ExtraButtons
{
    public static class CustomRpcMethods
    {
        [MethodRpc((uint)CustomRpcCalls.setOverlay)]
        public static void RpcSetOverlay(PlayerControl player, MeetingHud meeting)
        {
            var playerstate = meeting.playerStates.FirstOrDefault(x => x.TargetPlayerId == player.PlayerId);
            Logger<ExtraButtonsPlugin>.Info($"Setting overlay for {player.name}");
            Logger<ExtraButtonsPlugin>.Info($"Setting {player.name} meeting overlay");
            playerstate.gameObject.SetActive(true);
            playerstate.Overlay.gameObject.SetActive(true);
            playerstate.Overlay.sprite = ExtraButtonsPlugin.MeetingOverlay;
            SoundManager.Instance.PlaySound(ExtraButtonsPlugin.GetAudioClip("AlarmClip"), false, 10);
        }

        [MethodRpc((uint)CustomRpcCalls.removeOverlay)]
        public static void RpcRemoveOverlay(PlayerControl player, MeetingHud meeting)
        {
            Logger<ExtraButtonsPlugin>.Info($"Removing {player.name} overlay");
            var playerstate = meeting.playerStates.FirstOrDefault(x => x.TargetPlayerId == player.PlayerId);
            playerstate.Overlay.gameObject.SetActive(false);
        }
    }
}
