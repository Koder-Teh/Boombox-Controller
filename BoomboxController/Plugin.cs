using BepInEx;
using HarmonyLib;
using System;
using System.IO;
using System.Threading;

namespace BoomboxController
{
    [BepInPlugin("KoderTech.BoomboxController", "BoomboxController", Version)]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin instance;

        public static Harmony HarmonyLib;

        public static Configs config;

        public static BoomboxController controller;

        public const string Version = "1.2.3";

        private void Awake()
        {
            instance = this;
            config = new Configs();
            controller = new BoomboxController();
            HarmonyLib = new Harmony("com.kodertech.BoomboxController");
            Startup();
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

        public void Startup()
        {
            new WinApi().SizeConsole(1500, 500);
            WriteLogo();
            SwitchLanguage();
            if (File.Exists(@$"BoomboxController\lang\boombox_ru.cfg")) File.Delete(@$"BoomboxController\lang\boombox_ru.cfg");
            if (File.Exists(@$"BoomboxController\lang\boombox_en.cfg")) File.Delete(@$"BoomboxController\lang\boombox_en.cfg");
            if (!Directory.Exists(@$"BoomboxController\lang")) Directory.CreateDirectory(@$"BoomboxController\lang");
            if (!Directory.Exists(@$"BoomboxController\other")) Directory.CreateDirectory(@$"BoomboxController\other");
            if (!Directory.Exists(@$"BoomboxController\other\local")) Directory.CreateDirectory(@$"BoomboxController\other\local");
            if (!Directory.Exists(@$"BoomboxController\other\playlist")) Directory.CreateDirectory(@$"BoomboxController\other\playlist");
            if (!File.Exists(@$"BoomboxController\other\ffmpeg.exe"))
            {
                if (File.Exists(@$"BoomboxController\other\ffmpeg-master-latest-win64-gpl.zip"))
                {
                    if (!Downloader.Unpacking())
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
            controller.InitializationBoombox();
        }

        public void SwitchLanguage()
        {
            switch (config.languages.Value.ToLower())
            {
                case "ru":
                    config.GetLang().GetConfigRU();
                    break;
                case "en":
                    config.GetLang().GetConfigEN();
                    break;
            }
        }

        public void Log(object message)
        {
            Logger.LogInfo(message);
        }
    }
}
