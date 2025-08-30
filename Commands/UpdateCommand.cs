using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using LevelOffsetUpdater.Core;
using LevelOffsetUpdater.Services;

namespace KRGPMagic.Plugins.LevelOffsetUpdater
{
    // Команда для ручного обновления отметок расположения
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class UpdateCommand : IExternalCommand
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
                var elementFilter = new DoorWindowFilter();
                var manualUpdateService = new ManualUpdateEventHandler(elementFilter);

                if (manualUpdateService == null)
                {
                    message = "Сервис ручного обновления не инициализирован.";
                    TaskDialog.Show("Update Command", message);
                    return Result.Failed;
                }

                manualUpdateService.Raise();
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                TaskDialog.Show("Update Command Error", $"Произошла ошибка: {ex.Message}");
                return Result.Failed;
            }
        }
        #endregion
    }
}
