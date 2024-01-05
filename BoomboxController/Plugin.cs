using BepInEx;
using BepInEx.Configuration;
using DunGen;
using GameNetcodeStuff;
using HarmonyLib;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Design;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.Networking;
using UnityEngine.PlayerLoop;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static IngamePlayerSettings;
using static System.Runtime.CompilerServices.RuntimeHelpers;
using static UnityEngine.InputSystem.DefaultInputActions;
using static UnityEngine.InputSystem.InputActionRebindingExtensions;
using static UnityEngine.UIElements.StylePropertyAnimationSystem;

namespace BoomboxController
{
    [BepInPlugin("KoderTech.BoomboxController", "BoomboxController", "1.1.5")]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin instance;

        private static Harmony HarmonyLib;

        public static Configs config;

        private void Awake()
        {
            instance = this;
            if (File.Exists(@$"BoomboxController\lang\boombox_ru.cfg")) File.Delete(@$"BoomboxController\lang\boombox_ru.cfg");
            if (File.Exists(@$"BoomboxController\lang\boombox_en.cfg")) File.Delete(@$"BoomboxController\lang\boombox_en.cfg");
            config = new Configs();
            config.GetConfig();
            switch (config.languages.Value.ToLower())
            {
                case "ru":
                    config.GetLang().GetConfigRU();
                    break;
                case "en":
                    config.GetLang().GetConfigEN();
                    break;
            }
            WriteLogo();
            if (!Directory.Exists(@$"BoomboxController\lang")) Directory.CreateDirectory(@$"BoomboxController\lang");
            if (!Directory.Exists(@$"BoomboxController\other")) Directory.CreateDirectory(@$"BoomboxController\other");
            if (!Directory.Exists(@$"BoomboxController\other\playlist")) Directory.CreateDirectory(@$"BoomboxController\other\playlist");
            if (!File.Exists(@$"BoomboxController\other\ffmpeg.exe"))
            {
                if (File.Exists(@$"BoomboxController\other\ffmpeg-master-latest-win64-gpl.zip")) Unpacking();
                else
                {
                    Thread thread = new Thread(() => DownloadFilesToUnpacking(new Uri("https://github.com/BtbN/FFmpeg-Builds/releases/download/latest/ffmpeg-master-latest-win64-gpl.zip"), @"BoomboxController\other\ffmpeg-master-latest-win64-gpl.zip"));
                    thread.Start();
                }
            }
            HarmonyLib = new Harmony("com.kodertech.BoomboxController");
            HarmonyLib.PatchAll(typeof(BoomboxController));
        }

        public void WriteLogo()
        {
            Logger.LogInfo(@$" /$$$$$$$                                    /$$$$$$$                             /$$$$$$                        /$$                         /$$ /$$                    ");
            Logger.LogInfo(@$"| $$__  $$                                  | $$__  $$                           /$$__  $$                      | $$                        | $$| $$                    ");
            Logger.LogInfo(@$"| $$  \ $$  /$$$$$$   /$$$$$$  /$$$$$$/$$$$ | $$  \ $$  /$$$$$$  /$$   /$$      | $$  \__/  /$$$$$$  /$$$$$$$  /$$$$$$    /$$$$$$   /$$$$$$ | $$| $$  /$$$$$$   /$$$$$$ ");
            Logger.LogInfo(@$"| $$$$$$$  /$$__  $$ /$$__  $$| $$_  $$_  $$| $$$$$$$  /$$__  $$|  $$ /$$/      | $$       /$$__  $$| $$__  $$|_  $$_/   /$$__  $$ /$$__  $$| $$| $$ /$$__  $$ /$$__  $$");
            Logger.LogInfo(@$"| $$__  $$| $$  \ $$| $$  \ $$| $$ \ $$ \ $$| $$__  $$| $$  \ $$ \  $$$$/       | $$      | $$  \ $$| $$  \ $$  | $$    | $$  \__/| $$  \ $$| $$| $$| $$$$$$$$| $$  \__/");
            Logger.LogInfo(@$"| $$  \ $$| $$  | $$| $$  | $$| $$ | $$ | $$| $$  \ $$| $$  | $$  >$$  $$       | $$    $$| $$  | $$| $$  | $$  | $$ /$$| $$      | $$  | $$| $$| $$| $$_____/| $$      ");
            Logger.LogInfo(@$"| $$$$$$$/|  $$$$$$/|  $$$$$$/| $$ | $$ | $$| $$$$$$$/|  $$$$$$/ /$$/\  $$      |  $$$$$$/|  $$$$$$/| $$  | $$  |  $$$$/| $$      |  $$$$$$/| $$| $$|  $$$$$$$| $$      ");
            Logger.LogInfo(@$"|_______/  \______/  \______/ |__/ |__/ |__/|_______/  \______/ |__/  \__/       \______/  \______/ |__/  |__/   \___/  |__/       \______/ |__/|__/ \_______/|__/      ");
        }

        public static void DownloadFilesToUnpacking(Uri uri, string filename)
        {
            WebClient web = new WebClient();
            web.DownloadFileCompleted += Web_DownloadFileCompletedToUnpacking;
            web.DownloadFileAsync(uri, filename);
        }

        public static void DownloadFiles(Uri uri, string filename)
        {
            WebClient web = new WebClient();
            web.DownloadFileCompleted += Web_DownloadFileCompleted;
            web.DownloadFileAsync(uri, filename);
        }

        private static void Web_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            Thread.CurrentThread.Abort();
        }

        private static void Web_DownloadFileCompletedToUnpacking(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            Unpacking();
            if (!File.Exists(@$"BoomboxController\other\yt-dlp.exe"))
            {
                Thread thread = new Thread(() => DownloadFiles(new Uri("https://github.com/yt-dlp/yt-dlp/releases/download/2023.11.16/yt-dlp.exe"), @"BoomboxController\other\yt-dlp.exe"));
                thread.Start();
            }
            Thread.CurrentThread.Abort();
        }

        public static void Unpacking()
        {
            using (ZipArchive zip = ZipFile.OpenRead(@"BoomboxController\other\ffmpeg-master-latest-win64-gpl.zip"))
            {
                foreach (ZipArchiveEntry entry in zip.Entries)
                {
                    if (entry.Name.Equals("ffmpeg.exe")) entry.ExtractToFile(Path.Combine(@"BoomboxController\other", entry.Name));
                }
            }
            File.Delete(@"BoomboxController\other\ffmpeg-master-latest-win64-gpl.zip");
        }

        public void Log(object message)
        {
            Logger.LogInfo(message);
        }
    }

    public class QuitManager : MonoBehaviour
    {
        private void OnApplicationQuit()
        {
            DeleteFile();
        }

        public void DeleteFile()
        {
            foreach (FileInfo track in new DirectoryInfo(@"BoomboxController\other\playlist").GetFiles())
            {
                track.Delete();
            }
            FileInfo[] file = new DirectoryInfo(@"BoomboxController\other").GetFiles("*.mp3");
            if (file.Length == 1)
            {
                File.Delete(@$"BoomboxController\other\{file[0].Name}");
            }
        }
    }

    public class Configs
    {
        public ConfigEntry<bool> requstbattery;
        public ConfigEntry<bool> pocketitem;
        public ConfigEntry<string> languages;
        public ConfigEntry<string> body;
        public ConfigEntry<string> otherelem;

        public static Lang lang = new Lang();

        public void GetConfig()
        {
            var customFile = new ConfigFile(@"BoomboxController\boombox_controller.cfg", true);
            requstbattery = customFile.Bind("General.Toggles", "RequestBattery", false, "Enable/disable boombox battery (true = Enable; false = Disable)");
            pocketitem = customFile.Bind("General.Toggles", "PocketItem", true, "Enable/disable music in your pocket. (true = Enable; false = Disable)");
            languages = customFile.Bind("General", "Languages", "en", "EN/RU");
            body = customFile.Bind("Visual", "Body", "#FFFFFF", "Color body Boombox");
            otherelem = customFile.Bind("Visual", "Other", "#000000", "Color Other Elements Boombox");
        }

        public Lang GetLang()
        {
            return lang;
        }

        public class Lang
        {
            public ConfigEntry<string> main_1;
            public ConfigEntry<string> main_2;
            public ConfigEntry<string> main_3;
            public ConfigEntry<string> main_4;
            public ConfigEntry<string> main_5;
            public ConfigEntry<string> main_6;
            public ConfigEntry<string> main_7;
            public ConfigEntry<string> main_8;
            public ConfigEntry<string> main_9;
            public ConfigEntry<string> main_10;
            public ConfigEntry<string> main_11;
            public ConfigEntry<string> main_12;
            public ConfigEntry<string> main_13;
            public ConfigEntry<string> main_14;

            public void GetConfigRU()
            {
                var customFile = new ConfigFile(@"BoomboxController\lang\boombox_ru.cfg", true);
                main_1 = customFile.Bind("General", "Main_1", "Пожалуйста, подождите, загружаются дополнительные библиотеки, чтобы модификация заработала.");
                main_2 = customFile.Bind("General", "Main_2", "Взять BoomBox[1.1.5] : [E]\n@2 - @3\n@1 громкость\nСейчас играет: @4\nДоступных треков: @5");
                main_3 = customFile.Bind("General", "Main_3", "Все дополнительные библиотеки загружены, теперь вы можете использовать команды для бумбокса.");
                main_4 = customFile.Bind("General", "Main_4", "Подождите, трек еще загружается!");
                main_5 = customFile.Bind("General", "Main_5", "Команды:\n/bplay - Проиграть музыку\n/btime - Изменить позицию песни\n/bvolume - Изменить громкость трека");
                main_6 = customFile.Bind("General", "Main_6", "Введите правильный URL-адрес!");
                main_7 = customFile.Bind("General", "Main_7", "Пожалуйста подождите...");
                main_8 = customFile.Bind("General", "Main_8", "Трек был загружен в бумбокс");
                main_9 = customFile.Bind("General", "Main_9", "@1 изменил громкость трека @2");
                main_10 = customFile.Bind("General", "Main_10", "Введите правильную громкость трека (пример: 0, 10, 20, 30...)!");
                main_11 = customFile.Bind("General", "Main_11", "Ссылка недействительная!");
                main_12 = customFile.Bind("General", "Main_12", "Позиция трека изменена на @1!");
                main_13 = customFile.Bind("General", "Main_13", "Загрузка трека отменена!");
                main_14 = customFile.Bind("General", "Main_14", "Текущий трек был переключен на: @1!");
            }

            public void GetConfigEN()
            {
                var customFile = new ConfigFile(@"BoomboxController\lang\boombox_en.cfg", true);
                main_1 = customFile.Bind("General", "Main_1", "Please wait, additional libraries are being loaded for the modification to work.");
                main_2 = customFile.Bind("General", "Main_2", "Pickup BoomBox[1.1.5] : [E]\n@2 - @3\n@1 volume\nNow playing: @4\nAvailable tracks: @5");
                main_3 = customFile.Bind("General", "Main_3", "All libraries have loaded, now you can use the boombox commands.");
                main_4 = customFile.Bind("General", "Main_4", "Another track is being uploaded to the boombox!");
                main_5 = customFile.Bind("General", "Main_5", "Commands:\n/bplay - Play music\n/btime - Change the position of the song\n/bvolume - Change Boombox volume");
                main_6 = customFile.Bind("General", "Main_6", "Enter the correct URL!");
                main_7 = customFile.Bind("General", "Main_7", "Please wait...");
                main_8 = customFile.Bind("General", "Main_8", "The track was uploaded to the boombox");
                main_9 = customFile.Bind("General", "Main_9", "@1 changed the volume @2 of the boombox.");
                main_10 = customFile.Bind("General", "Main_10", "Enter the correct Volume (example: 0, 10, 20, 30...)!");
                main_11 = customFile.Bind("General", "Main_11", "Link is invalid!");
                main_12 = customFile.Bind("General", "Main_12", "Track position changed to @1!");
                main_13 = customFile.Bind("General", "Main_13", "Track download canceled!");
                main_14 = customFile.Bind("General", "Main_14", "The current track has been switched to: @1!");
            }
        }
    }

    public class BoomboxController : MonoBehaviour
    {
        public class Cache
        {
            public float Volume { get; set; }
            public string UpButton { get; set; }
            public string DownButton { get; set; }
        }

        public static bool startMusics = true;
        public static AudioBoomBox bom;
        public static VisualBoombox vbom;
        public static int timesPlayedWithoutTurningOff = 0;
        public static BoomboxItem boomboxItem = new BoomboxItem();
        public static int isSendingItemRPC = 0;
        public static string LastMessage;
        public static bool LoadingMusicBoombox = false;
        public static bool LoadingLibrary = false;
        public static string LastnameOfUserWhoTyped;
        public static double curretTime = 0;
        public static double totalTime = 0;
        public static Thread thread;
        public static bool isplayList = false;
        public static int Id = 0;
        public static int totalTack = 0;
        public static int currectTrack = 0;
        public static string NameTrack;
        public static AudioClip[] musicList;
        public static QuitManager quit;
        public static QuitManager quits;
        public static string[] sumbols = { "+" };
        public static KeyControl up = null;
        public static KeyControl down = null;

        [HarmonyPatch(typeof(GameNetworkManager), "OnEnable")]
        [HarmonyPostfix]
        public static void OnEnable(GameNetworkManager __instance)
        {
            using (StreamWriter sw = new StreamWriter(@"BoomboxController\logReport.txt"))
            {
                sw.WriteLine($"Game Version: {__instance.gameVersionNum}");
                sw.WriteLine($"Plugins: {new DirectoryInfo(@"BepInEx\plugins").GetFiles().Length}");
                foreach(var item in new DirectoryInfo(@"BepInEx\plugins").GetFiles())
                {
                    sw.WriteLine(item.Name);
                }
            }
        }

        [HarmonyPatch(typeof(MenuManager), "OnEnable")]
        [HarmonyPrefix]
        public static void OnEnable()
        {
            LastMessage = string.Empty;
            if(quit == null)
            {
                quit = new GameObject("QuitManager").AddComponent<QuitManager>();
                UnityEngine.Object.DontDestroyOnLoad((UnityEngine.Object)(object)quit);
            }
        }

        [HarmonyPatch(typeof(StartOfRound), "OnEnable")]
        [HarmonyPrefix]
        public static void OnEnables()
        {
            if (quits == null)
            {
                quits = new GameObject("QuitManager").AddComponent<QuitManager>();
                UnityEngine.Object.DontDestroyOnLoad((UnityEngine.Object)(object)quits);
            }
        }

        [HarmonyPatch(typeof(PlayerControllerB), "SetHoverTipAndCurrentInteractTrigger")]
        [HarmonyPrefix]
        public static bool SetHoverTipAndCurrentInteractTrigger(PlayerControllerB __instance, ref RaycastHit ___hit, ref Ray ___interactRay, ref int ___playerMask, ref int ___interactableObjectsMask)
        {
            if (!__instance.isGrabbingObjectAnimation)
            {
                ___interactRay = new Ray(__instance.gameplayCamera.transform.position, __instance.gameplayCamera.transform.forward);
                if (Physics.Raycast(___interactRay, out ___hit, __instance.grabDistance, ___interactableObjectsMask) && ___hit.collider.gameObject.layer != 8)
                {
                    string text = ___hit.collider.tag;
                    if (!(text == "PhysicsProp"))
                    {
                        if (text == "InteractTrigger")
                        {
                            InteractTrigger component = ___hit.transform.gameObject.GetComponent<InteractTrigger>();
                            if (component != __instance.previousHoveringOverTrigger && __instance.previousHoveringOverTrigger != null)
                            {
                                __instance.previousHoveringOverTrigger.isBeingHeldByPlayer = false;
                            }
                            if (!(component == null))
                            {
                                __instance.hoveringOverTrigger = component;
                                if (!component.interactable)
                                {
                                    __instance.cursorIcon.sprite = component.disabledHoverIcon;
                                    __instance.cursorIcon.enabled = component.disabledHoverIcon != null;
                                    __instance.cursorTip.text = component.disabledHoverTip;
                                }
                                else if (component.isPlayingSpecialAnimation)
                                {
                                    __instance.cursorIcon.enabled = false;
                                    __instance.cursorTip.text = "";
                                }
                                else if (__instance.isHoldingInteract)
                                {
                                    if (__instance.twoHanded)
                                    {
                                        __instance.cursorTip.text = "[Hands full]";
                                    }
                                    else if (!string.IsNullOrEmpty(component.holdTip))
                                    {
                                        __instance.cursorTip.text = component.holdTip;
                                    }
                                }
                                else
                                {
                                    __instance.cursorIcon.enabled = true;
                                    __instance.cursorIcon.sprite = component.hoverIcon;
                                    __instance.cursorTip.text = component.hoverTip;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (FirstEmptyItemSlot(__instance) == -1)
                        {
                            __instance.cursorTip.text = "Inventory full!";
                        }
                        else
                        {
                            GrabbableObject component2 = ___hit.collider.gameObject.GetComponent<GrabbableObject>();
                            if (!GameNetworkManager.Instance.gameHasStarted && !component2.itemProperties.canBeGrabbedBeforeGameStart && StartOfRound.Instance.testRoom == null)
                            {
                                __instance.cursorTip.text = "(Cannot pickup until ship is landed)";
                            }
                            if (___hit.transform.gameObject.GetComponent<BoomboxItem>() != null)
                            {
                                float volume = boomboxItem.boomboxAudio.volume;
                                if (component2 != null && !string.IsNullOrEmpty(component2.customGrabTooltip))
                                {
                                    __instance.cursorTip.text = component2.customGrabTooltip;
                                }
                                if (GameNetworkManager.Instance.gameHasStarted)
                                {
                                    if (___hit.transform.name.ToString() == "Boombox(Clone)")
                                    {
                                        if (!File.Exists(@"BoomboxController\other\ffmpeg.exe") || !File.Exists(@"BoomboxController\other\yt-dlp.exe"))
                                        {
                                            __instance.cursorTip.text = Plugin.config.GetLang().main_1.Value;
                                        }
                                        else
                                        {
                                            if (!boomboxItem.isPlayingMusic)
                                            {
                                                curretTime = 0;
                                                totalTime = 0;
                                            }
                                            int currect_ost = (int)curretTime % 3600;
                                            string currect_hours = Mathf.Floor((int)curretTime / 3600).ToString("00");
                                            string currect_minutes = Mathf.Floor((int)currect_ost / 60).ToString("00");
                                            string currect_seconds = Mathf.Floor((int)currect_ost % 60).ToString("00");
                                            int total_ost = (int)totalTime % 3600;
                                            string total_hours = Mathf.Floor((int)totalTime / 3600).ToString("00");
                                            string total_minutes = Mathf.Floor((int)total_ost / 60).ToString("00");
                                            string total_seconds = Mathf.Floor((int)total_ost % 60).ToString("00");
                                            if(Plugin.config.languages.Value == "en")
                                            {
                                                if (musicList == null || musicList.Length > 0)
                                                {
                                                    string playname = boomboxItem.isPlayingMusic ? "[Home]" : "Nothing";
                                                    __instance.cursorTip.text = Plugin.config.GetLang().main_2.Value.Replace("@1", $"{Math.Round(volume * 100)}%").Replace("@2", $"{currect_hours}:{currect_minutes}:{currect_seconds}").Replace("@3", $"{total_hours}:{total_minutes}:{total_seconds}").Replace("@4", $"{playname}").Replace("@5", $"{totalTack}") + $"\nIncrease volume [{(up == null ? "PU" : up.displayName)}]\nDecrease volume [{(down == null ? "PD" : down.displayName)}]";
                                                }
                                            }
                                            if (Plugin.config.languages.Value == "ru")
                                            {
                                                if(musicList == null || musicList.Length > 0)
                                                {
                                                    string playname = boomboxItem.isPlayingMusic ? "[Home]" : "Нечего";
                                                    __instance.cursorTip.text = Plugin.config.GetLang().main_2.Value.Replace("@1", $"{Math.Round(volume * 100)}%").Replace("@2", $"{currect_hours}:{currect_minutes}:{currect_seconds}").Replace("@3", $"{total_hours}:{total_minutes}:{total_seconds}").Replace("@4", $"{playname}").Replace("@5", $"{totalTack}") + $"\nУвеличить громкость [{(up == null ? "PU" : up.displayName)}]\nУменьшить громкость [{(down == null ? "PD" : down.displayName)}]";
                                                }
                                            }
                                            if (up != null)
                                            {
                                                if (up.wasPressedThisFrame)
                                                {
                                                    if (volume < 1.0f)
                                                    {
                                                        float vol = volume + 0.1f;
                                                        boomboxItem.boomboxAudio.volume = vol;
                                                        SaveCache(vol, up == null ? null : up.displayName, down == null ? null : down.displayName);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (Keyboard.current.pageUpKey.wasPressedThisFrame)
                                                {
                                                    if (volume < 1.0f)
                                                    {
                                                        float vol = volume + 0.1f;
                                                        boomboxItem.boomboxAudio.volume = vol;
                                                        SaveCache(vol, up == null ? null : up.displayName, down == null ? null : down.displayName);
                                                    }
                                                }
                                            }
                                            if (down != null)
                                            {
                                                if (down.wasPressedThisFrame)
                                                {
                                                    if (volume > 0.0f)
                                                    {
                                                        float vol = volume - 0.1f;
                                                        boomboxItem.boomboxAudio.volume = vol;
                                                        SaveCache(vol, up == null ? null : up.displayName, down == null ? null : down.displayName);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (Keyboard.current.pageDownKey.wasPressedThisFrame)
                                                {
                                                    if (volume > 0.0f)
                                                    {
                                                        float vol = volume - 0.1f;
                                                        boomboxItem.boomboxAudio.volume = vol;
                                                        SaveCache(vol, up == null ? null : up.displayName, down == null ? null : down.displayName);
                                                    }
                                                }
                                            }
                                            if (Keyboard.current.homeKey.IsPressed())
                                            {
                                                if (musicList.Length == 1) __instance.cursorTip.text = Plugin.config.GetLang().main_2.Value.Replace("@1", $"{Math.Round(volume * 100)}%").Replace("@2", $"{currect_hours}:{currect_minutes}:{currect_seconds}").Replace("@3", $"{total_hours}:{total_minutes}:{total_seconds}").Replace("@4", $"{NameTrack.Substring(0, NameTrack.Length - 4)}").Replace("@5", $"{totalTack}");
                                                if (musicList.Length > 1) __instance.cursorTip.text = Plugin.config.GetLang().main_2.Value.Replace("@1", $"{Math.Round(volume * 100)}%").Replace("@2", $"{currect_hours}:{currect_minutes}:{currect_seconds}").Replace("@3", $"{total_hours}:{total_minutes}:{total_seconds}").Replace("@4", $"{NameTrack.Replace(" ", "")}").Replace("@5", $"{totalTack}");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        __instance.cursorIcon.enabled = true;
                        __instance.cursorIcon.sprite = __instance.grabItemIcon;
                    }
                }
                else
                {
                    __instance.cursorIcon.enabled = false;
                    __instance.cursorTip.text = "";
                    if (__instance.hoveringOverTrigger != null)
                    {
                        __instance.previousHoveringOverTrigger = __instance.hoveringOverTrigger;
                    }
                    __instance.hoveringOverTrigger = null;
                }
            }
            if (StartOfRound.Instance.localPlayerUsingController)
            {
                StringBuilder stringBuilder = new StringBuilder(__instance.cursorTip.text);
                stringBuilder.Replace("[E]", "[X]");
                stringBuilder.Replace("[LMB]", "[X]");
                stringBuilder.Replace("[RMB]", "[R-Trigger]");
                stringBuilder.Replace("[F]", "[R-Shoulder]");
                stringBuilder.Replace("[Z]", "[L-Shoulder]");
                __instance.cursorTip.text = stringBuilder.ToString();
            }
            else
            {
                __instance.cursorTip.text = __instance.cursorTip.text.Replace("[LMB]", "[E]");
            }
            if (!__instance.isFreeCamera && Physics.Raycast(___interactRay, out ___hit, 5f, ___playerMask))
            {
                PlayerControllerB component3 = ___hit.collider.gameObject.GetComponent<PlayerControllerB>();
                if (component3 != null)
                {
                    component3.ShowNameBillboard();
                }
            }
            return false;
        }


        [HarmonyPatch(typeof(BoomboxItem), "Start")]
        [HarmonyPrefix]
        private static void Start(BoomboxItem __instance)
        {
            bom = new AudioBoomBox();
            vbom = new VisualBoombox();
            Color body;
            Color otherelem;
            bool bodyb = ColorUtility.TryParseHtmlString(Plugin.config.body.Value, out body);
            bool otherelemb = ColorUtility.TryParseHtmlString(Plugin.config.otherelem.Value, out otherelem);
            if (bodyb)
            {
                __instance.gameObject.GetComponent<MeshRenderer>().materials[3].color = body;
            }
            if (otherelemb)
            {
                __instance.gameObject.GetComponent<MeshRenderer>().materials[1].color = otherelem;
            }
            if (File.Exists(@"BoomboxController\back.jpg"))
            {
                System.Drawing.Image image = System.Drawing.Image.FromFile(@"BoomboxController\back.jpg");
                if(image.Width > image.Height)
                {
                    if(image.Width > 500)
                    {
                        vbom.Start(vbom.GetTexture(@"file:///" + Paths.GameRootPath + @$"\BoomboxController\back.jpg", __instance));
                    }
                }
            }
            __instance.boomboxAudio.volume = 0.5f;
            __instance.musicAudios = null;
            __instance.itemProperties.requiresBattery = Plugin.config.requstbattery.Value;
            boomboxItem = __instance;
            Cache cache = LoadCache();
            if(cache != null)
            {
                __instance.boomboxAudio.volume = cache.Volume;
                if(cache.UpButton != null)
                {
                    up = Keyboard.current.FindKeyOnCurrentKeyboardLayout(cache.UpButton);
                }
                if (cache.DownButton != null)
                {
                    down = Keyboard.current.FindKeyOnCurrentKeyboardLayout(cache.DownButton);
                }
            }
        }

        [HarmonyPatch(typeof(BoomboxItem), "StartMusic")]
        [HarmonyPrefix]
        private static bool Prefix(BoomboxItem __instance, bool startMusic, bool pitchDown, ref int ___timesPlayedWithoutTurningOff)
        {
            if (!LoadingMusicBoombox)
            {
                Plugin.instance.Log(startMusic + " startmusic");
                if (startMusic)
                {
                    boomboxItem.boomboxAudio.clip = musicList[currectTrack];
                    boomboxItem.boomboxAudio.pitch = 1f;
                    boomboxItem.boomboxAudio.Play();
                    boomboxItem.isPlayingMusic = startMusic;
                    boomboxItem.isBeingUsed = startMusic;
                    startMusics = false;
                }
                else if (boomboxItem.isPlayingMusic)
                {
                    boomboxItem.boomboxAudio.Stop();
                    boomboxItem.boomboxAudio.PlayOneShot(boomboxItem.stopAudios[UnityEngine.Random.Range(0, boomboxItem.stopAudios.Length)]);
                    timesPlayedWithoutTurningOff = 0;
                    boomboxItem.isPlayingMusic = startMusic;
                    boomboxItem.isBeingUsed = startMusic;
                    startMusics = true;
                }
            }
            return false;
        }

        [HarmonyPatch(typeof(BoomboxItem), "Update")]
        [HarmonyPrefix]
        private static void Postfix(BoomboxItem __instance, ref int ___timesPlayedWithoutTurningOff)
        {
            if (timesPlayedWithoutTurningOff <= 0)
            {
                ___timesPlayedWithoutTurningOff = 0;
            }
            timesPlayedWithoutTurningOff = ___timesPlayedWithoutTurningOff;
            if (boomboxItem.isPlayingMusic)
            {
                curretTime = boomboxItem.boomboxAudio.time;
                totalTime = boomboxItem.boomboxAudio.clip.length;
            }
            else boomboxItem.boomboxAudio.time = 0;
            if(musicList != null)
            {
                totalTack = musicList.Length;
            }
        }

        [HarmonyPatch(typeof(BoomboxItem), "PocketItem")]
        [HarmonyPrefix]
        private static bool PocketItem(BoomboxItem __instance)
        {
            if (Plugin.config.pocketitem.Value)
            {
                GrabbableObject objects = __instance.GetComponent<GrabbableObject>();
                if (objects != null)
                {
                    objects.EnableItemMeshes(enable: false);
                }
            }
            else
            {
                GrabbableObject objects = __instance.GetComponent<GrabbableObject>();
                if (objects != null)
                {
                    if (objects.IsOwner && objects.playerHeldBy != null)
                    {
                        objects.playerHeldBy.IsInspectingItem = false;
                    }
                    objects.isPocketed = true;
                    objects.EnableItemMeshes(enable: false);
                    __instance.gameObject.GetComponent<AudioSource>().PlayOneShot(objects.itemProperties.pocketSFX, 1f);
                }
                MethodInfo method = ((object)__instance).GetType().GetMethod("StartMusic", BindingFlags.Instance | BindingFlags.NonPublic);
                method.Invoke(__instance, new object[2] { false, false });
            }
            return false;
        }

        [HarmonyPatch(typeof(HUDManager), "Update")]
        [HarmonyPrefix]
        private static void Updat(HUDManager __instance)
        {
            if (File.Exists(@"BoomboxController\other\ffmpeg.exe") && File.Exists(@"BoomboxController\other\yt-dlp.exe"))
            {
                if (LoadingLibrary)
                {
                    DrawString(__instance, Plugin.config.GetLang().main_3.Value, "Boombox", "Boombox");
                    LoadingLibrary = false;
                }
            }
        }

        [HarmonyPatch(typeof(HUDManager), "Start")]
        [HarmonyPrefix]
        private static void StartPostfix(HUDManager __instance)
        {
            __instance.chatTextField.characterLimit = 200;
        }

        [HarmonyPatch(typeof(HUDManager), "AddChatMessage")]
        [HarmonyPostfix]
        private static void AddChatMessage(HUDManager __instance, string chatMessage, string nameOfUserWhoTyped)
        {
            AddChatMessageMain(__instance, chatMessage, nameOfUserWhoTyped);
        }

        public static async void AddChatMessageMain(HUDManager __instance, string chatMessage, string nameOfUserWhoTyped)
        {
            if (!File.Exists(@"BoomboxController\other\ffmpeg.exe") || !File.Exists(@"BoomboxController\other\yt-dlp.exe"))
            {
                DrawString(__instance, Plugin.config.GetLang().main_1.Value, "Boombox", nameOfUserWhoTyped);
                LoadingLibrary = true;
            }
            else
            {
                string[] vs = chatMessage.Split(' ');
                switch (vs[0].Replace("/", ""))
                {
                    case "bhelp":
                        if (Plugin.config.languages.Value.ToLower().Equals("en"))
                        {
                            DrawString(__instance, Plugin.config.GetLang().main_5.Value + "\nThe creator of the mod is KoderTech.\nWith love from Russia", "Boombox", nameOfUserWhoTyped);
                        }
                        if (Plugin.config.languages.Value.ToLower().Equals("ru"))
                        {
                            DrawString(__instance, Plugin.config.GetLang().main_5.Value + "\nСоздатель мода KoderTech.\nСпасибо что вы скачали именно этот мод)))", "Boombox", nameOfUserWhoTyped);
                        }
                        break;
                    case "bplay":
                        Regex regex = new Regex("^https?:\\/\\/(?:www\\.)?[-a-zA-Z0-9@:%._\\+~#=]{1,256}\\.[a-zA-Z0-9()]{1,6}\\b(?:[-a-zA-Z0-9()@:%_\\+.~#?&\\/=]*)$");
                        if (regex.IsMatch(vs[1]))
                        {
                            var url = vs[1].Remove(0, 8);
                            switch (url.Substring(0, url.IndexOf('/')))
                            {
                                case "www.youtube.com":
                                    if (vs[1].Contains("search_query"))
                                    {
                                        DrawString(__instance, Plugin.config.GetLang().main_6.Value, "Boombox YouTube", nameOfUserWhoTyped);
                                        break;
                                    }
                                    if (vs[1].Contains("playlist"))
                                    {
                                        boomboxItem.boomboxAudio.Stop();
                                        boomboxItem.boomboxAudio.PlayOneShot(boomboxItem.stopAudios[UnityEngine.Random.Range(0, boomboxItem.stopAudios.Length)]);
                                        timesPlayedWithoutTurningOff = 0;
                                        boomboxItem.isPlayingMusic = false;
                                        boomboxItem.isBeingUsed = false;
                                        LoadingMusicBoombox = true;
                                        if (File.Exists(@"BoomboxController\other\output.txt")) File.Delete(@"BoomboxController\other\output.txt");
                                        foreach (FileInfo track in new DirectoryInfo(@"BoomboxController\other\playlist").GetFiles())
                                        {
                                            track.Delete();
                                        }
                                        DrawString(__instance, Plugin.config.GetLang().main_7.Value, "Boombox YouTube", nameOfUserWhoTyped);
                                        if (!isplayList)
                                        {
                                            isplayList = true;
                                            await Task.Run(() =>
                                            {
                                                bool succeeded = false;
                                                Process info = new Process();
                                                info.StartInfo.FileName = @"BoomboxController\other\yt-dlp.exe";
                                                info.StartInfo.UseShellExecute = false;
                                                info.StartInfo.Arguments = $"-f bestaudio --extract-audio --audio-format mp3 --audio-quality 0 {vs[1]}";
                                                info.StartInfo.WorkingDirectory = @$"BoomboxController\other\playlist";
                                                info.StartInfo.CreateNoWindow = true;
                                                info.StartInfo.RedirectStandardOutput = true;
                                                info.Start();
                                                string output = info.StandardOutput.ReadToEnd();
                                                Id = info.Id;
                                                while (!succeeded)
                                                {
                                                    if (Process.GetProcessById(info.Id).HasExited)
                                                    {
                                                        if(new DirectoryInfo(@"BoomboxController\other\playlist").GetFiles("*.webm").Length == 0)
                                                        {
                                                            succeeded = true;
                                                            break;
                                                        }
                                                    }
                                                    System.Threading.Thread.Sleep(1000);
                                                }
                                                using (StreamWriter sw = File.CreateText(@"BoomboxController\other\output.txt"))
                                                {
                                                    sw.WriteLine(output);
                                                }
                                            });
                                            if (new DirectoryInfo(@"BoomboxController\other\playlist").GetFiles("*.mp3").Length == 0)
                                            {
                                                LoadingMusicBoombox = false;
                                                isplayList = false;
                                                DrawString(__instance, Plugin.config.GetLang().main_6.Value, "Boombox YouTube", nameOfUserWhoTyped);
                                                break;
                                            }
                                            using (StreamReader sr = new StreamReader(@"BoomboxController\other\output.txt"))
                                            {
                                                while (sr.Peek() >= 0)
                                                {
                                                    string text = sr.ReadLine();
                                                    if (text.Contains("[download] Downloading playlist:"))
                                                    {
                                                        string[] vs1 = text.Split(':');
                                                        NameTrack = vs1[1];
                                                    }
                                                }
                                            }
                                            currectTrack = 0;
                                            thread = new Thread(() => LoadPlaylist(__instance, nameOfUserWhoTyped));
                                            thread.Start();
                                        }
                                        break;
                                    }
                                    if (vs[1].Contains("list"))
                                    {
                                        DrawString(__instance, Plugin.config.GetLang().main_6.Value, "Boombox YouTube", nameOfUserWhoTyped);
                                        break;
                                    }
                                    else
                                    {
                                        boomboxItem.boomboxAudio.Stop();
                                        boomboxItem.boomboxAudio.PlayOneShot(boomboxItem.stopAudios[UnityEngine.Random.Range(0, boomboxItem.stopAudios.Length)]);
                                        timesPlayedWithoutTurningOff = 0;
                                        boomboxItem.isPlayingMusic = false;
                                        boomboxItem.isBeingUsed = false;
                                        LoadingMusicBoombox = true;
                                        FileInfo[] file = new DirectoryInfo(@"BoomboxController\other").GetFiles("*.mp3");
                                        if (file.Length == 1)
                                        {
                                            File.Delete(@$"BoomboxController\other\{file[0].Name}");
                                        }
                                        DrawString(__instance, Plugin.config.GetLang().main_7.Value, "Boombox YouTube", nameOfUserWhoTyped);
                                        if (!isplayList)
                                        {
                                            isplayList = true;
                                            await Task.Run(() =>
                                            {
                                                bool succeeded = false;
                                                bool part = false;
                                                Process info = new Process();
                                                info.StartInfo.FileName = @"BoomboxController\other\yt-dlp.exe";
                                                info.StartInfo.UseShellExecute = false;
                                                info.StartInfo.Arguments = $"-f bestaudio --extract-audio --audio-format mp3 --audio-quality 0 {vs[1]}";
                                                info.StartInfo.WorkingDirectory = @$"BoomboxController\other";
                                                info.StartInfo.CreateNoWindow = true;
                                                info.Start();
                                                Id = info.Id;
                                                while (!succeeded)
                                                {
                                                    if (part)
                                                    {
                                                        if (File.Exists(@$"BoomboxController\other\{NameTrack}"))
                                                        {
                                                            succeeded = true;
                                                            break;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        foreach (FileInfo f in new DirectoryInfo(@"BoomboxController\other").GetFiles("*.mp3"))
                                                        {
                                                            if (f.Exists)
                                                            {
                                                                NameTrack = f.Name;
                                                            }
                                                        }
                                                        if (Process.GetProcessById(info.Id).HasExited)
                                                        {
                                                            if (File.Exists(@$"BoomboxController\other\{NameTrack}"))
                                                            {
                                                                part = true;
                                                            }
                                                            else
                                                            {
                                                                DrawString(__instance, Plugin.config.GetLang().main_11.Value, "Boombox YouTube", nameOfUserWhoTyped);
                                                                break;
                                                            }
                                                        }
                                                    }
                                                    System.Threading.Thread.Sleep(1000);
                                                }
                                            });
                                            if (!File.Exists(@$"BoomboxController\other\{NameTrack}"))
                                            {
                                                LoadingMusicBoombox = false;
                                                isplayList = false;
                                                break;
                                            }
                                            bool sumbBlock = false;
                                            List<string> sumbol = new List<string>();
                                            FileInfo ext = new FileInfo(@$"BoomboxController\other\{NameTrack}");
                                            foreach(string sumb in sumbols)
                                            {
                                                if (ext.Name.Contains(sumb))
                                                {
                                                    sumbol.Add(sumb);
                                                    sumbBlock = true;
                                                }
                                            }
                                            if (sumbBlock)
                                            {
                                                string NameFile = String.Empty;
                                                foreach(string sumb in sumbol)
                                                {
                                                    NameFile = NameTrack.Replace(sumb, "");
                                                    ext.MoveTo(@$"BoomboxController\other\{NameTrack.Replace(sumb, "")}");
                                                }
                                                currectTrack = 0;
                                                bom.Start(bom.GetAudioClip(@"file:///" + Paths.GameRootPath + @$"\BoomboxController\other\{NameFile}", boomboxItem, AudioType.MPEG));
                                                DrawString(__instance, Plugin.config.GetLang().main_8.Value, "Boombox YouTube", nameOfUserWhoTyped);
                                            }
                                            else
                                            {
                                                currectTrack = 0;
                                                bom.Start(bom.GetAudioClip(@"file:///" + Paths.GameRootPath + @$"\BoomboxController\other\{NameTrack}", boomboxItem, AudioType.MPEG));
                                                DrawString(__instance, Plugin.config.GetLang().main_8.Value, "Boombox YouTube", nameOfUserWhoTyped);
                                            }
                                        }
                                    }
                                    break;
                                case "soundcloud.com":
                                    if (vs[1].Contains("sets"))
                                    {
                                        DrawString(__instance, Plugin.config.GetLang().main_6.Value, "Boombox SoundCloud", nameOfUserWhoTyped);
                                    }
                                    else
                                    {
                                        boomboxItem.boomboxAudio.Stop();
                                        boomboxItem.boomboxAudio.PlayOneShot(boomboxItem.stopAudios[UnityEngine.Random.Range(0, boomboxItem.stopAudios.Length)]);
                                        timesPlayedWithoutTurningOff = 0;
                                        boomboxItem.isPlayingMusic = false;
                                        boomboxItem.isBeingUsed = false;
                                        LoadingMusicBoombox = true;
                                        FileInfo[] file = new DirectoryInfo(@"BoomboxController\other").GetFiles("*.mp3");
                                        if (file.Length == 1)
                                        {
                                            File.Delete(@$"BoomboxController\other\{file[0].Name}");
                                        }
                                        DrawString(__instance, Plugin.config.GetLang().main_7.Value, "Boombox SoundCloud", nameOfUserWhoTyped);
                                        if (!isplayList)
                                        {
                                            isplayList = true;
                                            await Task.Run(() =>
                                            {
                                                bool succeeded = false;
                                                Process info = new Process();
                                                info.StartInfo.FileName = @"BoomboxController\other\yt-dlp.exe";
                                                info.StartInfo.UseShellExecute = false;
                                                info.StartInfo.Arguments = $"{vs[1]}";
                                                info.StartInfo.WorkingDirectory = @$"BoomboxController\other";
                                                info.StartInfo.CreateNoWindow = true;
                                                info.Start();
                                                Id = info.Id;
                                                while (!succeeded)
                                                {
                                                    foreach (FileInfo f in new DirectoryInfo(@"BoomboxController\other").GetFiles("*.mp3"))
                                                    {
                                                        if (f.Exists)
                                                        {
                                                            NameTrack = f.Name;
                                                        }
                                                    }
                                                    if (Process.GetProcessById(info.Id).HasExited)
                                                    {
                                                        if (File.Exists(@$"BoomboxController\other\{NameTrack}"))
                                                        {
                                                            succeeded = true;
                                                            break;
                                                        }
                                                        else
                                                        {
                                                            DrawString(__instance, Plugin.config.GetLang().main_11.Value, "Boombox SoundCloud", nameOfUserWhoTyped);
                                                            break;
                                                        }
                                                    }
                                                    System.Threading.Thread.Sleep(1000);
                                                }
                                            });
                                            if (!File.Exists(@$"BoomboxController\other\{NameTrack}"))
                                            {
                                                LoadingMusicBoombox = false;
                                                isplayList = false;
                                                break;
                                            }
                                            currectTrack = 0;
                                            bom.Start(bom.GetAudioClip(@"file:///" + Paths.GameRootPath + @$"\BoomboxController\other\{NameTrack}", boomboxItem, AudioType.MPEG));
                                            DrawString(__instance, Plugin.config.GetLang().main_8.Value, "Boombox SoundCloud", nameOfUserWhoTyped);
                                        }
                                    }
                                    break;
                                default:
                                    int type = GetAudioType(vs[1].Remove(0, vs[1].Length - 3));
                                    if (type == -1)
                                    {
                                        DrawString(__instance, Plugin.config.GetLang().main_6.Value, "Boombox", nameOfUserWhoTyped);
                                    }
                                    else
                                    {
                                        LoadingMusicBoombox = true;
                                        DrawString(__instance, Plugin.config.GetLang().main_7.Value, "Boombox", nameOfUserWhoTyped);
                                        bom.Start(bom.GetAudioClip(vs[1], boomboxItem, AudioType.MPEG));
                                        DrawString(__instance, Plugin.config.GetLang().main_8.Value, "Boombox", nameOfUserWhoTyped);
                                    }
                                    break;
                            }
                        }
                        else
                        {
                            DrawString(__instance, Plugin.config.GetLang().main_6.Value, "Boombox", nameOfUserWhoTyped);
                        }
                        break;
                    case "btime":
                        string[] arg = vs[1].Split(':');
                        switch (arg.Length)
                        {
                            case 2:
                                if (boomboxItem.isPlayingMusic)
                                {
                                    int arg1 = Convert.ToInt32(arg[0]);
                                    int arg2 = Convert.ToInt32(arg[1]);
                                    if (arg1 == 0)
                                    {
                                        if (arg2 == 0)
                                        {
                                            boomboxItem.boomboxAudio.time = 0;
                                            DrawString(__instance, Plugin.config.GetLang().main_12.Value.Replace("@1", $"00:00:00"), "Boombox", nameOfUserWhoTyped);
                                            break;
                                        }
                                        if (arg2 > 0)
                                        {
                                            if (arg2 < 60)
                                            {
                                                boomboxItem.boomboxAudio.time = arg2;
                                                DrawString(__instance, Plugin.config.GetLang().main_12.Value.Replace("@1", $"00:00:{arg2.ToString("00")}"), "Boombox", nameOfUserWhoTyped);
                                                break;
                                            }
                                        }
                                        break;
                                    }
                                    if (arg1 > 0)
                                    {
                                        if (arg1 < 60)
                                        {
                                            int correct = arg1 * 60;
                                            if (arg2 > 0)
                                            {
                                                if (arg2 < 60)
                                                {
                                                    int correct_sec = correct + arg2;
                                                    boomboxItem.boomboxAudio.time = correct_sec;
                                                    DrawString(__instance, Plugin.config.GetLang().main_12.Value.Replace("@1", $"00:{arg1.ToString("00")}:{arg2.ToString("00")}"), "Boombox", nameOfUserWhoTyped);
                                                    break;
                                                }
                                            }
                                            boomboxItem.boomboxAudio.time = correct;
                                            DrawString(__instance, Plugin.config.GetLang().main_12.Value.Replace("@1", $"00:{arg1.ToString("00")}:00"), "Boombox", nameOfUserWhoTyped);
                                            break;
                                        }
                                        break;
                                    }
                                }
                                break;
                            case 3:
                                if (boomboxItem.isPlayingMusic)
                                {
                                    int arg1 = Convert.ToInt32(arg[0]);
                                    int arg2 = Convert.ToInt32(arg[1]);
                                    int arg3 = Convert.ToInt32(arg[2]);
                                    if (arg1 == 0)
                                    {
                                        if (arg2 == 0)
                                        {
                                            if (arg3 == 0)
                                            {
                                                boomboxItem.boomboxAudio.time = 0;
                                                DrawString(__instance, Plugin.config.GetLang().main_12.Value.Replace("@1", $"00:00:00"), "Boombox", nameOfUserWhoTyped);
                                                break;
                                            }
                                        }
                                        if (arg2 > 0)
                                        {
                                            if (arg2 < 60)
                                            {
                                                int correct = arg2 * 60;
                                                if (arg3 > 0)
                                                {
                                                    if (arg3 < 60)
                                                    {
                                                        int correct_sec = correct + arg3;
                                                        boomboxItem.boomboxAudio.time = correct_sec;
                                                        DrawString(__instance, Plugin.config.GetLang().main_12.Value.Replace("@1", $"00:{arg2.ToString("00")}:{arg3.ToString("00")}"), "Boombox", nameOfUserWhoTyped);
                                                        break;
                                                    }
                                                }
                                                boomboxItem.boomboxAudio.time = correct;
                                                DrawString(__instance, Plugin.config.GetLang().main_12.Value.Replace("@1", $"00:{arg2.ToString("00")}:00"), "Boombox", nameOfUserWhoTyped);
                                            }
                                        }
                                        break;
                                    }
                                    if (arg1 > 0)
                                    {
                                        if (arg1 < 3)
                                        {
                                            int correct = arg1 * 3600;
                                            if (arg2 > 0)
                                            {
                                                if (arg2 < 60)
                                                {
                                                    int correct_minutes = correct + (arg2 * 60);
                                                    if (arg3 > 0)
                                                    {
                                                        if (arg3 < 60)
                                                        {
                                                            int correct_sec = correct_minutes + arg3;
                                                            boomboxItem.boomboxAudio.time = correct_sec;
                                                            DrawString(__instance, Plugin.config.GetLang().main_12.Value.Replace("@1", $"{arg1.ToString("00")}:{arg2.ToString("00")}:{arg3.ToString("00")}"), "Boombox", nameOfUserWhoTyped);
                                                            break;
                                                        }
                                                    }
                                                    boomboxItem.boomboxAudio.time = correct_minutes;
                                                    DrawString(__instance, Plugin.config.GetLang().main_12.Value.Replace("@1", $"{arg1.ToString("00")}:{arg2.ToString("00")}:00"), "Boombox", nameOfUserWhoTyped);
                                                    break;
                                                }
                                            }
                                            boomboxItem.boomboxAudio.time = correct;
                                            DrawString(__instance, Plugin.config.GetLang().main_12.Value.Replace("@1", $"{arg1.ToString("00")}:00:00"), "Boombox", nameOfUserWhoTyped);
                                        }
                                        break;
                                    }
                                }
                                break;
                        }
                        break;
                    case "bvolume":
                        float volume = boomboxItem.boomboxAudio.volume;
                        float correct_volume = (Convert.ToInt32(vs[1]) / 10) * 0.1f;
                        if (volume == correct_volume) break;
                        if (volume < correct_volume || volume > correct_volume)
                        {
                            boomboxItem.boomboxAudio.volume = correct_volume;
                            DrawString(__instance, Plugin.config.GetLang().main_9.Value.Replace("@1", $"{nameOfUserWhoTyped}").Replace("@2", $"{vs[1]}%"), "Boombox", nameOfUserWhoTyped);
                        }
                        break;
                    case "btrack":
                        if (Convert.ToInt32(vs[1]) > 0)
                        {
                            if(Convert.ToInt32(vs[1]) <= totalTack)
                            {
                                int track = Convert.ToInt32(vs[1]) - 1;
                                currectTrack = track;
                                boomboxItem.boomboxAudio.Stop();
                                timesPlayedWithoutTurningOff = 0;
                                boomboxItem.boomboxAudio.clip = musicList[currectTrack];
                                boomboxItem.boomboxAudio.pitch = 1f;
                                boomboxItem.boomboxAudio.Play();
                                DrawString(__instance, Plugin.config.GetLang().main_14.Value.Replace("@1", $"{vs[1]}"), "Boombox", nameOfUserWhoTyped);
                            }
                        }
                        break;
                }
            }
        }

        public static void LoadPlaylist(HUDManager __instance, string nameOfUserWhoTyped)
        {
            bool isPlaying = false;
            int total = 0;
            int allcount = new DirectoryInfo(@"BoomboxController\other\playlist").GetFiles().Length;
            FileInfo[] track = new DirectoryInfo(@"BoomboxController\other\playlist").GetFiles();
            while (!isPlaying)
            {
                if(total == allcount) break;
                if (track[total].Exists)
                {
                    bom.Start(bom.GetPlayList(@"file:///" + Paths.GameRootPath + @$"\BoomboxController\other\playlist\{track[total].Name}", boomboxItem, AudioType.MPEG));
                    total++;
                }
                System.Threading.Thread.Sleep(2000);
            }
            musicList = bom.audioclipsplay.ToArray();
            bom.audioclipsplay.Clear();
            LoadingMusicBoombox = false;
            DrawString(__instance, Plugin.config.GetLang().main_8.Value, "Boombox YouTube", nameOfUserWhoTyped);
            isplayList = false;
            thread.Abort();
        }

        public static int GetAudioType(string ext)
        {
            switch (ext)
            {
                case "mp3": return 13;
            }
            return -1;
        }

        public static void DrawString(HUDManager __instance, string chatMessage, string nameOfUserWhoTyped, string? name)
        {
            if (!(LastMessage == chatMessage))
            {
                LastMessage = chatMessage;
                LastnameOfUserWhoTyped = name;
                __instance.PingHUDElement(__instance.Chat, 4f);
                if (__instance.ChatMessageHistory.Count >= 4)
                {
                    __instance.chatText.text.Remove(0, __instance.ChatMessageHistory[0].Length);
                    __instance.ChatMessageHistory.Remove(__instance.ChatMessageHistory[0]);
                }
                StringBuilder stringBuilder = new StringBuilder(chatMessage);
                stringBuilder.Replace("[playerNum0]", StartOfRound.Instance.allPlayerScripts[0].playerUsername);
                stringBuilder.Replace("[playerNum1]", StartOfRound.Instance.allPlayerScripts[1].playerUsername);
                stringBuilder.Replace("[playerNum2]", StartOfRound.Instance.allPlayerScripts[2].playerUsername);
                stringBuilder.Replace("[playerNum3]", StartOfRound.Instance.allPlayerScripts[3].playerUsername);
                chatMessage = stringBuilder.ToString();
                string item = ((!string.IsNullOrEmpty(nameOfUserWhoTyped)) ? ("<color=#FF0000>" + nameOfUserWhoTyped + "</color>: <color=#FFFF00>'" + chatMessage + "'</color>") : ("<color=#7069ff>" + chatMessage + "</color>"));
                __instance.ChatMessageHistory.Add(item);
                __instance.chatText.text = "";
                for (int i = 0; i < __instance.ChatMessageHistory.Count; i++)
                {
                    TextMeshProUGUI textMeshProUGUI = __instance.chatText;
                    textMeshProUGUI.text = textMeshProUGUI.text + "\n" + __instance.ChatMessageHistory[i];
                }
            }
        }
        [HarmonyPatch(typeof(HUDManager), "AddPlayerChatMessageServerRpc")]
        [HarmonyPostfix]
        [ServerRpc(RequireOwnership = false)]
        private static void AddPlayerChatMessageServerRpc(HUDManager __instance, string chatMessage, int playerId)
        {
            if(chatMessage.Length > 50)
            {
                MethodInfo method = ((object)__instance).GetType().GetMethod("AddPlayerChatMessageClientRpc", BindingFlags.Instance | BindingFlags.NonPublic);
                method.Invoke(__instance, new object[2] { chatMessage, playerId });
            }
        }

        [HarmonyPatch(typeof(HUDManager), "SubmitChat_performed")]
        [HarmonyPrefix]
        private static bool SubmitChat_performed(HUDManager __instance, ref InputAction.CallbackContext context)
        {
            if (!LoadingMusicBoombox)
            {
                __instance.localPlayer = GameNetworkManager.Instance.localPlayerController;
                if (!context.performed || __instance.localPlayer == null || !__instance.localPlayer.isTypingChat || ((!__instance.localPlayer.IsOwner || (__instance.IsServer && !__instance.localPlayer.isHostPlayerObject)) && !__instance.localPlayer.isTestingPlayer) || __instance.localPlayer.isPlayerDead) ;
                else
                {
                    if (!string.IsNullOrEmpty(__instance.chatTextField.text) && __instance.chatTextField.text.Length < 200)
                    {
                        __instance.AddTextToChatOnServer(__instance.chatTextField.text, (int)__instance.localPlayer.playerClientId);
                    }
                    for (int i = 0; i < StartOfRound.Instance.allPlayerScripts.Length; i++)
                    {
                        if (StartOfRound.Instance.allPlayerScripts[i].isPlayerControlled && Vector3.Distance(GameNetworkManager.Instance.localPlayerController.transform.position, StartOfRound.Instance.allPlayerScripts[i].transform.position) > 24.4f && (!GameNetworkManager.Instance.localPlayerController.holdingWalkieTalkie || !StartOfRound.Instance.allPlayerScripts[i].holdingWalkieTalkie))
                        {
                            __instance.playerCouldRecieveTextChatAnimator.SetTrigger("ping");
                            break;
                        }
                    }
                    __instance.localPlayer.isTypingChat = false;
                    __instance.chatTextField.text = "";
                    EventSystem.current.SetSelectedGameObject(null);
                    __instance.PingHUDElement(__instance.Chat);
                    __instance.typingIndicator.enabled = false;
                    return false;
                }
            }
            return false;
        }

        public static int FirstEmptyItemSlot(PlayerControllerB __instance)
        {
            int result = -1;
            if (__instance.ItemSlots[__instance.currentItemSlot] == null)
            {
                result = __instance.currentItemSlot;
            }
            else
            {
                for (int i = 0; i < __instance.ItemSlots.Length; i++)
                {
                    if (__instance.ItemSlots[i] == null)
                    {
                        result = i;
                        break;
                    }
                }
            }
            return result;
        }

        public static GameObject upbutton;
        public static GameObject downbutton;

        [HarmonyPatch(typeof(KepRemapPanel), "OnEnable")]
        [HarmonyPrefix]
        public static void OnEnable(KepRemapPanel __instance)
        {
            __instance.keyRemapContainer.parent.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 900f);
            Cache cache = LoadCache();
            GameObject gameObject3 = UnityEngine.Object.Instantiate(__instance.sectionTextPrefab, __instance.keyRemapContainer);
            gameObject3.GetComponent<RectTransform>().anchoredPosition = new Vector2(-40f, 0f - __instance.verticalOffset * 18);
            gameObject3.GetComponentInChildren<TextMeshProUGUI>().text = "BOOMBOX CONTROLLER";
            __instance.keySlots.Add(gameObject3);
            GameObject gameObject4 = UnityEngine.Object.Instantiate(__instance.keyRemapSlotPrefab, __instance.keyRemapContainer);
            __instance.keySlots.Add(gameObject4);
            Destroy(gameObject4.GetComponentInChildren<SettingsOption>());
            upbutton = gameObject4;
            gameObject4.GetComponentInChildren<UnityEngine.UI.Button>().onClick.RemoveAllListeners();
            gameObject4.GetComponentInChildren<UnityEngine.UI.Button>().onClick.AddListener(BoomboxController_onClickUp);
            gameObject4.GetComponentInChildren<TextMeshProUGUI>().text = "Volume Up";
            gameObject4.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 0f - __instance.verticalOffset * 18f);
            if (cache == null)
            {
                gameObject4.transform.GetChild(2).gameObject.GetComponentInChildren<TextMeshProUGUI>().text = Keyboard.current.pageUpKey.displayName;
            }
            else
            {
                if (cache.UpButton == null && cache.DownButton == null)
                {
                    gameObject4.transform.GetChild(2).gameObject.GetComponentInChildren<TextMeshProUGUI>().text = Keyboard.current.pageUpKey.displayName;
                }
                else
                {
                    if (cache.UpButton == null)
                    {
                        gameObject4.transform.GetChild(2).gameObject.GetComponentInChildren<TextMeshProUGUI>().text = Keyboard.current.pageUpKey.displayName;
                    }
                    else
                    {
                        KeyControl control = Keyboard.current.FindKeyOnCurrentKeyboardLayout(cache.UpButton);
                        gameObject4.transform.GetChild(2).gameObject.GetComponentInChildren<TextMeshProUGUI>().text = control.displayName;
                    }
                }
            }
            GameObject gameObject5 = UnityEngine.Object.Instantiate(__instance.keyRemapSlotPrefab, __instance.keyRemapContainer);
            __instance.keySlots.Add(gameObject5);
            Destroy(gameObject5.GetComponentInChildren<SettingsOption>());
            downbutton = gameObject5;
            gameObject5.GetComponentInChildren<UnityEngine.UI.Button>().onClick.RemoveAllListeners();
            gameObject5.GetComponentInChildren<UnityEngine.UI.Button>().onClick.AddListener(BoomboxController_onClickDown);
            gameObject5.GetComponentInChildren<TextMeshProUGUI>().text = "Volume Down";
            gameObject5.GetComponent<RectTransform>().anchoredPosition = new Vector2(250f, 0f - __instance.verticalOffset * 18f);
            if (cache == null)
            {
                gameObject5.transform.GetChild(2).gameObject.GetComponentInChildren<TextMeshProUGUI>().text = Keyboard.current.pageDownKey.displayName;
            }
            else
            {
                if (cache.UpButton == null && cache.DownButton == null)
                {
                    gameObject5.transform.GetChild(2).gameObject.GetComponentInChildren<TextMeshProUGUI>().text = Keyboard.current.pageDownKey.displayName;
                }
                else
                {
                    if (cache.DownButton == null)
                    {
                        gameObject5.transform.GetChild(2).gameObject.GetComponentInChildren<TextMeshProUGUI>().text = Keyboard.current.pageDownKey.displayName;
                    }
                    else
                    {
                        KeyControl control = Keyboard.current.FindKeyOnCurrentKeyboardLayout(cache.DownButton);
                        gameObject5.transform.GetChild(2).gameObject.GetComponentInChildren<TextMeshProUGUI>().text = control.displayName;
                    }
                }
            }
        }

        private static async void BoomboxController_onClickUp()
        {
            Cache cache = LoadCache();
            upbutton.transform.GetChild(2).gameObject.GetComponentInChildren<TextMeshProUGUI>().enabled = false;
            upbutton.transform.GetChild(3).gameObject.SetActive(value: true);
            await Task.Run(() =>
            {
                bool s = false;
                while (!s)
                {
                    if (Keyboard.current.anyKey.wasPressedThisFrame)
                    {
                        foreach (KeyControl key in Keyboard.current.allKeys)
                        {
                            if (key.wasPressedThisFrame)
                            {
                                upbutton.transform.GetChild(2).gameObject.GetComponentInChildren<TextMeshProUGUI>().text = key.displayName;
                                upbutton.transform.GetChild(2).gameObject.GetComponentInChildren<TextMeshProUGUI>().enabled = true;
                                upbutton.transform.GetChild(3).gameObject.SetActive(value: false);
                                up = key;
                                s = true;
                                break;
                            }
                        }
                    }
                    Thread.Sleep(5);
                }
            });
            if (down == null)
            {
                if(cache != null)
                {
                    SaveCache(cache.Volume, up.displayName, cache.DownButton);
                }
                else
                {
                    SaveCache(0.5f, up.displayName, null);
                }
            }
            else
            {
                if(cache != null)
                {
                    SaveCache(cache.Volume, up.displayName, down.displayName);
                }
                else
                {
                    SaveCache(0.5f, up.displayName, down.displayName);
                }
            }
        }

        private static async void BoomboxController_onClickDown()
        {
            Cache cache = LoadCache();
            downbutton.transform.GetChild(2).gameObject.GetComponentInChildren<TextMeshProUGUI>().enabled = false;
            downbutton.transform.GetChild(3).gameObject.SetActive(value: true);
            await Task.Run(() =>
            {
                bool s = false;
                while (!s)
                {
                    if (Keyboard.current.anyKey.wasPressedThisFrame)
                    {
                        foreach (KeyControl key in Keyboard.current.allKeys)
                        {
                            if (key.wasPressedThisFrame)
                            {
                                downbutton.transform.GetChild(2).gameObject.GetComponentInChildren<TextMeshProUGUI>().text = key.displayName;
                                downbutton.transform.GetChild(2).gameObject.GetComponentInChildren<TextMeshProUGUI>().enabled = true;
                                downbutton.transform.GetChild(3).gameObject.SetActive(value: false);
                                down = key;
                                s = true;
                                break;
                            }
                        }
                    }
                    Thread.Sleep(5);
                }
            });
            if(up == null)
            {
                if (cache != null)
                {
                    SaveCache(cache.Volume, cache.UpButton, down.displayName);
                }
                else
                {
                    SaveCache(0.5f, null, down.displayName);
                }
            }
            else
            {
                if (cache != null)
                {
                    SaveCache(cache.Volume, up.displayName, down.displayName);
                }
                else
                {
                    SaveCache(0.5f, up.displayName, down.displayName);
                }
            }
        }

        public static void SaveCache(float vol, string up, string down)
        {
            Cache cache = new Cache();
            cache.Volume = vol;
            cache.UpButton = up;
            cache.DownButton = down;
            string json = JsonConvert.SerializeObject(cache);
            using (StreamWriter sw = new StreamWriter(@"BoomboxController\cache"))
            {
                sw.WriteLine(json);
            }
        }

        public static Cache LoadCache()
        {
            string json = String.Empty;
            if (File.Exists(@"BoomboxController\cache"))
            {
                using (StreamReader sr = new StreamReader(@"BoomboxController\cache"))
                {
                    json = sr.ReadToEnd();
                }
                return JsonConvert.DeserializeObject<Cache>(json);
            }
            return null;
        }
    }

    public class AudioBoomBox : MonoBehaviour
    {
        private static AudioBoomBox _instance;

        public List<AudioClip> audioclips = new List<AudioClip>();

        public List<AudioClip> audioclipsplay = new List<AudioClip>();

        public Coroutine Start(IEnumerator routine)
        {
            if(_instance == null)
            {
                _instance = new GameObject("AudioBoomBox").AddComponent<AudioBoomBox>();
                UnityEngine.Object.DontDestroyOnLoad((UnityEngine.Object)(object)_instance);
            }
            return ((MonoBehaviour)_instance).StartCoroutine(routine);
        }

        public IEnumerator GetAudioClip(string url, BoomboxItem boombox, AudioType type)
        {
            audioclips.Clear();
            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, type))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.ConnectionError)
                {
                    BoomboxController.LoadingMusicBoombox = false;
                    Plugin.instance.Log(www.error);
                    BoomboxController.isplayList = false;
                }
                else
                {
                    AudioClip myClip = DownloadHandlerAudioClip.GetContent(www);
                    audioclips.Add(myClip);
                    BoomboxController.musicList = audioclips.ToArray();
                    BoomboxController.LoadingMusicBoombox = false;
                    BoomboxController.isplayList = false;
                }
            }
        }

        public IEnumerator GetPlayList(string url, BoomboxItem boombox, AudioType type)
        {
            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, type))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.ConnectionError)
                {
                    Plugin.instance.Log(www.error);
                }
                else
                {
                    AudioClip myClip = DownloadHandlerAudioClip.GetContent(www);
                    audioclipsplay.Add(myClip);
                }
            }
        }
    }

    public class VisualBoombox : MonoBehaviour
    {
        private static VisualBoombox _instance;

        public Coroutine Start(IEnumerator routine)
        {
            if (_instance == null)
            {
                _instance = new GameObject("VisualBoombox").AddComponent<VisualBoombox>();
                UnityEngine.Object.DontDestroyOnLoad((UnityEngine.Object)(object)_instance);
            }
            return ((MonoBehaviour)_instance).StartCoroutine(routine);
        }

        public IEnumerator GetTexture(string url, BoomboxItem boombox)
        {
            using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url))
            {
                yield return uwr.SendWebRequest();

                if (uwr.result != UnityWebRequest.Result.Success)
                {
                    Plugin.instance.Log(uwr.error);
                }
                else
                {
                    var texture = DownloadHandlerTexture.GetContent(uwr);
                    GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cube.name = "QuadBoombox";
                    cube.transform.localScale = new Vector3(0.8f, 0.38f, 0.001f);
                    Vector3 rot = cube.transform.localRotation.eulerAngles;
                    rot.Set(0f, -90f, 0f);
                    cube.transform.localRotation = Quaternion.Euler(rot);
                    cube.transform.position = new Vector3(boombox.transform.position.x - 0.179f, boombox.transform.position.y, boombox.transform.position.z);
                    cube.transform.parent = boombox.transform;
                    cube.GetComponent<BoxCollider>().enabled = false;
                    cube.GetComponent<MeshRenderer>().material = new Material(Shader.Find("HDRP/Lit"));
                    cube.GetComponent<MeshRenderer>().material.mainTexture = texture;
                }
            }
        }

        //private const int MATERIAL_OPAQUE = 0;
        //private const int MATERIAL_TRANSPARENT = 1;

        //private void SetMaterialTransparent(Material material, bool enabled)
        //{
        //    material.SetFloat("_SurfaceType", enabled ? MATERIAL_TRANSPARENT : MATERIAL_OPAQUE);
        //    material.SetFloat("_BlendMode", enabled ? MATERIAL_TRANSPARENT : MATERIAL_OPAQUE);
        //    material.SetShaderPassEnabled("SHADOWCASTER", !enabled);
        //    material.renderQueue = enabled ? 3000 : 2000;
        //    material.SetFloat("_DstBlend", enabled ? 10 : 0);
        //    material.SetFloat("_SrcBlend", enabled ? 5 : 1);
        //    material.SetFloat("_ZWrite", enabled ? 0 : 1);
        //}
    }
}
