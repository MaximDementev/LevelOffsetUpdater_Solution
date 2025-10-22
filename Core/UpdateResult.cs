using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace LevelOffsetUpdater.Core
{
    // Результат операции обновления элементов
    public class UpdateResult
    {
        #region Properties
        public bool Success { get; set; }
        public string Message { get; set; }
        public int ProcessedCount { get; set; }
        public int UpdatedCount { get; set; }
        public int ErrorCount { get; set; }
        public List<ElementId> OffsetErrors { get; set; }
        public List<ElementId> WallDistanceErrors { get; set; }
        #endregion

        #region Constructor
        public UpdateResult()
        {
            Success = true;
            Message = string.Empty;
            ProcessedCount = 0;
            UpdatedCount = 0;
            ErrorCount = 0;
            OffsetErrors = new List<ElementId>();
            WallDistanceErrors = new List<ElementId>();
        }
        #endregion
    }
}
