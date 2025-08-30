using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using LevelOffsetUpdater.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;
using System.Xml.Linq;
using Settings = LevelOffsetUpdater.Core.Settings;

namespace LevelOffsetUpdater.Services
{
    // Обработчик ручного обновления всех элементов
    public class ManualUpdateEventHandler : IManualUpdateService
    {
        #region Fields
        private readonly IElementFilter _elementFilter;
        private ExternalEvent _externalEvent;
        #endregion

        #region Constructor
        public ManualUpdateEventHandler(IElementFilter elementFilter)
        {
            _elementFilter = elementFilter ?? throw new ArgumentNullException(nameof(elementFilter));
            _externalEvent = ExternalEvent.Create(this);
        }
        #endregion

        #region IManualUpdateService Implementation
        // Запускает ручное обновление
        public void Raise()
        {
            _externalEvent.Raise();
        }
        #endregion

        #region IExternalEventHandler Implementation
        // Выполняет ручное обновление всех элементов
        public void Execute(UIApplication app)
        {
            Document doc = app.ActiveUIDocument?.Document;
            if (doc == null) return;

            try
            {
                var settings = Settings.Load();
                var elements = GetTargetElements(doc);

                // Проверяем количество элементов и показываем предупреждение при необходимости
                if (elements.Count > settings.WarningThreshold)
                {
                    string warningMessage = string.Format(Constants.LARGE_UPDATE_WARNING_MESSAGE, elements.Count);
                    var result = MessageBox.Show(warningMessage, "Предупреждение",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                    if (result != DialogResult.Yes)
                        return;
                }

                var updateResult = UpdateAllElements(doc, elements, settings);

                string message = updateResult.Success
                    ? string.Format(Constants.UPDATE_COMPLETED_MESSAGE, updateResult.UpdatedCount)
                    : string.Format(Constants.UPDATE_ERROR_MESSAGE, updateResult.Message);

                TaskDialog.Show("Результат обновления", message);
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Ошибка", string.Format(Constants.UPDATE_ERROR_MESSAGE, ex.Message));
            }
        }

        public string GetName()
        {
            return "ManualUpdateEventHandler";
        }
        #endregion

        #region Private Methods
        // Получает все целевые элементы в документе
        private List<FamilyInstance> GetTargetElements(Document doc)
        {
            var elements = new List<FamilyInstance>();

            foreach (var category in Constants.TARGET_CATEGORIES)
            {
                var collector = new FilteredElementCollector(doc)
                    .OfCategory(category)
                    .WhereElementIsNotElementType()
                    .Cast<FamilyInstance>()
                    .Where(fi => _elementFilter.IsValidElement(fi));

                elements.AddRange(collector);
            }

            return elements;
        }

        // Обновляет все элементы в одной транзакции
        private UpdateResult UpdateAllElements(Document doc, List<FamilyInstance> elements, Settings settings)
        {
            var result = new UpdateResult();

            using (Transaction trans = new Transaction(doc, "Ручное обновление отметок"))
            {
                trans.Start();

                var calculator = new OffsetCalculator(doc);

                foreach (var element in elements)
                {
                    result.ProcessedCount++;

                    try
                    {
                        if (UpdateElementOffset(element, calculator, settings))
                        {
                            result.UpdatedCount++;
                        }
                    }
                    catch
                    {
                        result.ErrorCount++;
                    }
                }

                trans.Commit();
            }

            if (result.ErrorCount > 0)
            {
                result.Message = $"Обработано: {result.ProcessedCount}, обновлено: {result.UpdatedCount}, ошибок: {result.ErrorCount}";
            }

            return result;
        }

        // Обновляет отметку расположения для элемента
        private bool UpdateElementOffset(FamilyInstance familyInstance, OffsetCalculator calculator, Settings settings)
        {
            // Проверяем наличие и доступность параметра
            Parameter targetParam = familyInstance.LookupParameter(Constants.TARGET_PARAMETER_NAME);
            if (targetParam == null || targetParam.IsReadOnly)
                return false;

            // Вычисляем новое значение
            double? newOffsetMm = calculator.CalculateOffset(familyInstance, settings.RoundingStepMm);
            if (!newOffsetMm.HasValue)
                return false;

            // Устанавливаем новое значение
            if (targetParam.StorageType == StorageType.Double)
            {
                double newValueFeet = UnitUtils.ConvertToInternalUnits(newOffsetMm.Value, UnitTypeId.Millimeters);
                targetParam.Set(newValueFeet);
            }
            else if (targetParam.StorageType == StorageType.Integer)
            {
                targetParam.Set((int)Math.Round(newOffsetMm.Value));
            }

            return true;
        }
        #endregion
    }
}
