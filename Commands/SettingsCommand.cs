using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using LevelOffsetUpdater.Core;
using LevelOffsetUpdater.Services;
using LevelOffsetUpdater.UI;

namespace KRGPMagic.Plugins.LevelOffsetUpdater
{
    // Команда для открытия окна настроек
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class SettingsCommand : IExternalCommand
    {
        #region IPlugin Implementation
        public bool IsEnabled { get; set; }

        public bool Initialize()
        {
            return true;
        }

        public void Shutdown()
        {
        }
        #endregion

        #region IExternalCommand Implementation
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                var autoUpdateService = LevelOffsetUpdaterApplication.AutoUpdateService;
                var elementFilter = new DoorWindowFilter();
                var manualUpdateService = new ManualUpdateEventHandler(elementFilter);

                if (autoUpdateService == null || manualUpdateService == null)
                {
                    message = "Один или несколько сервисов не инициализированы.";
                    TaskDialog.Show("Settings Command", message);
                    return Result.Failed;
                }

                using (var form = new SettingsForm(autoUpdateService, manualUpdateService))
                {
                    form.ShowDialog();
                }
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                TaskDialog.Show("Settings Command Error", $"Произошла ошибка: {ex.Message}");
                return Result.Failed;
            }
        }
        #endregion
    }
}
