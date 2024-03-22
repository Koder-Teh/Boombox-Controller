using BoomboxController.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.InputSystem.Controls;
using UnityEngine;

namespace BoomboxController
{
    public class Variables : MonoBehaviour
    {
        #region Type
        public static AudioBoomBox bom;
        public static VisualBoombox vbom;
        public static BoomboxItem boomboxItem = new BoomboxItem();
        public static AudioClip[] musicList;
        public static QuitManager quit;
        public static QuitManager quits;
        public static KeyControl up = null;
        public static KeyControl down = null;
        #endregion

        #region Int
        public static int timesPlayedWithoutTurningOff = 0;
        public static int isSendingItemRPC = 0;
        public static int Id = 0;
        public static int totalTack = 0;
        public static int currectTrack = 0;
        #endregion

        #region Double
        public static double curretTime = 0;
        public static double totalTime = 0;
        #endregion

        #region Bool
        public static bool startMusics = true;
        public static bool LoadingMusicBoombox = false;
        public static bool LoadingLibrary = false;
        public static bool isplayList = false;
        internal static bool blockcompatibility = false;
        public static bool waitAutoNext = false;
        public static bool netSwitch = true;
        public static bool currentTrackChange = false;
        #endregion

        #region String
        public static string LastMessage;
        public static string LastnameOfUserWhoTyped;
        public static string NameTrack;
        public static string[] sumbols = { "+", "#", "�" };
        #endregion
    }
}
