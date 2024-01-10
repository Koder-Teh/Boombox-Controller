using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoomboxController
{
    public class Configs
    {
        public ConfigEntry<bool> requstbattery;
        public ConfigEntry<bool> pocketitem;
        public ConfigEntry<string> languages;
        public ConfigEntry<bool> visual;
        public ConfigEntry<string> body;
        public ConfigEntry<string> otherelem;

        public static Lang lang = new Lang();

        public void GetConfig()
        {
            var customFile = new ConfigFile(@"BoomboxController\boombox_controller.cfg", true);
            requstbattery = customFile.Bind("General.Toggles", "RequestBattery", false, "Enable/disable boombox battery (true = Enable; false = Disable)");
            pocketitem = customFile.Bind("General.Toggles", "PocketItem", true, "Enable/disable music in your pocket. (true = Enable; false = Disable)");
            languages = customFile.Bind("General", "Languages", "en", "EN/RU");
            visual = customFile.Bind("Visual", "Visual", false, "Enable/Disable Visual Elements of Boombox");
            body = customFile.Bind("Visual", "Body", "#FFFFFF", "Color body Boombox");
            otherelem = customFile.Bind("Visual", "Other", "#000000", "Color Other Elements Boombox");
        }

        public Lang GetLang()
        {
            return lang;
        }

        public class Lang
        {
            public ConfigEntry<string> main_1;
            public ConfigEntry<string> main_2;
            public ConfigEntry<string> main_3;
            public ConfigEntry<string> main_4;
            public ConfigEntry<string> main_5;
            public ConfigEntry<string> main_6;
            public ConfigEntry<string> main_7;
            public ConfigEntry<string> main_8;
            public ConfigEntry<string> main_9;
            public ConfigEntry<string> main_10;
            public ConfigEntry<string> main_11;
            public ConfigEntry<string> main_12;
            public ConfigEntry<string> main_13;
            public ConfigEntry<string> main_14;

            public void GetConfigRU()
            {
                var customFile = new ConfigFile(@"BoomboxController\lang\boombox_ru.cfg", true);
                main_1 = customFile.Bind("General", "Main_1", "Пожалуйста, подождите, загружаются дополнительные библиотеки, чтобы модификация заработала.");
                main_2 = customFile.Bind("General", "Main_2", "Взять BoomBox[1.1.6] : [E]\n@2 - @3\n@1 громкость\nСейчас играет: @4\nДоступных треков: @5");
                main_3 = customFile.Bind("General", "Main_3", "Все дополнительные библиотеки загружены, теперь вы можете использовать команды для бумбокса.");
                main_4 = customFile.Bind("General", "Main_4", "Подождите, трек еще загружается!");
                main_5 = customFile.Bind("General", "Main_5", "Команды:\n/bplay - Проиграть музыку\n/btime - Изменить позицию песни\n/bvolume - Изменить громкость трека");
                main_6 = customFile.Bind("General", "Main_6", "Введите правильный URL-адрес!");
                main_7 = customFile.Bind("General", "Main_7", "Пожалуйста подождите...");
                main_8 = customFile.Bind("General", "Main_8", "Трек был загружен в бумбокс");
                main_9 = customFile.Bind("General", "Main_9", "@1 изменил громкость трека @2");
                main_10 = customFile.Bind("General", "Main_10", "Введите правильную громкость трека (пример: 0, 10, 20, 30...)!");
                main_11 = customFile.Bind("General", "Main_11", "Ссылка недействительная!");
                main_12 = customFile.Bind("General", "Main_12", "Позиция трека изменена на @1!");
                main_13 = customFile.Bind("General", "Main_13", "Загрузка трека отменена!");
                main_14 = customFile.Bind("General", "Main_14", "Текущий трек был переключен на: @1!");
            }

            public void GetConfigEN()
            {
                var customFile = new ConfigFile(@"BoomboxController\lang\boombox_en.cfg", true);
                main_1 = customFile.Bind("General", "Main_1", "Please wait, additional libraries are being loaded for the modification to work.");
                main_2 = customFile.Bind("General", "Main_2", "Pickup BoomBox[1.1.6] : [E]\n@2 - @3\n@1 volume\nNow playing: @4\nAvailable tracks: @5");
                main_3 = customFile.Bind("General", "Main_3", "All libraries have loaded, now you can use the boombox commands.");
                main_4 = customFile.Bind("General", "Main_4", "Another track is being uploaded to the boombox!");
                main_5 = customFile.Bind("General", "Main_5", "Commands:\n/bplay - Play music\n/btime - Change the position of the song\n/bvolume - Change Boombox volume");
                main_6 = customFile.Bind("General", "Main_6", "Enter the correct URL!");
                main_7 = customFile.Bind("General", "Main_7", "Please wait...");
                main_8 = customFile.Bind("General", "Main_8", "The track was uploaded to the boombox");
                main_9 = customFile.Bind("General", "Main_9", "@1 changed the volume @2 of the boombox.");
                main_10 = customFile.Bind("General", "Main_10", "Enter the correct Volume (example: 0, 10, 20, 30...)!");
                main_11 = customFile.Bind("General", "Main_11", "Link is invalid!");
                main_12 = customFile.Bind("General", "Main_12", "Track position changed to @1!");
                main_13 = customFile.Bind("General", "Main_13", "Track download canceled!");
                main_14 = customFile.Bind("General", "Main_14", "The current track has been switched to: @1!");
            }
        }
    }
}
