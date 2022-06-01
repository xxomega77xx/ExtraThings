using BepInEx;
using BepInEx.IL2CPP;
using BepInEx.Logging;
using HarmonyLib;
using System;
using UnityEngine;

namespace ExtraButtons
{
    [BepInPlugin(Id, "ExtraButtons", Version)]
    [BepInProcess("Among Us.exe")]
    public class Main : BasePlugin
    {
        
        public const string Version = "1.0.0";
        public const string Id = "ExtraButtons.pack";
        public Harmony Harmony { get; } = new Harmony(Id);

        public override void Load()
        {
            Harmony.PatchAll();
        }

        [HarmonyPatch(typeof(LobbyBehaviour), nameof(HudManager.Start))]
        public class LobbyPatch
        {
            public static void Prefix(HudManager __instance)
            {
                var ReadyButton = UnityEngine.Object.Instantiate(HudManager.Instance.UseButton, HudManager.Instance.UseButton.transform.parent);
                UnityEngine.Object.Destroy(ReadyButton.GetComponentInChildren<TextTranslatorTMP>());
                ReadyButton.name = "ReadyButton";
                ReadyButton.OverrideText("Ready");
                ReadyButton.Show();
                ReadyButton.enabled = true;
                ReadyButton.Awake();
                ReadyButton.OverrideColor(color: Color.red);
                ReadyButton.transform.localPosition = new Vector3((float)ReadyButton.transform.localPosition.x - 2f, (float)ReadyButton.gameObject.transform.localPosition.y, (float)ReadyButton.gameObject.transform.position.z);
                ReadyButton.graphic.SetCooldownNormalizedUvs();
                var currentName = PlayerControl.LocalPlayer.name;
                var passiveButton = ReadyButton.GetComponent<PassiveButton>();
                passiveButton.OnClick = new UnityEngine.UI.Button.ButtonClickedEvent();
                passiveButton.OnClick.AddListener((Action)(() =>
                {
                    PlayerControl.LocalPlayer.RpcSetName($"<color=green>{currentName}");
                    if (ReadyButton.buttonLabelText.text == "Ready")
                    {
                        ReadyButton.OverrideText("UnReady");
                        ReadyButton.OverrideColor(Color.green);
                    }
                    else
                    {
                        PlayerControl.LocalPlayer.RpcSetName($"<color=red>{currentName}");
                        ReadyButton.OverrideText("Ready");
                        ReadyButton.OverrideColor(Color.red);
                    }
                }));

            }


        }
        [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.OnDestroy))]
        public class OnGameStart
        {
            
            public static void Prefix(IntroCutscene __instance)
            {
                var currentName = PlayerControl.LocalPlayer.name;
                GameObject ReadyButton;
                var CHLog = new ManualLogSource("StuffandThings");
                BepInEx.Logging.Logger.Sources.Add(CHLog);

                ReadyButton = GameObject.Find("ReadyButton");
                CHLog.Log(LogLevel.Info, "Attempting to destroy readybutton");
                UnityEngine.Object.Destroy(ReadyButton);
                CHLog.Log(LogLevel.Info, "ReadyButton Destroyed.");

                PlayerControl.LocalPlayer.SetName($"<color=white>{currentName}");
            }
        }

        //[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
        //public class OnMeetingStart
        //{
        //    public static void Prefix(MeetingHud __instance)
        //    {
        //        var RaiseLowerHandButton = UnityEngine.Object.Instantiate(HudManager.Instance.UseButton, HudManager.Instance.UseButton.transform.parent);
        //        UnityEngine.Object.Destroy(RaiseLowerHandButton.GetComponentInChildren<TextTranslatorTMP>());
        //        RaiseLowerHandButton.transform.localPosition = new Vector3((float)RaiseLowerHandButton.transform.localPosition.x - 2f, (float)RaiseLowerHandButton.gameObject.transform.localPosition.y, (float)RaiseLowerHandButton.gameObject.transform.position.z);
        //        RaiseLowerHandButton.name = "RaiseHandButton";
        //        RaiseLowerHandButton.OverrideText("Raise Hand");
        //        RaiseLowerHandButton.Show();
        //        RaiseLowerHandButton.enabled = true;
        //        RaiseLowerHandButton.Awake();
        //        RaiseLowerHandButton.graphic.SetCooldownNormalizedUvs();
        //        var currentName = PlayerControl.LocalPlayer.name;
                
        //        var passiveButton = RaiseLowerHandButton.GetComponent<PassiveButton>();
        //        passiveButton.OnClick = new UnityEngine.UI.Button.ButtonClickedEvent();
        //        passiveButton.OnClick.AddListener((Action)(() =>
        //        {
                    
        //            PlayerControl.LocalPlayer.SetName($"<color=blue>{currentName} Hand Raised!");
        //            PlayerControl.LocalPlayer.nameText.name = $"<color=blue>{currentName} Hand Raised!";
        //            PlayerControl.LocalPlayer.RpcSendChat($"{currentName} raised hand");
        //            if (RaiseLowerHandButton.buttonLabelText.text == "Raise Hand")
        //            {
        //                RaiseLowerHandButton.OverrideText("Lower Hand");
        //                PlayerControl.LocalPlayer.RpcSendChat($"{currentName} lowered hand");
        //            }
        //            else
        //            {
        //                RaiseLowerHandButton.OverrideText("Raise Hand");
        //                PlayerControl.LocalPlayer.SetName(currentName);
                        
        //            }
        //        }
        //        ));


        //        }
        //}

        //[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Close))]
        //public class OnMeetingDestroy
        //{
        //    public static void Postfix(MeetingHud __instance)
        //    {
        //        GameObject LowerHandButton;
        //        GameObject RaiseHandButton;

        //        LowerHandButton = GameObject.Find("LowerHandButton");
        //        RaiseHandButton = GameObject.Find("RaiseHandButton");
        //        UnityEngine.Object.Destroy(LowerHandButton);
        //        UnityEngine.Object.Destroy(RaiseHandButton);
        //    }
        //}

    }
}
