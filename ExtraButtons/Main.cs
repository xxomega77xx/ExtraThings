using BepInEx;
using BepInEx.IL2CPP;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections;
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

        [HarmonyPatch(typeof(LobbyBehaviour), nameof(LobbyBehaviour.Start))]
        public class LobbyPatch
        {
            public static void Prefix()
            {

                CreateReadyButtons();
            }


            public static void CreateReadyButtons()
            {
                var CHLog = new ManualLogSource("ExtraButtons");
                BepInEx.Logging.Logger.Sources.Add(CHLog);
                try
                {

                    CHLog.Log(LogLevel.Info, "Starting creation of buttons");

                    var ReadyButton = UnityEngine.Object.Instantiate(HudManager.Instance.UseButton, HudManager.Instance.UseButton.transform.parent);
                    UnityEngine.Object.Destroy(ReadyButton.GetComponentInChildren<TextTranslatorTMP>());
                    CHLog.Log(LogLevel.Info, "Button Grabbed");
                    ReadyButton.name = "ReadyButton";
                    CHLog.Log(LogLevel.Info, "button name set");
                    ReadyButton.OverrideText("Ready");
                    CHLog.Log(LogLevel.Info, "button text set");
                    ReadyButton.OverrideColor(color: Color.red);
                    CHLog.Log(LogLevel.Info, "button color set");
                    ReadyButton.transform.localPosition = new Vector3((float)ReadyButton.transform.localPosition.x - 2f, (float)ReadyButton.gameObject.transform.localPosition.y, (float)ReadyButton.gameObject.transform.position.z);
                    CHLog.Log(LogLevel.Info, "button position set");
                    ReadyButton.graphic.SetCooldownNormalizedUvs();
                    CHLog.Log(LogLevel.Info, " button cooldown set");
                    var passiveButton = ReadyButton.GetComponent<PassiveButton>();
                    CHLog.Log(LogLevel.Info, "passive button component Grabbed");
                    passiveButton.OnClick = new UnityEngine.UI.Button.ButtonClickedEvent();
                    CHLog.Log(LogLevel.Info, "onclick event created");
                    passiveButton.OnClick.AddListener((Action)(() =>
                    {

                        var currentName = PlayerControl.LocalPlayer.name;
                        if (currentName.Contains(">"))
                        {
                            var modifiedName = currentName.Split(">", StringSplitOptions.RemoveEmptyEntries);
                            if (ReadyButton.buttonLabelText.text == "Ready")
                            {
                                CHLog.Log(LogLevel.Info, $"Current name : {modifiedName[1]}");
                                PlayerControl.LocalPlayer.CheckName($"<color=green>{modifiedName[1]}");
                                CHLog.Log(LogLevel.Info, "Button Clicked player set to green");
                                ReadyButton.OverrideText("UnReady");
                                ReadyButton.OverrideColor(Color.green);
                            }
                            else
                            {
                                CHLog.Log(LogLevel.Info, $"Current name : {modifiedName[1]}");
                                CHLog.Log(LogLevel.Info, "Button Clicked player set to red");
                                CHLog.Log(LogLevel.Info, $"Button text {ReadyButton.buttonLabelText.text}");
                                PlayerControl.LocalPlayer.CheckName($"<color=red>{modifiedName[1]}");
                                ReadyButton.OverrideText("Ready");
                                ReadyButton.OverrideColor(Color.red);
                            }
                        }
                        else
                        {
                            if (ReadyButton.buttonLabelText.text == "Ready")
                            {
                                PlayerControl.LocalPlayer.CheckName($"<color=green>{currentName}");
                                CHLog.Log(LogLevel.Info, "Button Clicked player set to green");
                                ReadyButton.OverrideText("UnReady");
                                ReadyButton.OverrideColor(Color.green);
                            }
                            else
                            {
                                CHLog.Log(LogLevel.Info, "Button Clicked player set to red");
                                CHLog.Log(LogLevel.Info, $"Button text {ReadyButton.buttonLabelText.text}");
                                PlayerControl.LocalPlayer.CheckName($"<color=red>{currentName}");
                                ReadyButton.OverrideText("Ready");
                                ReadyButton.OverrideColor(Color.red);
                            }
                        }
                        
                        
                        
                    }));


                }
                catch (Exception e)
                {
                    CHLog.Log(LogLevel.Error, $"{e.InnerException.StackTrace}");
                    throw;
                }
            }
        }

        [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.OnDestroy))]
        public class OnGameStart
        {

            public static void Prefix(IntroCutscene __instance)
            {
                var currentName = PlayerControl.LocalPlayer.name;
                GameObject ReadyButton;
                var CHLog = new ManualLogSource("ExtraButtons");
                BepInEx.Logging.Logger.Sources.Add(CHLog);

                ReadyButton = GameObject.Find("ReadyButton");
                CHLog.Log(LogLevel.Info, "Attempting to destroy readybutton");
                UnityEngine.Object.Destroy(ReadyButton);
                CHLog.Log(LogLevel.Info, "ReadyButton Destroyed.");

                PlayerControl.LocalPlayer.CheckName($"<color=white>{currentName}");
            }
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
        public class OnMeetingStart
        {
            public static void Prefix(MeetingHud __instance)
            {
                var RaiseLowerHandButton = UnityEngine.Object.Instantiate(HudManager.Instance.UseButton, HudManager.Instance.UseButton.transform.parent);
                UnityEngine.Object.Destroy(RaiseLowerHandButton.GetComponentInChildren<TextTranslatorTMP>());
                RaiseLowerHandButton.transform.localPosition = new Vector3((float)RaiseLowerHandButton.transform.localPosition.x - 5f, (float)RaiseLowerHandButton.gameObject.transform.localPosition.y, (float)RaiseLowerHandButton.gameObject.transform.position.z);
                RaiseLowerHandButton.name = "RaiseHandButton";
                RaiseLowerHandButton.OverrideText("Raise Hand");
                RaiseLowerHandButton.Show();
                RaiseLowerHandButton.enabled = true;
                RaiseLowerHandButton.Awake();
                RaiseLowerHandButton.graphic.SetCooldownNormalizedUvs();
                var currentName = PlayerControl.LocalPlayer.name;

                var passiveButton = RaiseLowerHandButton.GetComponent<PassiveButton>();
                passiveButton.OnClick = new UnityEngine.UI.Button.ButtonClickedEvent();
                passiveButton.OnClick.AddListener((Action)(() =>
                {

                    PlayerControl.LocalPlayer.CheckName($"<color=blue>{currentName} Hand Raised!");
                    PlayerControl.LocalPlayer.nameText.name = $"<color=blue>{currentName} Hand Raised!";
                    PlayerControl.LocalPlayer.RpcSendChat($"{currentName} raised hand");
                    if (RaiseLowerHandButton.buttonLabelText.text == "Raise Hand")
                    {
                        RaiseLowerHandButton.OverrideText("Lower Hand");
                    }
                    else
                    {
                        RaiseLowerHandButton.OverrideText("Raise Hand");
                        PlayerControl.LocalPlayer.RpcSendChat($"{currentName} lowered hand");

                    }
                }
                ));


            }
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Close))]
        public class OnMeetingDestroy
        {
            public static void Postfix(MeetingHud __instance)
            {
                GameObject LowerHandButton;
                GameObject RaiseHandButton;

                LowerHandButton = GameObject.Find("LowerHandButton");
                RaiseHandButton = GameObject.Find("RaiseHandButton");
                UnityEngine.Object.Destroy(LowerHandButton);
                UnityEngine.Object.Destroy(RaiseHandButton);
            }
        }

    }
}
