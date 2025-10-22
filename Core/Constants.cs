namespace LevelOffsetUpdater.Core
{
    // Класс для хранения всех постоянных данных приложения
    public static class Constants
    {
        #region Parameter Names
        public const string TARGET_PARAMETER_NAME = "ADSK_Размер_Отметка расположения";
        public const string WALL_DISTANCE_PARAMETER_NAME = "KRGP_Отверстие_Расстояние до низа стены";
        #endregion

        #region Settings
        public const string SETTINGS_FOLDER_PATH = @"AppData\Roaming\MagicEntry\UserData\SettingsForApps";
        public const string SETTINGS_FILE_NAME = "LevelOffsetUpdater.json";
        public const int DEFAULT_ROUNDING_STEP_MM = 5;
        public const int DEFAULT_WARNING_THRESHOLD = 1000;
        #endregion

        #region UI Text
        public const string WINDOW_TITLE = "Настройки обновления отметок";
        public const string AUTO_UPDATE_CHECKBOX_TEXT = "Автоматически обновлять отметки расположения";
        public const string AUTO_UPDATE_WALL_DISTANCE_TEXT = "Автоматически обновлять расстояние до низа стены";
        public const string ROUNDING_STEP_LABEL_TEXT = "Шаг округления (мм):";
        public const string WARNING_THRESHOLD_LABEL_TEXT = "Предупреждать при количестве элементов больше:";
        public const string UPDATE_ALL_BUTTON_TEXT = "Обновить у всех сейчас";
        public const string CLOSE_BUTTON_TEXT = "Закрыть";
        #endregion

        #region Messages
        public const string PARAMETER_NOT_FOUND_MESSAGE = "Параметр '{0}' не найден у элемента. Автоматическое обновление отключено.";
        public const string PARAMETER_READ_ONLY_MESSAGE = "Параметр '{0}' заблокирован для редактирования. Автоматическое обновление отключено.";
        public const string PARAMETER_VALIDATION_ERROR_TITLE = "Ошибка обновления";
        public const string PARAMETER_VALIDATION_ERROR_MESSAGE = "Не удалось найти или получить доступ к следующим параметрам:\n{0}\n\nАвтоматическое обновление отключено.";
        public const string LARGE_UPDATE_WARNING_MESSAGE = "Найдено {0} элементов для обновления. Процесс может занять продолжительное время. Рекомендуется выполнить синхронизацию перед запуском. Продолжить?";
        public const string UPDATE_COMPLETED_MESSAGE = "Обновление завершено. Обработано элементов: {0}";
        public const string UPDATE_ERROR_MESSAGE = "Ошибка при обновлении: {0}";
        #endregion

        #region Categories
        public static readonly Autodesk.Revit.DB.BuiltInCategory[] TARGET_CATEGORIES =
        {
            Autodesk.Revit.DB.BuiltInCategory.OST_Doors,
            Autodesk.Revit.DB.BuiltInCategory.OST_Windows
        };
        #endregion
    }
}
