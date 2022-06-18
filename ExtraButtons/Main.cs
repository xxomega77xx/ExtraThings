using BepInEx;
using BepInEx.IL2CPP;
using BepInEx.Logging;
using HarmonyLib;
using Hazel;
using Reactor;
using Reactor.Extensions;
using Reactor.Networking.MethodRpc;
using System;
using System.Linq;
using System.Reflection;
using UnhollowerBaseLib;
using UnityEngine;
//Some methods and code snatched from TOU so thanks to those guys for that
namespace ExtraButtons
{
    [BepInPlugin(Id, "ExtraButtons", Version)]
    [BepInProcess("Among Us.exe")]
    [BepInDependency(ReactorPlugin.Id)]
    public class ExtraButtonsPlugin : BasePlugin
    {

        public const string Version = "1.0.0";
        public const string Id = "ExtraButtons.pack";
        public Harmony Harmony { get; } = new Harmony(Id);
        public static Sprite Ready;
        public static Sprite NotReady;
        public static Sprite RaiseHand;
        public static Sprite LowerHand;
        public static Sprite MeetingOverlay;
        public static int position = 0;
        public static int samplerate = 44100;
        public static float frequency = 440;
        public override void Load()
        {

            Ready = CreateSprite("ExtraButtons.Assets.ready_button.png");
            NotReady = CreateSprite("ExtraButtons.Assets.notreadybutton.png");
            RaiseHand = CreateSprite("ExtraButtons.Assets.raise_hand_glow_button.png");
            MeetingOverlay = CreateSprite("ExtraButtons.Assets.hand_raise_overlay.png");

            Harmony.PatchAll();
        }

        [HarmonyPatch(typeof(MeetingHud))]
        public class GetPlayersinMeeting
        {
            public static Il2CppReferenceArray<PlayerVoteArea> values;


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
                    ReadyButton.graphic.sprite = Ready;

                    UnityEngine.Object.Destroy(ReadyButton.GetComponentInChildren<TextTranslatorTMP>());
                    CHLog.Log(LogLevel.Info, "Button Grabbed");
                    ReadyButton.name = "ReadyButton";
                    CHLog.Log(LogLevel.Info, "button name set");
                    ReadyButton.OverrideText("");
                    CHLog.Log(LogLevel.Info, "button text set");
                    ReadyButton.OverrideColor(color: Color.green);
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
                            if (ReadyButton.graphic.color == Color.green)
                            {
                                CHLog.Log(LogLevel.Info, $"Current name : {modifiedName[1]}");
                                PlayerControl.LocalPlayer.CheckName($"<color=green>{modifiedName[1]}");
                                CHLog.Log(LogLevel.Info, "Button Clicked player set to green");
                                ReadyButton.OverrideColor(Color.red);
                                ReadyButton.graphic.sprite = NotReady;
                            }
                            else
                            {
                                CHLog.Log(LogLevel.Info, $"Current name : {modifiedName[1]}");
                                CHLog.Log(LogLevel.Info, "Button Clicked player set to red");
                                CHLog.Log(LogLevel.Info, $"Button text {ReadyButton.buttonLabelText.text}");
                                PlayerControl.LocalPlayer.CheckName($"<color=red>{modifiedName[1]}");
                                ReadyButton.graphic.sprite = Ready;
                                ReadyButton.OverrideColor(Color.green);
                            }
                        }
                        else
                        {
                            if (ReadyButton.graphic.color == Color.green)
                            {
                                PlayerControl.LocalPlayer.CheckName($"<color=green>{currentName}");
                                CHLog.Log(LogLevel.Info, "Button Clicked player set to green");
                                ReadyButton.OverrideColor(Color.red);
                                ReadyButton.graphic.sprite = NotReady;
                            }
                            else
                            {
                                CHLog.Log(LogLevel.Info, "Button Clicked player set to red");
                                CHLog.Log(LogLevel.Info, $"Button text {ReadyButton.buttonLabelText.text}");
                                PlayerControl.LocalPlayer.CheckName($"<color=red>{currentName}");
                                ReadyButton.OverrideColor(Color.green);
                                ReadyButton.graphic.sprite = Ready;
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

                PlayerControl.LocalPlayer.CheckName($"{currentName}");
            }
        }

        [MethodRpc((uint)CustomRpcCalls.setOverlay)]
        public static void RpcSetOverlay(PlayerControl player, MeetingHud meeting)
        {
            var playerstate = meeting.playerStates.FirstOrDefault(x => x.TargetPlayerId == player.PlayerId);
            Logger<ExtraButtonsPlugin>.Info($"Setting overlay for {player.name}");
            Logger<ExtraButtonsPlugin>.Info($"Setting {player.name} meeting overlay");
            playerstate.gameObject.SetActive(true);
            playerstate.Overlay.gameObject.SetActive(true);
            playerstate.Overlay.sprite = MeetingOverlay;
            SoundManager.Instance.PlaySound(meeting.VoteSound, false, 10);
        }

        [MethodRpc((uint)CustomRpcCalls.removeOverlay)]
        public static void RpcRemoveOverlay(PlayerControl player, MeetingHud meeting)
        {
            Logger<ExtraButtonsPlugin>.Info($"Removing {player.name} overlay");
            var playerstate = meeting.playerStates.FirstOrDefault(x => x.TargetPlayerId == player.PlayerId);
            playerstate.Overlay.gameObject.SetActive(false);
        }


        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
        public class OnMeetingStart
        {
            public static void Prefix(MeetingHud __instance)
            {                
                var RaiseLowerHandButton = UnityEngine.Object.Instantiate(HudManager.Instance.UseButton, HudManager.Instance.UseButton.transform.parent);
                RaiseLowerHandButton.graphic.sprite = RaiseHand;
                UnityEngine.Object.Destroy(RaiseLowerHandButton.GetComponentInChildren<TextTranslatorTMP>());
                RaiseLowerHandButton.transform.localPosition = new Vector3((float)RaiseLowerHandButton.transform.localPosition.x - 5f, (float)RaiseLowerHandButton.gameObject.transform.localPosition.y, (float)RaiseLowerHandButton.gameObject.transform.position.z);
                RaiseLowerHandButton.name = "RaiseHandButton";
                RaiseLowerHandButton.OverrideText("");
                RaiseLowerHandButton.OverrideColor(Color.green);
                if (!PlayerControl.LocalPlayer.Data.IsDead || !__instance.amDead)
                {
                    RaiseLowerHandButton.Show();
                    RaiseLowerHandButton.enabled = true;
                    RaiseLowerHandButton.Awake();
                }
                RaiseLowerHandButton.graphic.SetCooldownNormalizedUvs();
                var passiveButton = RaiseLowerHandButton.GetComponent<PassiveButton>();
                passiveButton.OnClick = new UnityEngine.UI.Button.ButtonClickedEvent();
                passiveButton.OnClick.AddListener((Action)(() =>
                {
                    if (RaiseLowerHandButton.graphic.color == Color.green)
                    {
                        RpcSetOverlay(PlayerControl.LocalPlayer, __instance);
                        RaiseLowerHandButton.OverrideColor(Color.red);
                    }
                    else
                    {
                        RpcRemoveOverlay(PlayerControl.LocalPlayer, __instance);
                        RaiseLowerHandButton.OverrideColor(Color.green);
                    };

                }
                ));

            }
        }



        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Update))]
        public class OnMeetingDead
        {
            public static void Postfix(MeetingHud __instance)
            {
                if (PlayerControl.LocalPlayer.Data.IsDead || __instance.amDead)
                {
                    GameObject RaiseHandButton;
                    RaiseHandButton = GameObject.Find("RaiseHandButton");
                    UnityEngine.Object.Destroy(RaiseHandButton);
                }
            }
        }


        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Close))]
        public class OnMeetingDestroy
        {
            public static void Postfix(MeetingHud __instance)
            {
                GameObject RaiseHandButton;
                RaiseHandButton = GameObject.Find("RaiseHandButton");
                UnityEngine.Object.Destroy(RaiseHandButton);
            }
        }

        private static DLoadImage _iCallLoadImage;
        public static Sprite CreateSprite(string name)
        {
            var pixelsPerUnit = 100f;
            var pivot = new Vector2(0.5f, 0.5f);

            var assembly = Assembly.GetExecutingAssembly();
            var tex = GUIExtensions.CreateEmptyTexture();
            var imageStream = assembly.GetManifestResourceStream(name);
            var img = imageStream.ReadFully();
            LoadImage(tex, img, true);
            tex.DontDestroy();
            var sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), pivot, pixelsPerUnit);
            sprite.DontDestroy();
            return sprite;

        }
        public static void LoadImage(Texture2D tex, byte[] data, bool markNonReadable)
        {
            _iCallLoadImage ??= IL2CPP.ResolveICall<DLoadImage>("UnityEngine.ImageConversion::LoadImage");
            var il2CPPArray = (Il2CppStructArray<byte>)data;
            _iCallLoadImage.Invoke(tex.Pointer, il2CPPArray.Pointer, markNonReadable);
        }
        private delegate bool DLoadImage(IntPtr tex, IntPtr data, bool markNonReadable);

    }
}
