using BepInEx.Bootstrap;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BoomboxController.Startups
{
    public class StartupManager : BoomboxController
    {
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
        public static void OnEnable_StartOfRound(StartOfRound __instance)
        {
            if (quits == null)
            {
                quits = new GameObject("QuitManager").AddComponent<QuitManager>();
                UnityEngine.Object.DontDestroyOnLoad((UnityEngine.Object)(object)quits);
                //network = new GameObject("NetworkBoombox").AddComponent<NetworkBoombox>();
                //UnityEngine.Object.DontDestroyOnLoad((UnityEngine.Object)(object)network);
            }
        }
    }
}
