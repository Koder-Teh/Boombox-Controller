using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using DunGen;
using GameNetcodeStuff;
using HarmonyLib;
using Mono.Cecil;
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
        public static string[] sumbols = { "+", "#" };
        public static KeyControl up = null;
        public static KeyControl down = null;
        private static bool blockcompatibility = false;
        public static bool waitAutoNext = false;

        #region Стартеры

        [HarmonyPatch(typeof(GameNetworkManager), "OnEnable")]
        [HarmonyPostfix]
        public static void OnEnable_GameNetworkManager(GameNetworkManager __instance)
        {
            using (StreamWriter sw = new StreamWriter(@"BoomboxController\logReport.txt"))
            {
                sw.WriteLine($"Game Version: {__instance.gameVersionNum}");
                sw.WriteLine($"Plugins: {Chainloader.PluginInfos.Count}");
                foreach (var item in Chainloader.PluginInfos)
                {
                    if (item.Key == "BoomboxSyncFix")
                    {
                        Plugin.instance.Log("Чтобы мод работал, удалите мод BoomboxSyncFix.");
                        Plugin.instance.Log("For the mod to work, uninstall the BoomboxSyncFix mod.");
                        blockcompatibility = true;
                    }
                    sw.WriteLine(item.Key + " " + item.Value.Location);
                }
            }
        }

        [HarmonyPatch(typeof(MenuManager), "OnEnable")]
        [HarmonyPrefix]
        public static void OnEnable_MenuManager()
        {
            LastMessage = string.Empty;
            if (quit == null)
            {
                quit = new GameObject("QuitManager").AddComponent<QuitManager>();
                UnityEngine.Object.DontDestroyOnLoad((UnityEngine.Object)(object)quit);
            }
        }

        [HarmonyPatch(typeof(StartOfRound), "OnEnable")]
        [HarmonyPrefix]
        public static void OnEnable_StartOfRound()
        {
            if (quits == null)
            {
                quits = new GameObject("QuitManager").AddComponent<QuitManager>();
                UnityEngine.Object.DontDestroyOnLoad((UnityEngine.Object)(object)quits);
            }
        }

        #endregion

        #region Наведение, поле зрение

        [HarmonyPatch(typeof(PlayerControllerB), "SetHoverTipAndCurrentInteractTrigger")]
        [HarmonyPrefix]
        public static bool SetHoverTipAndCurrentInteractTrigger_PlayerControllerB(PlayerControllerB __instance, ref RaycastHit ___hit, ref Ray ___interactRay, ref int ___playerMask, ref int ___interactableObjectsMask)
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
                                                if (!waitAutoNext)
                                                {
                                                    curretTime = 0;
                                                    totalTime = 0;
                                                }
                                            }
                                            int currect_ost = (int)curretTime % 3600;
                                            string currect_hours = Mathf.Floor((int)curretTime / 3600).ToString("00");
                                            string currect_minutes = Mathf.Floor((int)currect_ost / 60).ToString("00");
                                            string currect_seconds = Mathf.Floor((int)currect_ost % 60).ToString("00");
                                            int total_ost = (int)totalTime % 3600;
                                            string total_hours = Mathf.Floor((int)totalTime / 3600).ToString("00");
                                            string total_minutes = Mathf.Floor((int)total_ost / 60).ToString("00");
                                            string total_seconds = Mathf.Floor((int)total_ost % 60).ToString("00");
                                            if (Plugin.config.languages.Value == "en")
                                            {
                                                if (musicList == null || musicList.Length > 0)
                                                {
                                                    string playname = boomboxItem.isPlayingMusic ? "[Home]" : "Nothing";
                                                    __instance.cursorTip.text = Plugin.config.GetLang().main_2.Value.Replace("@1", $"{Math.Round(volume * 100)}%").Replace("@2", $"{currect_hours}:{currect_minutes}:{currect_seconds}").Replace("@3", $"{total_hours}:{total_minutes}:{total_seconds}").Replace("@4", $"{playname}").Replace("@5", $"{totalTack}") + $"\nIncrease volume [{(up == null ? "PU" : up.displayName)}]\nDecrease volume [{(down == null ? "PD" : down.displayName)}]";
                                                }
                                            }
                                            if (Plugin.config.languages.Value == "ru")
                                            {
                                                if (musicList == null || musicList.Length > 0)
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

        #endregion

        #region Бумбокс

        [HarmonyPatch(typeof(BoomboxItem), "Start")]
        [HarmonyPrefix]
        private static void Start_BoomboxItem(BoomboxItem __instance)
        {
            bom = new AudioBoomBox();
            vbom = new VisualBoombox();
            if (Plugin.config.visual.Value)
            {
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
                    if (image.Width > image.Height)
                    {
                        if (image.Width > 500)
                        {
                            vbom.Start(vbom.GetTexture(@"file:///" + Paths.GameRootPath + @$"\BoomboxController\back.jpg", __instance));
                        }
                    }
                }
            }
            __instance.boomboxAudio.volume = 0.5f;
            __instance.itemProperties.weight = 0;
            __instance.musicAudios = null;
            __instance.itemProperties.requiresBattery = Plugin.config.requstbattery.Value;
            boomboxItem = __instance;
            Cache cache = LoadCache();
            if (cache != null)
            {
                __instance.boomboxAudio.volume = cache.Volume;
                if (cache.UpButton != null)
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
        private static bool StartMusic_BoomboxItem(BoomboxItem __instance, bool startMusic, bool pitchDown, ref int ___timesPlayedWithoutTurningOff)
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
        private static void Update_BoomboxItem(BoomboxItem __instance, ref int ___timesPlayedWithoutTurningOff)
        {
            if (timesPlayedWithoutTurningOff <= 0)
            {
                ___timesPlayedWithoutTurningOff = 0;
            }
            timesPlayedWithoutTurningOff = ___timesPlayedWithoutTurningOff;
            if (musicList != null)
            {
                totalTack = musicList.Length;
            }
            if (boomboxItem.isPlayingMusic)
            {
                curretTime = boomboxItem.boomboxAudio.time;
                totalTime = boomboxItem.boomboxAudio.clip.length;
                if ((currectTrack + 1) != totalTack)
                {
                    if(Math.Floor(curretTime) == Math.Floor(totalTime))
                    {
                        boomboxItem.boomboxAudio.Stop();
                        boomboxItem.isPlayingMusic = false;
                        waitAutoNext = true;
                    }
                }
                else
                {
                    if(totalTack == 1)
                    {
                        if (Math.Floor(curretTime) == Math.Floor(totalTime))
                        {
                            boomboxItem.boomboxAudio.time = 0;
                            return;
                        }
                    }
                    else
                    {
                        boomboxItem.boomboxAudio.Stop();
                        currectTrack = 0;
                        boomboxItem.boomboxAudio.time = 0;
                        boomboxItem.boomboxAudio.clip = musicList[currectTrack];
                        boomboxItem.boomboxAudio.Play();
                    }
                }
            }
            else
            {
                if (musicList != null)
                {
                    if (waitAutoNext)
                    {
                        currectTrack = currectTrack + 1;
                        Plugin.instance.Log($"Position track: {currectTrack}");
                        boomboxItem.boomboxAudio.clip = musicList[currectTrack];
                        boomboxItem.boomboxAudio.time = 0;
                        boomboxItem.isPlayingMusic = true;
                        waitAutoNext = false;
                        boomboxItem.boomboxAudio.Play();
                    }
                }
            }
        }

        [HarmonyPatch(typeof(BoomboxItem), "PocketItem")]
        [HarmonyPrefix]
        private static bool PocketItem_BoomboxItem(BoomboxItem __instance)
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

        #endregion

        #region Чат

        [HarmonyPatch(typeof(HUDManager), "Update")]
        [HarmonyPrefix]
        private static void Update_HUDManager(HUDManager __instance)
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
        private static void Start_HUDManager(HUDManager __instance)
        {
            __instance.chatTextField.characterLimit = 200;
        }

        [HarmonyPatch(typeof(HUDManager), "AddChatMessage")]
        [HarmonyPostfix]
        private static void AddChatMessage_HUDManager(HUDManager __instance, string chatMessage, string nameOfUserWhoTyped)
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
                        if (vs.Length == 1) break;
                        Regex regex = new Regex("^https?:\\/\\/(?:www\\.)?[-a-zA-Z0-9@:%._\\+~#=]{1,256}\\.[a-zA-Z0-9()]{1,6}\\b(?:[-a-zA-Z0-9()@:%_\\+.~#?&\\/=]*)$");
                        if (regex.IsMatch(vs[1]))
                        {
                            var url = vs[1].Remove(0, 8);
                            switch (url.Substring(0, url.IndexOf('/')))
                            {
                                case "music.youtube.com":
                                    if (vs[1].Contains("list"))
                                    {
                                        DrawString(__instance, Plugin.config.GetLang().main_6.Value, "Boombox Music YouTube", nameOfUserWhoTyped);
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
                                        FileInfo[] files = new DirectoryInfo(@"BoomboxController\other").GetFiles("*.mp3");
                                        if (files.Length == 1)
                                        {
                                            File.Delete(@$"BoomboxController\other\{files[0].Name}");
                                        }
                                        DrawString(__instance, Plugin.config.GetLang().main_7.Value, "Boombox Music YouTube", nameOfUserWhoTyped);
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
                                                info.StartInfo.Arguments = $"-f bestaudio --extract-audio --ignore-config --audio-format mp3 --audio-quality 0 {vs[1]}";
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
                                                                DrawString(__instance, Plugin.config.GetLang().main_11.Value, "Boombox Music YouTube", nameOfUserWhoTyped);
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
                                            foreach (string sumb in sumbols)
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
                                                foreach (string sumb in sumbol)
                                                {
                                                    NameFile = NameTrack.Replace(sumb, "");
                                                    ext.MoveTo(@$"BoomboxController\other\{NameTrack.Replace(sumb, "")}");
                                                }
                                                currectTrack = 0;
                                                boomboxItem.boomboxAudio.time = 0;
                                                bom.Start(bom.GetAudioClip(@"file:///" + Paths.GameRootPath + @$"\BoomboxController\other\{NameFile}", boomboxItem, AudioType.MPEG));
                                                DrawString(__instance, Plugin.config.GetLang().main_8.Value, "Boombox Music YouTube", nameOfUserWhoTyped);
                                            }
                                            else
                                            {
                                                currectTrack = 0;
                                                boomboxItem.boomboxAudio.time = 0;
                                                bom.Start(bom.GetAudioClip(@"file:///" + Paths.GameRootPath + @$"\BoomboxController\other\{NameTrack}", boomboxItem, AudioType.MPEG));
                                                DrawString(__instance, Plugin.config.GetLang().main_8.Value, "Boombox Music YouTube", nameOfUserWhoTyped);
                                            }
                                        }
                                    }
                                    break;
                                case "youtu.be":
                                    if (vs[1].Contains("list"))
                                    {
                                        DrawString(__instance, Plugin.config.GetLang().main_6.Value, "Boombox Music YouTube", nameOfUserWhoTyped);
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
                                        FileInfo[] filesy = new DirectoryInfo(@"BoomboxController\other").GetFiles("*.mp3");
                                        if (filesy.Length == 1)
                                        {
                                            File.Delete(@$"BoomboxController\other\{filesy[0].Name}");
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
                                                info.StartInfo.Arguments = $"-f bestaudio --extract-audio --ignore-config --audio-format mp3 --audio-quality 0 {vs[1]}";
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
                                            foreach (string sumb in sumbols)
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
                                                foreach (string sumb in sumbol)
                                                {
                                                    NameFile = NameTrack.Replace(sumb, "");
                                                    ext.MoveTo(@$"BoomboxController\other\{NameTrack.Replace(sumb, "")}");
                                                }
                                                currectTrack = 0;
                                                boomboxItem.boomboxAudio.time = 0;
                                                bom.Start(bom.GetAudioClip(@"file:///" + Paths.GameRootPath + @$"\BoomboxController\other\{NameFile}", boomboxItem, AudioType.MPEG));
                                                DrawString(__instance, Plugin.config.GetLang().main_8.Value, "Boombox YouTube", nameOfUserWhoTyped);
                                            }
                                            else
                                            {
                                                currectTrack = 0;
                                                boomboxItem.boomboxAudio.time = 0;
                                                bom.Start(bom.GetAudioClip(@"file:///" + Paths.GameRootPath + @$"\BoomboxController\other\{NameTrack}", boomboxItem, AudioType.MPEG));
                                                DrawString(__instance, Plugin.config.GetLang().main_8.Value, "Boombox YouTube", nameOfUserWhoTyped);
                                            }
                                        }
                                    }
                                    break;
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
                                                info.StartInfo.Arguments = $"-f bestaudio --extract-audio --ignore-config --audio-format mp3 --audio-quality 0 {vs[1]}";
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
                                                        if (new DirectoryInfo(@"BoomboxController\other\playlist").GetFiles("*.webm").Length == 0)
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
                                            boomboxItem.boomboxAudio.time = 0;
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
                                                info.StartInfo.Arguments = $"-f bestaudio --extract-audio --ignore-config --audio-format mp3 --audio-quality 0 {vs[1]}";
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
                                            foreach (string sumb in sumbols)
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
                                                foreach (string sumb in sumbol)
                                                {
                                                    NameFile = NameTrack.Replace(sumb, "");
                                                    ext.MoveTo(@$"BoomboxController\other\{NameTrack.Replace(sumb, "")}");
                                                }
                                                currectTrack = 0;
                                                boomboxItem.boomboxAudio.time = 0;
                                                bom.Start(bom.GetAudioClip(@"file:///" + Paths.GameRootPath + @$"\BoomboxController\other\{NameFile}", boomboxItem, AudioType.MPEG));
                                                DrawString(__instance, Plugin.config.GetLang().main_8.Value, "Boombox YouTube", nameOfUserWhoTyped);
                                            }
                                            else
                                            {
                                                currectTrack = 0;
                                                boomboxItem.boomboxAudio.time = 0;
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
                                                info.StartInfo.Arguments = $"--ignore-config {vs[1]}";
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
                                            boomboxItem.boomboxAudio.time = 0;
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
                        if (vs.Length == 1) break;
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
                        if (vs.Length == 1) break;
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
                        if (vs.Length == 1) break;
                        if (Convert.ToInt32(vs[1]) > 0)
                        {
                            if (Convert.ToInt32(vs[1]) <= totalTack)
                            {
                                int track = Convert.ToInt32(vs[1]) - 1;
                                currectTrack = track;
                                boomboxItem.boomboxAudio.Stop();
                                timesPlayedWithoutTurningOff = 0;
                                boomboxItem.boomboxAudio.clip = musicList[currectTrack];
                                boomboxItem.boomboxAudio.pitch = 1f;
                                boomboxItem.boomboxAudio.time = 0;
                                boomboxItem.boomboxAudio.Play();
                                DrawString(__instance, Plugin.config.GetLang().main_14.Value.Replace("@1", $"{vs[1]}"), "Boombox", nameOfUserWhoTyped);
                            }
                        }
                        break;
                }
            }
        }

        [HarmonyPatch(typeof(HUDManager), "AddPlayerChatMessageServerRpc")]
        [HarmonyPostfix]
        [ServerRpc(RequireOwnership = false)]
        private static void AddPlayerChatMessageServerRpc_HUDManager(HUDManager __instance, string chatMessage, int playerId)
        {
            if (chatMessage.Length > 50)
            {
                __instance.GetType().GetMethod("AddPlayerChatMessageClientRpc", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(__instance, new object[2] { chatMessage, playerId });
            }
        }

        [HarmonyPatch(typeof(HUDManager), "AddPlayerChatMessageClientRpc")]
        [HarmonyPostfix]
        [ClientRpc]
        private static void AddPlayerChatMessageClientRpc_HUDManager(HUDManager __instance, string chatMessage, int playerId)
        {
            if (Plugin.config.radiuscheck.Value)
            {
                if (IsCommand(chatMessage, new string[] { "bhelp", "bplay", "btime", "bvolume", "btrack" }))
                {
                    if (!(Vector3.Distance(GameNetworkManager.Instance.localPlayerController.transform.position, __instance.playersManager.allPlayerScripts[playerId].transform.position) < 25f))
                    {
                        __instance.GetType().GetMethod("AddChatMessage", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(__instance, new object[2] { chatMessage, __instance.playersManager.allPlayerScripts[playerId].playerUsername });
                    }
                }
            }
        }

        [HarmonyPatch(typeof(HUDManager), "SubmitChat_performed")]
        [HarmonyPrefix]
        private static void SubmitChat_performed_HUDManager(HUDManager __instance, ref InputAction.CallbackContext context)
        {
            if (LoadingMusicBoombox)
            {
                if (IsCommand(__instance.chatTextField.text, new string[] { "bhelp", "bplay", "btime", "bvolume", "btrack" })) __instance.chatTextField.text = String.Empty;
            }
            else
            {
                if (!blockcompatibility)
                {
                    if (IsCommand(__instance.chatTextField.text, new string[] { "bhelp", "bplay", "btime", "bvolume", "btrack" }))
                    {
                        SubmitChat(__instance);
                        return;
                    }
                }
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
            }
        }

        public static void SubmitChat(HUDManager __instance)
        {
            if (!string.IsNullOrEmpty(__instance.chatTextField.text) && __instance.chatTextField.text.Length < 200)
            {
                __instance.AddTextToChatOnServer(__instance.chatTextField.text, (int)__instance.localPlayer.playerClientId);
            }
            for (int i = 0; i < StartOfRound.Instance.allPlayerScripts.Length; i++)
            {
                if (Plugin.config.radiuscheck.Value)
                {
                    if (StartOfRound.Instance.allPlayerScripts[i].isPlayerControlled && (!GameNetworkManager.Instance.localPlayerController.holdingWalkieTalkie || !StartOfRound.Instance.allPlayerScripts[i].holdingWalkieTalkie))
                    {
                        __instance.playerCouldRecieveTextChatAnimator.SetTrigger("ping");
                        break;
                    }
                }
                else
                {
                    if (StartOfRound.Instance.allPlayerScripts[i].isPlayerControlled && Vector3.Distance(GameNetworkManager.Instance.localPlayerController.transform.position, StartOfRound.Instance.allPlayerScripts[i].transform.position) > 24.4f && (!GameNetworkManager.Instance.localPlayerController.holdingWalkieTalkie || !StartOfRound.Instance.allPlayerScripts[i].holdingWalkieTalkie))
                    {
                        __instance.playerCouldRecieveTextChatAnimator.SetTrigger("ping");
                        break;
                    }
                }
            }
            __instance.localPlayer.isTypingChat = false;
            __instance.chatTextField.text = "";
            EventSystem.current.SetSelectedGameObject(null);
            __instance.PingHUDElement(__instance.Chat);
            __instance.typingIndicator.enabled = false;
        }

        #endregion

        #region Остальное

        public static bool IsCommand(string text, string[] args)
        {
            foreach (string command in args)
            {
                if (text.Contains(command)) return true;
            }
            return false;
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

        public static int GetAudioType(string ext)
        {
            switch (ext)
            {
                case "mp3": return 13;
            }
            return -1;
        }

        public static void LoadPlaylist(HUDManager __instance, string nameOfUserWhoTyped)
        {
            bool isPlaying = false;
            int total = 0;
            int allcount = new DirectoryInfo(@"BoomboxController\other\playlist").GetFiles().Length;
            FileInfo[] track = new DirectoryInfo(@"BoomboxController\other\playlist").GetFiles();
            while (!isPlaying)
            {
                if (total == allcount) break;
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

        #endregion

        #region Настройки, управление

        public static GameObject upbutton;
        public static GameObject downbutton;

        [HarmonyPatch(typeof(KepRemapPanel), "OnEnable")]
        [HarmonyPrefix]
        public static void OnEnable_KepRemapPanel(KepRemapPanel __instance)
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
                if (cache != null)
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
            if (up == null)
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

        #endregion
    }
}
