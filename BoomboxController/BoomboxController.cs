using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.Controls;
using BoomboxController.Audio;
using BoomboxController.Boombox;
using BoomboxController.Save;
using BoomboxController.Commands;
using BoomboxController.Options;
using BoomboxController.Startups;
using BoomboxController.Vision;

namespace BoomboxController
{
    public class BoomboxController : Variables
    {

        public static SaveManager saveManager;

        public void InitializationBoombox()
        {
            saveManager = new SaveManager();
            Plugin.HarmonyLib.PatchAll(typeof(AudioManager));
            Plugin.HarmonyLib.PatchAll(typeof(BoomboxManager));
            Plugin.HarmonyLib.PatchAll(typeof(CommandManager));
            Plugin.HarmonyLib.PatchAll(typeof(MenuManager));
            Plugin.HarmonyLib.PatchAll(typeof(OptionManager));
            Plugin.HarmonyLib.PatchAll(typeof(SaveManager));
            Plugin.HarmonyLib.PatchAll(typeof(StartupManager));
            Plugin.HarmonyLib.PatchAll(typeof(VisionManager));
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
    }
}
