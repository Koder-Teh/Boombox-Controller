using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BoomboxController
{
    public class Downloader
    {
        public static void DownloadFilesToUnpacking(Uri uri, string filename)
        {
            WebClient web = new WebClient();
            web.DownloadFileCompleted += Web_DownloadFileCompletedToUnpacking;
            web.DownloadFileAsync(uri, filename);
        }

        public static void DownloadFiles(Uri uri, string filename)
        {
            WebClient web = new WebClient();
            web.DownloadFileCompleted += Web_DownloadFileCompleted;
            web.DownloadFileAsync(uri, filename);
        }

        private static void Web_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            Thread.CurrentThread.Abort();
        }

        private static void Web_DownloadFileCompletedToUnpacking(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            Unpacking();
            if (!File.Exists(@$"BoomboxController\other\yt-dlp.exe"))
            {
                Thread thread = new Thread(() => DownloadFiles(new Uri("https://github.com/yt-dlp/yt-dlp/releases/download/2025.11.12/yt-dlp.exe"), @"BoomboxController\other\yt-dlp.exe"));
                thread.Start();
            }
            Thread.CurrentThread.Abort();
        }

        public static bool Unpacking()
        {
            try
            {
                using (ZipArchive zip = ZipFile.OpenRead(@"BoomboxController\other\ffmpeg-master-latest-win64-gpl.zip"))
                {
                    foreach (ZipArchiveEntry entry in zip.Entries)
                    {
                        if (entry.Name.Equals("ffmpeg.exe")) entry.ExtractToFile(Path.Combine(@"BoomboxController\other", entry.Name));
                    }
                }
                File.Delete(@"BoomboxController\other\ffmpeg-master-latest-win64-gpl.zip");
                return true;
            }
            catch (InvalidDataException)
            {
                Plugin.instance.Log("Zip файл поврежден, потому что он не был загружен должным образом. Сложно ли не закрывать игру и просто позволить загрузке файлов завершиться?");
                Plugin.instance.Log("Zip is broken because it wasn't downloaded properly. Is it difficult to not close the game and just let the downloading of files to finish?");
                File.Delete(@"BoomboxController\other\ffmpeg-master-latest-win64-gpl.zip");
                return false;
            }
        }
    }
}
