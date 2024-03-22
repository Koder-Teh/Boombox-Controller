using BepInEx.Bootstrap;
using BepInEx;
using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.Drawing.Design;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UIElements;
using UnityEngine;

namespace BoomboxController.Audio
{
    public class AudioManager : BoomboxController
    {
        public static int GetAudioType(string ext)
        {
            switch (ext)
            {
                case "mp3": return 13;
            }
            return -1;
        }

        public static async Task LoadMusicLocal(HUDManager __instance, string nameOfUserWhoTyped)
        {
            await Task.Run(async () =>
            {
                List<FileInfo> files = new List<FileInfo>();
                List<FileInfo> dependMusic = await Dependants_LocalMusic();
                FileInfo[] track = new DirectoryInfo(@"BoomboxController\other\local").GetFiles();
                foreach (var t in track)
                {
                    foreach (var f in sumbols)
                    {
                        if (t.Exists && t.Name.Contains(f))
                        {
                            t.MoveTo(@$"BoomboxController\other\local\{t.Name.Replace(f.ToString(), "")}");
                        }
                    }
                }
                files.AddRange(dependMusic);
                files.AddRange(track);
                foreach (FileInfo file in files)
                {
                    await bom.GetPlayList(@$"file:///{file.FullName}", boomboxItem, AudioType.MPEG);
                }
                if (track.Length != 0)
                {
                    musicList = bom.audioclipsplay.ToArray();
                    bom.audioclipsplay.Clear();
                }
                isplayList = false;
                LoadingMusicBoombox = false;
                DrawString(__instance, "Tracks Loading", "Boombox", nameOfUserWhoTyped);
            });
        }

        public static Task<List<FileInfo>> Dependants_LocalMusic()
        {
            List<FileInfo> files = new List<FileInfo>();
            string path = Chainloader.PluginInfos.ToArray()[0].Value.Location;
            string newpath = path.Substring(0, path.IndexOf("plugins"));
            DirectoryInfo[] deptrack = new DirectoryInfo($@"{newpath}\plugins").GetDirectories();
            foreach (var gfdg in deptrack)
            {
                foreach (var gewf in gfdg.GetDirectories())
                {
                    if (gewf.Name.Contains("BoomboxMusic"))
                    {
                        foreach (var gfd in gewf.GetFiles("*.mp3"))
                        {
                            files.Add(gfd);
                        }
                    }
                }
            }
            foreach (var t in files)
            {
                foreach (var f in sumbols)
                {
                    if (t.Name.Contains(f))
                    {
                        t.MoveTo(@$"{t.FullName.Replace(t.Name.ToString(), "")}\{t.Name.Replace(f.ToString(), "")}");
                    }
                }
            }
            return Task.FromResult<List<FileInfo>>(files);
        }

        public static async Task LoadPlaylist(HUDManager __instance, string nameOfUserWhoTyped)
        {
            await Task.Run(async () =>
            {
                FileInfo[] track = new DirectoryInfo(@"BoomboxController\other\playlist").GetFiles();
                foreach (var t in track)
                {
                    foreach (var f in sumbols)
                    {
                        if (t.Exists)
                        {
                            if (t.Name.Contains(f))
                            {
                                t.MoveTo(@$"BoomboxController\other\playlist\{t.Name.Replace(f, "")}");
                            }
                        }
                    }
                }
                foreach (FileInfo file in track)
                {
                    await bom.GetPlayList(@"file:///" + Paths.GameRootPath + @$"\BoomboxController\other\playlist\{file.Name}", boomboxItem, AudioType.MPEG);
                }
                musicList = bom.audioclipsplay.ToArray();
                bom.audioclipsplay.Clear();
                LoadingMusicBoombox = false;
                DrawString(__instance, Plugin.config.GetLang().main_8.Value, "Boombox YouTube", nameOfUserWhoTyped);
                isplayList = false;
            });
        }
    }
}
