using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoomboxController.Save
{
    public class SaveManager
    {
        public void SaveCache(float vol, string up, string down)
        {
            Cache cache = new Cache();
            cache.Volume = vol;
            cache.UpButton = up;
            cache.DownButton = down;
            string json = JsonConvert.SerializeObject(cache);
            using (StreamWriter sw = new StreamWriter(@"BoomboxController\cache"))
            {
                sw.WriteLine(json);
            }
        }

        public Cache LoadCache()
        {
            string json = String.Empty;
            if (File.Exists(@"BoomboxController\cache"))
            {
                using (StreamReader sr = new StreamReader(@"BoomboxController\cache"))
                {
                    json = sr.ReadToEnd();
                }
                return JsonConvert.DeserializeObject<Cache>(json);
            }
            return null;
        }
    }
}
