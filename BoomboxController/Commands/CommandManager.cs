using BepInEx;
using BoomboxController.Audio;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace BoomboxController.Commands
{
    public class CommandManager : BoomboxController
    {
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
            __instance.chatTextField.characterLimit = 1000;
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
                        if (!netSwitch) break;
                        if (vs.Length == 1) break;
                        //Regex regex = new Regex("^https?:\\/\\/(?:www\\.)?[-a-zA-Z0-9@:%._\\+~#=]{1,256}\\.[a-zA-Z0-9()]{1,6}\\b(?:[-a-zA-Z0-9()@:%_\\+.~#?&\\/=]*)$");
                        if (Uri.IsWellFormedUriString(vs[1], UriKind.Absolute))
                        {
                            var url = vs[1].Remove(0, 8);
                            switch (url.Substring(0, url.IndexOf('/')))
                            {
                                case "music.youtube.com":
                                    if (url.Remove(0, url.IndexOf('/')) == "/watch") break;
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
                                    if (url.Remove(0, url.IndexOf('/')) == "/watch") break;
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
                                    if (url.Remove(0, url.IndexOf('/')) == "/watch") break;
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
                                            await AudioManager.LoadPlaylist(__instance, nameOfUserWhoTyped);
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
                                    int type = AudioManager.GetAudioType(vs[1].Remove(0, vs[1].Length - 3));
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
                                if (startMusics)
                                {
                                    boomboxItem.isPlayingMusic = true;
                                    boomboxItem.isBeingUsed = true;
                                    startMusics = false;
                                }
                                currentTrackChange = true;
                                DrawString(__instance, Plugin.config.GetLang().main_14.Value.Replace("@1", $"{vs[1]}"), "Boombox", nameOfUserWhoTyped);
                            }
                        }
                        break;
                    case "bswitch":
                        if (vs.Length == 1) break;
                        switch (vs[1])
                        {
                            case "net":
                                netSwitch = true;
                                musicList = null;
                                totalTack = 0;
                                boomboxItem.boomboxAudio.Stop();
                                boomboxItem.boomboxAudio.PlayOneShot(boomboxItem.stopAudios[UnityEngine.Random.Range(0, boomboxItem.stopAudios.Length)]);
                                timesPlayedWithoutTurningOff = 0;
                                boomboxItem.isPlayingMusic = false;
                                boomboxItem.isBeingUsed = false;
                                currectTrack = 0;
                                boomboxItem.boomboxAudio.time = 0;
                                DrawString(__instance, "The link URL is available!", "Boombox", nameOfUserWhoTyped);
                                break;
                            case "local":
                                if (!isplayList)
                                {
                                    NameTrack = "Local-Music";
                                    isplayList = true;
                                    DrawString(__instance, "Loading...", "Boombox", nameOfUserWhoTyped);
                                    boomboxItem.boomboxAudio.Stop();
                                    boomboxItem.boomboxAudio.PlayOneShot(boomboxItem.stopAudios[UnityEngine.Random.Range(0, boomboxItem.stopAudios.Length)]);
                                    timesPlayedWithoutTurningOff = 0;
                                    boomboxItem.isPlayingMusic = false;
                                    boomboxItem.isBeingUsed = false;
                                    currectTrack = 0;
                                    boomboxItem.boomboxAudio.time = 0;
                                    await AudioManager.LoadMusicLocal(__instance, nameOfUserWhoTyped);
                                    netSwitch = false;
                                    DrawString(__instance, "Local music is available!", "Boombox", nameOfUserWhoTyped);
                                }
                                break;
                        }
                        break;
                    case "bload":
                        if (netSwitch) break;
                        if (isplayList)
                        {
                            isplayList = true;
                            DrawString(__instance, "Loading...", "Boombox", nameOfUserWhoTyped);
                            boomboxItem.boomboxAudio.Stop();
                            boomboxItem.boomboxAudio.PlayOneShot(boomboxItem.stopAudios[UnityEngine.Random.Range(0, boomboxItem.stopAudios.Length)]);
                            timesPlayedWithoutTurningOff = 0;
                            boomboxItem.isPlayingMusic = false;
                            boomboxItem.isBeingUsed = false;
                            currectTrack = 0;
                            boomboxItem.boomboxAudio.time = 0;
                            await AudioManager.LoadMusicLocal(__instance, nameOfUserWhoTyped);
                        }
                        break;
                        //case "bmenu":
                        //    if (!isplayList)
                        //    {
                        //        isplayList = true;
                        //        CreateMenu();
                        //    }
                        //    break;
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
                if (!string.IsNullOrEmpty(__instance.chatTextField.text) && __instance.chatTextField.text.Length < 1000)
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
            if (!string.IsNullOrEmpty(__instance.chatTextField.text) && __instance.chatTextField.text.Length < 1000)
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

        public static bool IsCommand(string text, string[] args)
        {
            foreach (string command in args)
            {
                if (text.Contains(command)) return true;
            }
            return false;
        }
    }
}
