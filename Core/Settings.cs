using System;
using System.IO;
using System.Xml;
using Newtonsoft.Json;
using Formatting = Newtonsoft.Json.Formatting;

namespace LevelOffsetUpdater.Core
{
    // Класс для управления настройками приложения
    public class Settings
    {
        #region Properties
        public bool AutoUpdateEnabled { get; set; } = false;
        public int RoundingStepMm { get; set; } = Constants.DEFAULT_ROUNDING_STEP_MM;
        public int WarningThreshold { get; set; } = Constants.DEFAULT_WARNING_THRESHOLD;
        #endregion

        #region Static Methods
        // Загружает настройки из файла или создает настройки по умолчанию
        public static Settings Load()
        {
            try
            {
                string settingsPath = GetSettingsFilePath();
                if (File.Exists(settingsPath))
                {
                    string json = File.ReadAllText(settingsPath);
                    return JsonConvert.DeserializeObject<Settings>(json) ?? new Settings();
                }
            }
            catch
            {
                // Игнорируем ошибки загрузки, используем настройки по умолчанию
            }
            return new Settings();
        }

        // Сохраняет настройки в файл
        public void Save()
        {
            try
            {
                string settingsPath = GetSettingsFilePath();
                string settingsDir = Path.GetDirectoryName(settingsPath);

                if (!Directory.Exists(settingsDir))
                {
                    Directory.CreateDirectory(settingsDir);
                }

                string json = JsonConvert.SerializeObject(this, Formatting.Indented);
                File.WriteAllText(settingsPath, json);
            }
            catch
            {
                // Игнорируем ошибки сохранения
            }
        }

        // Получает полный путь к файлу настроек
        private static string GetSettingsFilePath()
        {
            string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            return Path.Combine(userProfile, Constants.SETTINGS_FOLDER_PATH, Constants.SETTINGS_FILE_NAME);
        }
        #endregion
    }
}
