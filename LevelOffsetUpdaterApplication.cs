using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.UI;
using LevelOffsetUpdater;
using LevelOffsetUpdater.Core;
using LevelOffsetUpdater.Services;
using System;
using System.IO;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace KRGPMagic.Plugins.LevelOffsetUpdater
{
    // Главный класс приложения для управления плагином обновления отметок
    public class LevelOffsetUpdaterApplication : IExternalApplication
    {
        #region Fields
        private const string TAB_NAME = "KRGPMagic";
        private const string PANEL_NAME = "Проемы и отверстия";
        private const string SETTINGS_BUTTON_NAME = "Настройки обновления";
        private const string UPDATE_BUTTON_NAME = "Обновить\nотметки проемов";
        private const string UPDATE_TO_WALL_DIST_BUTTON_NAME = "Обновить\nрасстояния до НС";


        private AutoUpdateEventHandler _autoUpdateService;
        #endregion

        #region Static Properties
        public static AutoUpdateEventHandler AutoUpdateService { get; private set; }
        #endregion

        #region IExternalApplication Implementation
        // Инициализирует 
        public Result OnStartup(UIControlledApplication application)
        {
            try
            {
                var elementFilter = new DoorWindowFilter();
                _autoUpdateService = new AutoUpdateEventHandler(application.ControlledApplication, elementFilter);
                AutoUpdateService = _autoUpdateService;
                _autoUpdateService.Initialize();

                // Создаем UI элементы
                CreateRibbonPanel(application);

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Ошибка запуска LevelOffsetUpdater",
                    $"Не удалось инициализировать приложение: {ex.Message}");
                return Result.Failed;
            }
        }

        // Завершает работу приложения при закрытии Revit
        public Result OnShutdown(UIControlledApplication application)
        {
            try
            {
                _autoUpdateService?.Shutdown();
                AutoUpdateService = null;
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Ошибка завершения LevelOffsetUpdater",
                    $"Ошибка при завершении работы: {ex.Message}");
                return Result.Failed;
            }
        }
        #endregion

        #region Private Methods
        // Создает панель с кнопками на ленте Revit
        private void CreateRibbonPanel(UIControlledApplication application)
        {
            // Получаем или создаем вкладку
            RibbonPanel panel = GetOrCreateRibbonPanel(application, TAB_NAME, PANEL_NAME);

            // Создаем SplitButton вместо отдельных кнопок
            CreateLevelOffsetSplitButton(panel);
        }

        // Получает существующую или создает новую панель на ленте
        private RibbonPanel GetOrCreateRibbonPanel(UIControlledApplication application, string tabName, string panelName)
        {
            try
            {
                // Пытаемся создать вкладку (если уже существует, будет проигнорировано)
                application.CreateRibbonTab(tabName);
            }
            catch
            {
                // Вкладка уже существует
            }

            // Ищем существующую панель
            foreach (RibbonPanel existingPanel in application.GetRibbonPanels(tabName))
            {
                if (existingPanel.Name == panelName)
                    return existingPanel;
            }

            // Создаем новую панель
            return application.CreateRibbonPanel(tabName, panelName);
        }

        // Создает SplitButton с обеими командами
        private void CreateLevelOffsetSplitButton(RibbonPanel panel)
        {
            // Создаем кнопку обновления отметок
            var mainButtonData = new PushButtonData(
                nameof(UpdateCommand),
                UPDATE_BUTTON_NAME,
                Assembly.GetExecutingAssembly().Location,
                typeof(UpdateCommand).FullName);

            mainButtonData.ToolTip = "Обновить отметки расположения у всех дверей и окон";
            mainButtonData.LongDescription = $"Выполняет ручное обновление параметра  {Constants.TARGET_PARAMETER_NAME} для всех дверей и окон в текущем проекте";

            // Создаем кнопку обновления расстояния до НС
            var distanceToWallButtonData = new PushButtonData(
                nameof(UpdateWallDistanceCommand),
                UPDATE_TO_WALL_DIST_BUTTON_NAME,
                Assembly.GetExecutingAssembly().Location,
                typeof(UpdateWallDistanceCommand).FullName);

            distanceToWallButtonData.ToolTip = "Обновить расстояния до низа стен у всех дверей и окон";
            distanceToWallButtonData.LongDescription = $"Выполняет ручное обновление параметра {Constants.WALL_DISTANCE_PARAMETER_NAME} для всех дверей и окон в текущем проекте";

            

            // Создаем кнопку настроек для выпадающего меню
            var settingsButtonData = new PushButtonData(
                nameof(SettingsCommand),
                SETTINGS_BUTTON_NAME,
                Assembly.GetExecutingAssembly().Location,
                typeof(SettingsCommand).FullName);

            settingsButtonData.ToolTip = "Открыть настройки автоматического обновления отметок расположения";
            settingsButtonData.LongDescription = "Позволяет настроить параметры автоматического обновления параметров для дверей и окон";

            // Создаем SplitButton
            var splitButtonData = new SplitButtonData("LevelOffsetUpdaterSplit", "Level Offset");
            var splitButton = panel.AddItem(splitButtonData) as SplitButton;

            // Добавляем основную кнопку
            var mainButton = splitButton.AddPushButton(mainButtonData);

            // Добавляем основную кнопку
            var distanceToWallButton = splitButton.AddPushButton(distanceToWallButtonData);

            // Добавляем кнопку настроек в выпадающее меню
            var settingsButton = splitButton.AddPushButton(settingsButtonData);

            // Устанавливаем иконки
            var updateIcon = GetIconPath("update.png");
            if (File.Exists(updateIcon))
            {
                mainButton.LargeImage = new BitmapImage(new Uri(updateIcon));
                mainButton.Image = new BitmapImage(new Uri(updateIcon));
                splitButton.LargeImage = new BitmapImage(new Uri(updateIcon));
            }

            // Устанавливаем иконки
            var distanceToWallUpdateIcon = GetIconPath("update.png");
            if (File.Exists(updateIcon))
            {
                distanceToWallButton.LargeImage = new BitmapImage(new Uri(updateIcon));
                distanceToWallButton.Image = new BitmapImage(new Uri(updateIcon));
                splitButton.LargeImage = new BitmapImage(new Uri(updateIcon));
            }

            var settingsIcon = GetIconPath("settings.png");
            if (File.Exists(settingsIcon))
            {
                settingsButton.LargeImage = new BitmapImage(new Uri(settingsIcon));
                settingsButton.Image = new BitmapImage(new Uri(settingsIcon));
            }
        }

        // Получает путь к иконке
        private string GetIconPath(string iconFileName)
        {
            string assemblyLocation = Assembly.GetExecutingAssembly().Location;
            string assemblyDirectory = Path.GetDirectoryName(assemblyLocation);
            return Path.Combine(assemblyDirectory, "Icons", iconFileName);
        }
        #endregion
    }

}
