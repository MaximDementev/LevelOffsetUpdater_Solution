using Autodesk.Revit.UI;

namespace LevelOffsetUpdater.Services
{
    // Интерфейс для сервиса ручного обновления
    public interface IManualUpdateService : IExternalEventHandler
    {
        void Raise();
        void RaiseOffsetUpdate();
        void RaiseWallDistanceUpdate();
    }
}
