namespace LevelOffsetUpdater.Services
{
    // Интерфейс для сервиса автоматического обновления
    public interface IAutoUpdateService
    {
        bool IsEnabled { get; set; }
        void Initialize();
        void Shutdown();
    }
}
