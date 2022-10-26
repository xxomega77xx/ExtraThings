using BepInEx;
using BepInEx.Unity.IL2CPP;
using BepInEx.Logging;
using HarmonyLib;
using Reactor;
using Reactor.Utilities.Extensions;
using Reactor.Utilities;
using System;
using System.Reflection;
using UnityEngine;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
//Some methods and code snatched from TOU so thanks to those guys for that
namespace ExtraButtons
{
    [BepInPlugin(Id, "ExtraButtons", Version)]
    [BepInProcess("Among Us.exe")]
    [BepInDependency(ReactorPlugin.Id)]
    public class ExtraButtonsPlugin : BasePlugin
    {

        public const string Version = "2.0.0";
        public const string Id = "ExtraButtons.pack";
        public Harmony Harmony { get; } = new Harmony(Id);

        public static Sprite Ready;
        public static Sprite NotReady;
        public static Sprite RaiseHand;
        public static Sprite MeetingOverlay;
        public static AudioClip CustomAudio;


        public override void Load()
        {
            LoadAssetBundle();
            Ready = CreateSprite("ExtraButtons.Assets.ready_button.png");
            NotReady = CreateSprite("ExtraButtons.Assets.notreadybutton.png");
            RaiseHand = CreateSprite("ExtraButtons.Assets.raise_hand_glow_button.png");
            MeetingOverlay = CreateSprite("ExtraButtons.Assets.hand_raise_overlay.png");

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
                    ReadyButton.graphic.sprite = Ready;

                    UnityEngine.Object.Destroy(ReadyButton.GetComponentInChildren<TextTranslatorTMP>());
                    ReadyButton.name = "ReadyButton";
                    ReadyButton.OverrideText("");
                    ReadyButton.OverrideColor(color: Color.green);
                    ReadyButton.transform.localPosition = new Vector3((float)ReadyButton.transform.localPosition.x - 2f, (float)ReadyButton.gameObject.transform.localPosition.y, (float)ReadyButton.gameObject.transform.position.z);
                    
                    ReadyButton.graphic.SetCooldownNormalizedUvs();
                    var passiveButton = ReadyButton.GetComponent<PassiveButton>();
                    passiveButton.OnClick = new UnityEngine.UI.Button.ButtonClickedEvent();
                    passiveButton.OnClick.AddListener((Action)(() =>
                    {

                        var currentName = PlayerControl.LocalPlayer.name;
                        if (currentName.Contains(">"))
                        {
                            var modifiedName = currentName.Split(">", StringSplitOptions.RemoveEmptyEntries);
                            if (ReadyButton.graphic.color == Color.green)
                            {
                                PlayerControl.LocalPlayer.CheckName($"<color=green>{modifiedName[1]}");
                                ReadyButton.OverrideColor(Color.red);
                                ReadyButton.graphic.sprite = NotReady;
                            }
                            else
                            {
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
                                ReadyButton.OverrideColor(Color.red);
                                ReadyButton.graphic.sprite = NotReady;
                            }
                            else
                            {
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
                UnityEngine.Object.Destroy(ReadyButton);

                PlayerControl.LocalPlayer.CheckName($"{currentName}");
            }
        }       


        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
        public class OnMeetingStart
        {
            public static void Prefix(MeetingHud __instance)
            {

                CreateRaisHandButton();
                
            }

            public static void CreateRaisHandButton()
            {
                var RaiseLowerHandButton = UnityEngine.Object.Instantiate(HudManager.Instance.UseButton, HudManager.Instance.UseButton.transform.parent);
                RaiseLowerHandButton.graphic.sprite = RaiseHand;
                UnityEngine.Object.Destroy(RaiseLowerHandButton.GetComponentInChildren<TextTranslatorTMP>());
                RaiseLowerHandButton.transform.localPosition = new Vector3((float)RaiseLowerHandButton.transform.localPosition.x - 5f, (float)RaiseLowerHandButton.gameObject.transform.localPosition.y, (float)RaiseLowerHandButton.gameObject.transform.position.z);
                RaiseLowerHandButton.name = "RaiseHandButton";
                RaiseLowerHandButton.OverrideText("");
                RaiseLowerHandButton.OverrideColor(Color.green);
                if (!PlayerControl.LocalPlayer.Data.IsDead || !MeetingHud.Instance.amDead)
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
                        CustomRpcMethods.RpcSetOverlay(PlayerControl.LocalPlayer, MeetingHud.Instance);
                        RaiseLowerHandButton.OverrideColor(Color.red);
                    }
                    else
                    {
                        CustomRpcMethods.RpcRemoveOverlay(PlayerControl.LocalPlayer, MeetingHud.Instance);
                        RaiseLowerHandButton.OverrideColor(Color.green);
                    };

                }
                ));
            }

            public static void CreatePlayerButtons(string BtnAudioClipName, string BtnObjName, Sprite BtnImage)
            {
                var customAudioButton = UnityEngine.Object.Instantiate(HudManager.Instance.UseButton, HudManager.Instance.UseButton.transform.parent);
                customAudioButton.graphic.sprite = BtnImage;
                UnityEngine.Object.Destroy(customAudioButton.GetComponentInChildren<TextTranslatorTMP>());
                customAudioButton.transform.localPosition = new Vector3((float)customAudioButton.transform.localPosition.x - 10f, (float)customAudioButton.gameObject.transform.localPosition.y, (float)customAudioButton.gameObject.transform.position.z);
                customAudioButton.name = BtnObjName;
                customAudioButton.OverrideText("");
                customAudioButton.OverrideColor(Color.green);
                if (!PlayerControl.LocalPlayer.Data.IsDead || !MeetingHud.Instance.amDead)
                {
                    customAudioButton.Show();
                    customAudioButton.enabled = true;
                    customAudioButton.Awake();
                }
                customAudioButton.graphic.SetCooldownNormalizedUvs();
                var passiveButton = customAudioButton.GetComponent<PassiveButton>();
                passiveButton.OnClick = new UnityEngine.UI.Button.ButtonClickedEvent();
                passiveButton.OnClick.AddListener((Action)(() =>
                {
                    CustomRpcMethods.RpcPlayCustomAudio(PlayerControl.LocalPlayer, MeetingHud.Instance, BtnAudioClipName);
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
            var tex = CanvasUtilities.CreateEmptyTexture();
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

        private static AssetBundle AssetBundle;

        public static void LoadAssetBundle()        {
            byte[] bundleRead = Assembly.GetCallingAssembly().GetManifestResourceStream("ExtraButtons.Assets.audioassets").ReadFully();
            AssetBundle = AssetBundle.LoadFromMemory(bundleRead);}
        public static UnityEngine.Object LoadAsset(string name)
            => AssetBundle.LoadAsset(name);
        public static AudioClip GetAudioClip(string name)
                => LoadAsset(name).Cast<GameObject>().GetComponent<AudioSource>().clip;
    }
}
