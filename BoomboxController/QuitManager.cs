using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BoomboxController
{
    public class QuitManager : MonoBehaviour
    {
        private void OnApplicationQuit()
        {
            DeleteFile();
        }

        public void DeleteFile()
        {
            foreach (FileInfo track in new DirectoryInfo(@"BoomboxController\other\playlist").GetFiles())
            {
                track.Delete();
            }
            FileInfo[] file = new DirectoryInfo(@"BoomboxController\other").GetFiles("*.mp3");
            if (file.Length == 1)
            {
                File.Delete(@$"BoomboxController\other\{file[0].Name}");
            }
        }
    }
}
