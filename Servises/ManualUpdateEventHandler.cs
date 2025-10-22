using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using LevelOffsetUpdater.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Settings = LevelOffsetUpdater.Core.Settings;

namespace LevelOffsetUpdater.Services
{
    // Обработчик ручного обновления элементов
    public class ManualUpdateEventHandler : IManualUpdateService
    {
        #region Fields
        private readonly IElementFilter _elementFilter;
        private ExternalEvent _externalEvent;
        private bool _updateOffsetOnly;
        private bool _updateWallDistanceOnly;
        #endregion

        #region Constructor
        public ManualUpdateEventHandler(IElementFilter elementFilter)
        {
            _elementFilter = elementFilter ?? throw new ArgumentNullException(nameof(elementFilter));
            _externalEvent = ExternalEvent.Create(this);
        }
        #endregion

        #region IManualUpdateService Implementation
        public void RaiseOffsetUpdate()
        {
            _updateOffsetOnly = true;
            _updateWallDistanceOnly = false;
            _externalEvent.Raise();
        }

        public void RaiseWallDistanceUpdate()
        {
            _updateOffsetOnly = false;
            _updateWallDistanceOnly = true;
            _externalEvent.Raise();
        }

        public void Raise()
        {
            _updateOffsetOnly = false;
            _updateWallDistanceOnly = false;
            _externalEvent.Raise();
        }
        #endregion

        #region IExternalEventHandler Implementation
        public void Execute(UIApplication app)
        {
            Document doc = app.ActiveUIDocument?.Document;
            if (doc == null) return;

            try
            {
                var settings = Settings.Load();

                if (!ValidateProjectParameters(doc, settings))
                {
                    return;
                }

                var elements = ElementUpdateService.GetTargetElements(doc, _elementFilter);

                if (elements.Count > settings.WarningThreshold)
                {
                    string warningMessage = string.Format(Constants.LARGE_UPDATE_WARNING_MESSAGE, elements.Count);
                    var result = MessageBox.Show(warningMessage, "Предупреждение",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                    if (result != DialogResult.Yes)
                        return;
                }

                var updateResult = UpdateElements(doc, elements, settings);

                ShowUpdateResults(updateResult);
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
        private bool ValidateProjectParameters(Document doc, Settings settings)
        {
            List<string> errors = new List<string>();

            // Проверяем параметр отметки расположения
            if (!_updateWallDistanceOnly)
            {
                var offsetValidation = ParameterValidator.ValidateParameter(doc, Constants.TARGET_PARAMETER_NAME);
                if (!offsetValidation.IsValid)
                {
                    errors.Add(offsetValidation.ErrorMessage);
                    settings.AutoUpdateEnabled = false;
                }
            }

            // Проверяем параметр расстояния до низа стены
            if (!_updateOffsetOnly)
            {
                var wallDistanceValidation = ParameterValidator.ValidateParameter(doc, Constants.WALL_DISTANCE_PARAMETER_NAME);
                if (!wallDistanceValidation.IsValid)
                {
                    errors.Add(wallDistanceValidation.ErrorMessage);
                    settings.AutoUpdateWallDistanceEnabled = false;
                }
            }

            if (errors.Count > 0)
            {
                string errorMessage = "Ошибка обновления\n\n" +
                    string.Join("\n\n", errors) + "\n\n" +
                    "Автоматическое обновление соответствующих параметров выключено.";

                TaskDialog.Show("Ошибка обновления", errorMessage);
                settings.Save();

                return false;
            }

            return true;
        }

        private UpdateResult UpdateElements(Document doc, List<FamilyInstance> elements, Settings settings)
        {
            var result = new UpdateResult();
            var offsetErrors = new List<ElementId>();
            var wallDistanceErrors = new List<ElementId>();

            using (Transaction trans = new Transaction(doc, "Ручное обновление параметров"))
            {
                trans.Start();

                var calculator = new OffsetCalculator(doc);

                foreach (var element in elements)
                {
                    result.ProcessedCount++;

                    bool offsetUpdated = false;
                    bool wallDistanceUpdated = false;

                    // Обновляем отметку расположения (если не режим только расстояния)
                    if (!_updateWallDistanceOnly)
                    {
                        try
                        {
                            offsetUpdated = ElementUpdateService.UpdateElementOffset(element, calculator, settings);
                            if (!offsetUpdated)
                            {
                                offsetErrors.Add(element.Id);
                            }
                        }
                        catch
                        {
                            offsetErrors.Add(element.Id);
                        }
                    }

                    // Обновляем расстояние до низа стены (если не режим только отметок)
                    if (!_updateOffsetOnly)
                    {
                        try
                        {
                            wallDistanceUpdated = ElementUpdateService.UpdateWallDistance(element, calculator, settings);
                            if (!wallDistanceUpdated)
                            {
                                wallDistanceErrors.Add(element.Id);
                            }
                        }
                        catch
                        {
                            wallDistanceErrors.Add(element.Id);
                        }
                    }

                    if (offsetUpdated || wallDistanceUpdated)
                    {
                        result.UpdatedCount++;
                    }
                }

                trans.Commit();
            }

            result.OffsetErrors = offsetErrors;
            result.WallDistanceErrors = wallDistanceErrors;
            result.ErrorCount = offsetErrors.Count + wallDistanceErrors.Count;

            if (result.ErrorCount > 0)
            {
                result.Message = $"Обработано: {result.ProcessedCount}, обновлено: {result.UpdatedCount}, ошибок: {result.ErrorCount}";
            }

            return result;
        }

        private void ShowUpdateResults(UpdateResult result)
        {
            if (result.ErrorCount == 0)
            {
                string message = string.Format(Constants.UPDATE_COMPLETED_MESSAGE, result.UpdatedCount);
                TaskDialog.Show("Результат обновления", message);
                return;
            }

            // Формируем сообщение с группировкой ошибок по параметрам
            StringBuilder errorMessage = new StringBuilder();
            errorMessage.AppendLine($"Обработано элементов: {result.ProcessedCount}");
            errorMessage.AppendLine($"Успешно обновлено: {result.UpdatedCount}");
            errorMessage.AppendLine($"Ошибок: {result.ErrorCount}");
            errorMessage.AppendLine();

            if (result.OffsetErrors.Count > 0)
            {
                errorMessage.AppendLine($"Не удалось обновить параметр '{Constants.TARGET_PARAMETER_NAME}':");
                errorMessage.AppendLine(string.Join(", ", result.OffsetErrors.Select(id => id.IntegerValue)));
                errorMessage.AppendLine();
            }

            if (result.WallDistanceErrors.Count > 0)
            {
                errorMessage.AppendLine($"Не удалось обновить параметр '{Constants.WALL_DISTANCE_PARAMETER_NAME}':");
                errorMessage.AppendLine(string.Join(", ", result.WallDistanceErrors.Select(id => id.IntegerValue)));
            }

            // Показываем окно с TextBox для длинного списка ID
            var errorForm = new System.Windows.Forms.Form
            {
                Text = "Результат обновления с ошибками",
                Width = 600,
                Height = 400,
                StartPosition = FormStartPosition.CenterScreen
            };

            var textBox = new System.Windows.Forms.TextBox
            {
                Multiline = true,
                ScrollBars = ScrollBars.Both,
                Dock = DockStyle.Fill,
                Text = errorMessage.ToString(),
                ReadOnly = true,
                Font = new System.Drawing.Font("Consolas", 9)
            };

            errorForm.Controls.Add(textBox);
            errorForm.ShowDialog();
        }
        #endregion
    }
}
