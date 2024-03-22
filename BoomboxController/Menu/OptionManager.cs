using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem;
using UnityEngine;
using BoomboxController.Save;
using Cache = BoomboxController.Save.Cache;

namespace BoomboxController.Options
{
    public class OptionManager : BoomboxController
    {
        public static GameObject upbutton;
        public static GameObject downbutton;

        [HarmonyPatch(typeof(KepRemapPanel), "OnEnable")]
        [HarmonyPrefix]
        public static void OnEnable_KepRemapPanel(KepRemapPanel __instance)
        {
            __instance.keyRemapContainer.parent.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 900f);
            Cache cache = saveManager.LoadCache();
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
            Cache cache = saveManager.LoadCache();
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
                    saveManager.SaveCache(cache.Volume, up.displayName, cache.DownButton);
                }
                else
                {
                    saveManager.SaveCache(0.5f, up.displayName, null);
                }
            }
            else
            {
                if (cache != null)
                {
                    saveManager.SaveCache(cache.Volume, up.displayName, down.displayName);
                }
                else
                {
                    saveManager.SaveCache(0.5f, up.displayName, down.displayName);
                }
            }
        }

        private static async void BoomboxController_onClickDown()
        {
            Cache cache = saveManager.LoadCache();
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
                    saveManager.SaveCache(cache.Volume, cache.UpButton, down.displayName);
                }
                else
                {
                    saveManager.SaveCache(0.5f, null, down.displayName);
                }
            }
            else
            {
                if (cache != null)
                {
                    saveManager.SaveCache(cache.Volume, up.displayName, down.displayName);
                }
                else
                {
                    saveManager.SaveCache(0.5f, up.displayName, down.displayName);
                }
            }
        }
    }
}
