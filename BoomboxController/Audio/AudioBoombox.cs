using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Networking;
using UnityEngine;
using System.IO;
using BepInEx;
using System.Drawing.Design;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using UnityEngine.UIElements;
using System.Threading;

namespace BoomboxController.Audio
{
    public class AudioBoomBox : MonoBehaviour
    {
        private static AudioBoomBox _instance;

        public List<AudioClip> audioclips = new List<AudioClip>();

        public List<AudioClip> audioclipsplay = new List<AudioClip>();

        public Coroutine Start(IEnumerator routine)
        {
            if (_instance == null)
            {
                _instance = new GameObject("AudioBoomBox").AddComponent<AudioBoomBox>();
                DontDestroyOnLoad((UnityEngine.Object)(object)_instance);
            }
            return _instance.StartCoroutine(routine);
        }

        public IEnumerator GetAudioClip(string url, BoomboxItem boombox, AudioType type)
        {
            audioclips.Clear();
            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, type))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.ConnectionError)
                {
                    BoomboxController.LoadingMusicBoombox = false;
                    Plugin.instance.Log(www.error);
                    BoomboxController.isplayList = false;
                }
                else
                {
                    AudioClip myClip = DownloadHandlerAudioClip.GetContent(www);
                    audioclips.Add(myClip);
                    BoomboxController.musicList = audioclips.ToArray();
                    BoomboxController.LoadingMusicBoombox = false;
                    BoomboxController.isplayList = false;
                }
            }
        }

        public async Task GetPlayList(string url, BoomboxItem boombox, AudioType type)
        {
            Plugin.instance.Log(url);
            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, type))
            {
                var content = www.SendWebRequest();

                while (!content.isDone) await Task.Delay(100);

                if (www.result == UnityWebRequest.Result.ConnectionError)
                {
                    Plugin.instance.Log(www.error);
                }
                else
                {
                    AudioClip myClip = DownloadHandlerAudioClip.GetContent(www);
                    audioclipsplay.Add(myClip);
                    //Plugin.instance.Log(url);
                }
            }
        }
    }
}
