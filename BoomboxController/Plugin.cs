using BepInEx;
using BepInEx.Bootstrap;
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
using System.Runtime.InteropServices;
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
    [BepInPlugin("KoderTech.BoomboxController", "BoomboxController", Version)]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin instance;

        private static Harmony HarmonyLib;

        public static Configs config;

        public const string Version = "1.2.2";

        private void Awake()
        {
            new WinApi().SizeConsole(1500, 500);
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
            if (!Directory.Exists(@$"BoomboxController\other\local")) Directory.CreateDirectory(@$"BoomboxController\other\local");
            if (!Directory.Exists(@$"BoomboxController\other\playlist")) Directory.CreateDirectory(@$"BoomboxController\other\playlist");
            if (!File.Exists(@$"BoomboxController\other\ffmpeg.exe"))
            {
                if (File.Exists(@$"BoomboxController\other\ffmpeg-master-latest-win64-gpl.zip"))
                {
                    if(!Downloader.Unpacking())
                    {
                        Thread thread = new Thread(() => Downloader.DownloadFilesToUnpacking(new Uri("https://github.com/BtbN/FFmpeg-Builds/releases/download/latest/ffmpeg-master-latest-win64-gpl.zip"), @"BoomboxController\other\ffmpeg-master-latest-win64-gpl.zip"));
                        thread.Start();
                    }
                }
                else
                {
                    Thread thread = new Thread(() => Downloader.DownloadFilesToUnpacking(new Uri("https://github.com/BtbN/FFmpeg-Builds/releases/download/latest/ffmpeg-master-latest-win64-gpl.zip"), @"BoomboxController\other\ffmpeg-master-latest-win64-gpl.zip"));
                    thread.Start();
                }
            }
            HarmonyLib = new Harmony("com.kodertech.BoomboxController");
            HarmonyLib.PatchAll(typeof(BoomboxController));
            HarmonyLib.PatchAll(typeof(GrabbleBoombox));
        }

        public void WriteLogo()
        {
            Logger.LogInfo($"\n" +
                     $"                                                                                                                                                                  \n" +
                     $"`7MM\"\"\"Yp,                                   *MM                                 .g8\"\"\"bgd                  mm                   `7MM `7MM                  \n" +
                     $"  MM    Yb                                    MM                               .dP'     `M                  MM                     MM   MM                  \n" +
                     $"  MM    dP  ,pW\"Wq.   ,pW\"Wq.`7MMpMMMb.pMMMb. MM,dMMb.   ,pW\"Wq.`7M'   `MF'    dM'       `,pW\"Wq.`7MMpMMMbmmMMmm `7Mb,od8 ,pW\"Wq.  MM   MM  .gP\"Ya `7Mb,od8 \n" +
                     $"  MM\"\"\"bg. 6W'   `Wb 6W'   `Wb MM    MM    MM MM    `Mb 6W'   `Wb `VA ,V'      MM        6W'   `Wb MM    MM MM     MM' \"'6W'   `Wb MM   MM ,M'   Yb  MM' \"' \n" +
                     $"  MM    `Y 8M     M8 8M     M8 MM    MM    MM MM     M8 8M     M8   XMX        MM.       8M     M8 MM    MM MM     MM    8M     M8 MM   MM 8M\"\"\"\"\"\"  MM     \n" +
                     $"  MM    ,9 YA.   ,A9 YA.   ,A9 MM    MM    MM MM.   ,M9 YA.   ,A9 ,V' VA.      `Mb.     ,YA.   ,A9 MM    MM MM     MM    YA.   ,A9 MM   MM YM.    ,  MM     \n" +
                     $".JMMmmmd9   `Ybmd9'   `Ybmd9'.JMML  JMML  JMMLP^YbmdP'   `Ybmd9'.AM.   .MA.      `\"bmmmd' `Ybmd9'.JMML  JMML`Mbmo.JMML.   `Ybmd9'.JMML.JMML.`Mbmmd'.JMML.   \n" +
                     $"                                                                                                                                                                  ");
        }

        public void Log(object message)
        {
            Logger.LogInfo(message);
        }
    }
}
