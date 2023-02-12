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
using System.Linq;
using System.Collections.Generic;
//Some methods and code snatched from TOU so thanks to those guys for that
namespace ExtraThings
{
    [BepInPlugin(Id, "ExtraThings", Version)]
    [BepInProcess("Among Us.exe")]
    [BepInDependency(ReactorPlugin.Id)]
    public class ExtraThingsPlugin : BasePlugin
    {

        public const string Version = "1.0.0";
        public const string Id = "ExtraThings.pack";
        public Harmony Harmony { get; } = new Harmony(Id);

        public static Sprite Ready;
        public static Sprite NotReady;
        public static Sprite RaiseHand;
        public static Sprite MeetingOverlay;
        public static AudioClip CustomAudio;
        public static Material MagicShader;

        public override void Load()
        {
            LoadAssetBundle();
            Ready = CreateSprite("ExtraThings.Assets.ready_button.png");
            NotReady = CreateSprite("ExtraThings.Assets.notreadybutton.png");
            RaiseHand = CreateSprite("ExtraThings.Assets.raise_hand_glow_button.png");
            MeetingOverlay = CreateSprite("ExtraThings.Assets.hand_raise_overlay.png");

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
                var CHLog = new ManualLogSource("ExtraThings");
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
                var CHLog = new ManualLogSource("ExtraThings");
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

        private static AssetBundle AudioAssetBundle;
        private static AssetBundle HatAssetBundle;

        public static void LoadAssetBundle()
        {
            byte[] audioBundleRead = Assembly.GetCallingAssembly().GetManifestResourceStream("ExtraThings.Assets.audioassets").ReadFully();
            byte[] hatBundleRead = Assembly.GetCallingAssembly().GetManifestResourceStream("ExtraThings.Assets.hatassetbundle").ReadFully();
            AudioAssetBundle = AssetBundle.LoadFromMemory(audioBundleRead);
            HatAssetBundle = AssetBundle.LoadFromMemory(hatBundleRead);
        }
        public static UnityEngine.Object LoadAudioAsset(string name) => AudioAssetBundle.LoadAsset(name);
        public static UnityEngine.Object LoadHatAsset(string name) => HatAssetBundle.LoadAsset(name);

        public static AudioClip GetAudioClip(string name)
                => LoadAudioAsset(name).Cast<GameObject>().GetComponent<AudioSource>().clip;
        public struct AuthorData
        {
            public string AuthorName;
            public string HatName;
            public string FloorHatName;
            public string ClimbHatName;
            public string LeftImageName;
            public string BackImageName;
            public bool NoBounce;
            public bool altShader;
        }
        //Be sure to spell everything right or it will not load all hats after spelling error
        //Must be prefab name not name of asset for hatname
        public static List<AuthorData> authorDatas = new List<AuthorData>()
        {
            new AuthorData {AuthorName = "Berg", HatName = "birdhead", NoBounce = false},
            //new AuthorData {AuthorName = "Berg", HatName = "Army", NoBounce = false},
            new AuthorData {AuthorName = "Berg", HatName = "Navy", NoBounce = false},
            new AuthorData {AuthorName = "Berg", HatName = "Marine", NoBounce = false},
            new AuthorData {AuthorName = "Berg", HatName = "Airforce", NoBounce = false},
            new AuthorData {AuthorName = "Berg", HatName = "a_pretty_sus", LeftImageName = "a_pretty_sus_left", NoBounce = false},
            new AuthorData {AuthorName = "Berg", HatName = "ParadoxMonkey", NoBounce = false},
            new AuthorData {AuthorName = "Berg", HatName = "Gina", NoBounce = false},
            new AuthorData {AuthorName = "Berg", HatName = "imsus", LeftImageName = "imsus_left", NoBounce = false},
            new AuthorData {AuthorName = "Berg", HatName = "blackbirdhead", NoBounce = false },
            new AuthorData {AuthorName = "Berg", HatName = "jess", NoBounce = false,altShader = true},
            new AuthorData {AuthorName = "Berg", HatName = "murderghost" , NoBounce = false},
            new AuthorData {AuthorName = "Berg", HatName = "odaidenhat", NoBounce = false},
            new AuthorData {AuthorName = "Berg", HatName = "Omega", NoBounce = false},
            new AuthorData {AuthorName = "Berg", HatName = "reapercostume",FloorHatName ="reaperdead",ClimbHatName = "reaperclimb",LeftImageName = "reapercostumeleft", NoBounce = false},
            new AuthorData {AuthorName = "Berg", HatName = "reapermask",FloorHatName ="reaperdead",ClimbHatName = "reaperclimb",LeftImageName = "reapermaskleft", NoBounce = false},
            new AuthorData {AuthorName = "Berg", HatName = "viking", NoBounce = false},
            new AuthorData {AuthorName = "Berg", HatName = "vikingbeer", NoBounce = false},
            new AuthorData {AuthorName = "Berg", HatName = "pineapple", NoBounce = false},
            //new AuthorData {AuthorName = "Berg", HatName = "willhair", NoBounce = false},
            new AuthorData {AuthorName = "Wong", HatName = "vader",FloorHatName ="vaderdead",ClimbHatName = "vaderclimb", NoBounce = false},
            new AuthorData {AuthorName = "Berg", HatName = "unclesam",FloorHatName ="unclesamdead", NoBounce = false},
            new AuthorData {AuthorName = "NightRaiderTea", HatName = "Bunpix", NoBounce = true},
            new AuthorData {AuthorName = "NightRaiderTea", HatName = "Cadbury", NoBounce = true},
            new AuthorData {AuthorName = "NightRaiderTea", HatName = "CatEars", NoBounce = true,altShader = true},
            new AuthorData {AuthorName = "Angel", HatName = "dirtybirb", NoBounce = false},
            new AuthorData {AuthorName = "NightRaiderTea", HatName = "DJ", NoBounce = true},
            new AuthorData {AuthorName = "NightRaiderTea", HatName = "EnbyScarf", NoBounce = false},
            new AuthorData {AuthorName = "NightRaiderTea", HatName = "Happy", NoBounce = false, altShader = true},
            new AuthorData {AuthorName = "NightRaiderTea", HatName = "Carla", NoBounce = false, altShader = true},
            new AuthorData {AuthorName = "NightRaiderTea", HatName = "Pika", NoBounce = false, altShader = true},
            new AuthorData {AuthorName = "NightRaiderTea", HatName = "Gun", NoBounce = false, altShader = true},
            new AuthorData {AuthorName = "NightRaiderTea", HatName = "Espeon", NoBounce = true},
            new AuthorData {AuthorName = "NightRaiderTea", HatName = "Gwendolyn", NoBounce = true},
            new AuthorData {AuthorName = "NightRaiderTea", HatName = "Jester", NoBounce = true},
            new AuthorData {AuthorName = "NightRaiderTea", HatName = "PizzaRod", NoBounce = true},
            new AuthorData {AuthorName = "NightRaiderTea", HatName = "Sombra", NoBounce = true},
            new AuthorData {AuthorName = "NightRaiderTea", HatName = "Sprxk", NoBounce = true},
            new AuthorData {AuthorName = "NightRaiderTea", HatName = "Swole", NoBounce = false},
            new AuthorData {AuthorName = "NightRaiderTea", HatName = "TransScarf", NoBounce = false},
            new AuthorData {AuthorName = "NightRaiderTea", HatName = "Unicorn", NoBounce = true},
            new AuthorData {AuthorName = "NightRaiderTea", HatName = "Wings", NoBounce = false},
            new AuthorData {AuthorName = "Paradox", HatName = "Dino", NoBounce = true, altShader = true},
            new AuthorData {AuthorName = "Berg", HatName = "Ugg", NoBounce = false},
            new AuthorData {AuthorName = "NightRaiderTea", HatName = "SilverSylveon", NoBounce = false},
            new AuthorData {AuthorName = "NightRaiderTea", HatName = "DownyCrake", NoBounce = false},
            new AuthorData {AuthorName = "NightRaiderTea", HatName = "Ram", NoBounce = false , altShader = true},
            new AuthorData {AuthorName = "NightRaiderTea", HatName = "Kitsune", NoBounce = true, altShader = true},
            new AuthorData {AuthorName = "NightRaiderTea", HatName = "GlitchedSwole", NoBounce = true, altShader = true},
            new AuthorData {AuthorName = "NightRaiderTea", HatName = "TigerShark", NoBounce = true, altShader = true},
            new AuthorData {AuthorName = "NightRaiderTea", HatName = "Eevee_Female", NoBounce = false,altShader = true},
            new AuthorData {AuthorName = "NightRaiderTea", HatName = "Eevee_Male", NoBounce = false,altShader = true},
            new AuthorData {AuthorName = "NightRaiderTea", HatName = "Crowned_Bluebelle", NoBounce = false},
            new AuthorData {AuthorName = "NightRaiderTea", HatName = "Bluebow", NoBounce = false},
            new AuthorData {AuthorName = "NightRaiderTea", HatName = "Boneskinner", NoBounce = false},
            new AuthorData {AuthorName = "NightRaiderTea", HatName = "DjDolphin", NoBounce = false,altShader = true},
            new AuthorData {AuthorName = "NightRaiderTea", HatName = "OzzaMask", NoBounce = false,altShader = true},
            new AuthorData {AuthorName = "NightRaiderTea", HatName = "HornsofDevils", NoBounce = false},
            new AuthorData {AuthorName = "Berg", HatName = "Syziggy",LeftImageName = "syziggyleft" ,NoBounce = false},
            new AuthorData {AuthorName = "Berg", HatName = "Ereks",NoBounce = false,altShader = true},
            new AuthorData {AuthorName = "Berg", HatName = "Ravenlord",BackImageName = "Ravenlordback",NoBounce = false,altShader = false}
        };

        internal static Dictionary<int, AuthorData> IdToData = new Dictionary<int, AuthorData>();

        private static bool _customHatsLoaded = false;
        [HarmonyPatch(typeof(HatManager), nameof(HatManager.GetHatById))]
        public static class AddCustomHats
        {

            public static void Prefix(HatManager __instance)
            {
                var CHLog = new ManualLogSource("HatPack");
                BepInEx.Logging.Logger.Sources.Add(CHLog);
                if (!_customHatsLoaded)
                {
                    var allHats = __instance.allHats.ToList();
                    CHLog.LogInfo("Adding hats from hatpack");
                    foreach (var data in authorDatas)
                    {
                        HatID++;
                        try
                        {

                            if (data.FloorHatName != null && data.ClimbHatName != null && data.LeftImageName != null)
                            {
                                CHLog.LogInfo("Adding " + data.HatName + " and associated floor/climb hats/left image");
                                if (data.NoBounce)
                                {
                                    if (data.altShader == true)
                                    {
                                        CHLog.LogInfo("Adding " + data.HatName + " with Alt shaders and bounce");
                                        allHats.Add(CreateHat(GetSprite(data.HatName), data.AuthorName, GetSprite(data.ClimbHatName), GetSprite(data.FloorHatName), leftimage: null, back: null, true, true));
                                    }
                                    else
                                    {
                                        CHLog.LogInfo("Adding " + data.HatName + " with bounce enabled");
                                        allHats.Add(CreateHat(GetSprite(data.HatName), data.AuthorName, climb: GetSprite(data.ClimbHatName), floor: GetSprite(data.FloorHatName), leftimage: GetSprite(data.LeftImageName), back: null, true, false));
                                    }
                                }
                                else
                                {
                                    CHLog.LogInfo("Adding " + data.HatName + " with bounce disabled");
                                    allHats.Add(CreateHat(GetSprite(data.HatName), data.AuthorName, climb: GetSprite(data.ClimbHatName), floor: GetSprite(data.FloorHatName), leftimage: GetSprite(data.LeftImageName)));
                                }

                            }
                            else if (data.HatName != null && data.LeftImageName != null && data.ClimbHatName == null && data.FloorHatName == null)
                            {
                                if (data.NoBounce)
                                {
                                    CHLog.LogInfo("Adding " + data.HatName + " with bounce enabled and left image");
                                    allHats.Add(CreateHat(GetSprite(data.HatName), data.AuthorName, climb: null, floor: null, GetSprite(data.LeftImageName), back: null, true));
                                }
                                else
                                {
                                    //Add hat without bounce and leftimage
                                    CHLog.LogInfo("Adding " + data.HatName + " without bounce and left image");
                                    allHats.Add(CreateHat(GetSprite(data.HatName), data.AuthorName, climb: null, floor: null, GetSprite(data.LeftImageName)));
                                }
                            }
                            else if (data.HatName != null && data.BackImageName != null && data.LeftImageName == null && data.ClimbHatName == null && data.FloorHatName == null)
                            {
                                if (data.NoBounce)
                                {
                                    CHLog.LogInfo("Adding " + data.HatName + " with bounce enabled and back image");
                                    allHats.Add(CreateHat(GetSprite(data.HatName), data.AuthorName, climb: null, floor: null, leftimage: null, back: GetSprite(data.BackImageName), true));
                                }
                                else
                                {
                                    //Add hat without bounce and back image
                                    CHLog.LogInfo("Adding " + data.HatName + " without bounce and back image");
                                    allHats.Add(CreateHat(GetSprite(data.HatName), data.AuthorName, climb: null, floor: null, leftimage: null, GetSprite(data.BackImageName)));
                                }
                            }
                            else
                            {
                                if (data.altShader == true)
                                {
                                    CHLog.LogInfo("Adding " + data.HatName + " with Alt shaders");
                                    allHats.Add(CreateHat(GetSprite(data.HatName), data.AuthorName, climb: null, floor: null, leftimage: null, back: null, data.NoBounce, data.altShader));
                                }
                                else
                                {
                                    CHLog.LogInfo("Adding " + data.HatName);
                                    allHats.Add(CreateHat(GetSprite(data.HatName), data.AuthorName));
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            CHLog.LogInfo("An exception occured " + ex.InnerException.Message);
                        }
                        IdToData.Add(HatManager.Instance.allHats.Count + HatID, data);
                        _customHatsLoaded = true;
                    }
                    _customHatsLoaded = true;
                    __instance.allHats = allHats.ToArray();
                }
            }

            public static Sprite GetSprite(string name)
                => LoadHatAsset(name).Cast<GameObject>().GetComponent<SpriteRenderer>().sprite;

            public static int HatID = 0;
            /// <summary>
            /// Creates hat based on specified values
            /// </summary>
            /// <param name="sprite"></param>
            /// <param name="author"></param>
            /// <param name="climb"></param>
            /// <param name="floor"></param>
            /// <param name="leftimage"></param>
            /// <param name="bounce"></param>
            /// <param name="altshader"></param>
            /// <returns>HatBehaviour</returns>
            private static HatData CreateHat(Sprite sprite, string author, Sprite climb = null, Sprite floor = null, Sprite leftimage = null, Sprite back = null, bool bounce = false, bool altshader = false)
            {
                //Borrowed from Other Roles to get hats alt shaders to work
                if (MagicShader == null)
                {
                    Material hatShader = DestroyableSingleton<HatManager>.Instance.PlayerMaterial;
                    MagicShader = hatShader;
                }

                HatData newHat = ScriptableObject.CreateInstance<HatData>();
                newHat.hatViewData.viewData = ScriptableObject.CreateInstance<HatViewData>();
                newHat.name = $"{sprite.name} (by {author})";
                newHat.StoreName = "HatPack";
                newHat.hatViewData.viewData.MainImage = sprite;
                newHat.hatViewData.viewData.BackImage = back;
                newHat.hatViewData.viewData.LeftBackImage = back;
                newHat.ProductId = "hat_" + sprite.name.Replace(' ', '_');
                newHat.BundleId = "hat_" + sprite.name.Replace(' ', '_');
                newHat.displayOrder = HatID;
                newHat.InFront = true;
                newHat.NoBounce = bounce;
                newHat.hatViewData.viewData.FloorImage = floor;
                newHat.hatViewData.viewData.ClimbImage = climb;
                newHat.Free = true;
                newHat.hatViewData.viewData.LeftMainImage = leftimage;
                newHat.ChipOffset = new Vector2(-0.1f, 0.4f);
                if (altshader == true) { newHat.hatViewData.viewData.AltShader = MagicShader; }

                return newHat;
            }
        }
    }
}

