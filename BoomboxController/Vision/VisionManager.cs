using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.InputSystem;
using UnityEngine;
using System.IO;

namespace BoomboxController.Vision
{
    public class VisionManager : BoomboxController
    {
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
                                                        saveManager.SaveCache(vol, up == null ? null : up.displayName, down == null ? null : down.displayName);
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
                                                        saveManager.SaveCache(vol, up == null ? null : up.displayName, down == null ? null : down.displayName);
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
                                                        saveManager.SaveCache(vol, up == null ? null : up.displayName, down == null ? null : down.displayName);
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
                                                        saveManager.SaveCache(vol, up == null ? null : up.displayName, down == null ? null : down.displayName);
                                                    }
                                                }
                                            }
                                            //if (Keyboard.current.upArrowKey.wasPressedThisFrame)
                                            //{
                                            //    Plugin.instance.Log(__instance.NetworkObjectId + " " + __instance.NetworkBehaviourId);
                                            //    network.PositionChange_Server();
                                            //}
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
    }
}
