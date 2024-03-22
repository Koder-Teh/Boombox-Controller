using BepInEx;
using BoomboxController.Audio;
using BoomboxController.Save;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Drawing.Design;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using UnityEngine;
using System.IO;
using Cache = BoomboxController.Save.Cache;

namespace BoomboxController.Boombox
{
    public class BoomboxManager : BoomboxController
    {
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
            Cache cache = saveManager.LoadCache();
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
                    if (!currentTrackChange)
                    {
                        currectTrack = UnityEngine.Random.Range(0, totalTack - 1);
                    }
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
                    currentTrackChange = false;
                }
            }
            //else
            //{
            //    boomboxItem.isBeingUsed = false;
            //    boomboxItem.UseItemOnClient();
            //    //DrawString(HUDManager.Instance, Plugin.config.GetLang().main_3.Value, "Boombox", "Boombox");
            //}
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
                    if (Math.Floor(curretTime) == Math.Floor(totalTime))
                    {
                        boomboxItem.boomboxAudio.Stop();
                        boomboxItem.isPlayingMusic = false;
                        waitAutoNext = true;
                    }
                }
                else
                {
                    if (totalTack == 1)
                    {
                        if (Math.Floor(curretTime) == Math.Floor(totalTime))
                        {
                            boomboxItem.boomboxAudio.time = 0;
                            return;
                        }
                    }
                    else
                    {
                        if (Math.Floor(curretTime) == (Math.Floor(totalTime) - 1))
                        {
                            boomboxItem.boomboxAudio.Stop();
                            currectTrack = 0;
                            boomboxItem.boomboxAudio.time = 0;
                            boomboxItem.boomboxAudio.clip = musicList[currectTrack];
                            boomboxItem.boomboxAudio.Play();
                        }
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
    }
}
