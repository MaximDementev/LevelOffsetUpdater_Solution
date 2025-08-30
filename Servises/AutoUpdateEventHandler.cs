using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using LevelOffsetUpdater.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using static System.Net.Mime.MediaTypeNames;
using System.Xml.Linq;
using Settings = LevelOffsetUpdater.Core.Settings;

namespace LevelOffsetUpdater.Services
{
    // Обработчик автоматического обновления отметок при изменении элементов
    public class AutoUpdateEventHandler : IAutoUpdateService, IExternalEventHandler
    {
        #region Fields
        private readonly ControlledApplication _application;
        private readonly IElementFilter _elementFilter;
        private readonly ExternalEvent _externalEvent;
        private readonly HashSet<ElementId> _elementsToProcess = new HashSet<ElementId>();
        private Document _currentDocument;
        private bool _isEnabled;
        private Settings _settings;
        #endregion

        #region Properties
        public bool IsEnabled
        {
            get => _isEnabled;
            set => _isEnabled = value;
        }
        #endregion

        #region Constructor
        public AutoUpdateEventHandler(ControlledApplication application, IElementFilter elementFilter)
        {
            _application = application ?? throw new ArgumentNullException(nameof(application));
            _elementFilter = elementFilter ?? throw new ArgumentNullException(nameof(elementFilter));
            _externalEvent = ExternalEvent.Create(this);
            _settings = Settings.Load();
            _isEnabled = _settings.AutoUpdateEnabled;
        }
        #endregion

        #region IAutoUpdateService Implementation
        // Инициализирует сервис автоматического обновления
        public void Initialize()
        {
            _application.DocumentChanged += OnDocumentChanged;
        }

        // Завершает работу сервиса
        public void Shutdown()
        {
            _application.DocumentChanged -= OnDocumentChanged;
        }
        #endregion

        #region Event Handlers
        // Обрабатывает изменения в документе
        private void OnDocumentChanged(object sender, DocumentChangedEventArgs e)
        {
            if (!_isEnabled) return;

            try
            {
                _currentDocument = e.GetDocument();
                var addedElements = e.GetAddedElementIds();
                var modifiedElements = e.GetModifiedElementIds();

                bool hasElementsToProcess = false;

                // Проверяем добавленные элементы
                foreach (var elementId in addedElements)
                {
                    var element = _currentDocument.GetElement(elementId);
                    if (_elementFilter.IsValidElement(element))
                    {
                        _elementsToProcess.Add(elementId);
                        hasElementsToProcess = true;
                    }
                }

                // Проверяем измененные элементы
                foreach (var elementId in modifiedElements)
                {
                    var element = _currentDocument.GetElement(elementId);
                    if (_elementFilter.IsValidElement(element))
                    {
                        _elementsToProcess.Add(elementId);
                        hasElementsToProcess = true;
                    }
                }

                if (hasElementsToProcess)
                {
                    _externalEvent.Raise();
                }
            }
            catch
            {
                // Игнорируем ошибки в обработчике события
            }
        }
        #endregion

        #region IExternalEventHandler Implementation
        // Выполняет обновление элементов в безопасном контексте
        public void Execute(UIApplication app)
        {
            if (_currentDocument == null || !_elementsToProcess.Any())
                return;

            try
            {
                using (Transaction trans = new Transaction(_currentDocument, "Автоматическое обновление отметок"))
                {
                    trans.Start();

                    var elementsToUpdate = new List<ElementId>(_elementsToProcess);
                    _elementsToProcess.Clear();

                    var calculator = new OffsetCalculator(_currentDocument);
                    _settings = Settings.Load(); // Обновляем настройки

                    foreach (var elementId in elementsToUpdate)
                    {
                        var element = _currentDocument.GetElement(elementId);
                        if (element is FamilyInstance familyInstance && _elementFilter.IsValidElement(familyInstance))
                        {
                            if (!UpdateElementOffset(familyInstance, calculator))
                            {
                                // Если обновление не удалось из-за проблем с параметром, отключаем автообновление
                                _isEnabled = false;
                                _settings.AutoUpdateEnabled = false;
                                _settings.Save();
                                break;
                            }
                        }
                    }

                    trans.Commit();
                }
            }
            catch
            {
                // Игнорируем ошибки транзакции
            }
        }

        public string GetName()
        {
            return "AutoUpdateEventHandler";
        }
        #endregion

        #region Private Methods
        // Обновляет отметку расположения для элемента
        private bool UpdateElementOffset(FamilyInstance familyInstance, OffsetCalculator calculator)
        {
            try
            {
                // Проверяем наличие и доступность параметра
                Parameter targetParam = familyInstance.LookupParameter(Constants.TARGET_PARAMETER_NAME);
                if (targetParam == null)
                {
                    TaskDialog.Show("Предупреждение",
                        string.Format(Constants.PARAMETER_NOT_FOUND_MESSAGE, Constants.TARGET_PARAMETER_NAME));
                    return false;
                }

                if (targetParam.IsReadOnly)
                {
                    TaskDialog.Show("Предупреждение",
                        string.Format(Constants.PARAMETER_READ_ONLY_MESSAGE, Constants.TARGET_PARAMETER_NAME));
                    return false;
                }

                // Вычисляем новое значение
                double? newOffsetMm = calculator.CalculateOffset(familyInstance, _settings.RoundingStepMm);
                if (!newOffsetMm.HasValue)
                    return true; // Не удалось вычислить, но это не критическая ошибка

                // Получаем текущее значение параметра
                double currentValueMm = 0;
                if (targetParam.HasValue)
                {
                    if (targetParam.StorageType == StorageType.Double)
                    {
                        double currentValueFeet = targetParam.AsDouble();
                        currentValueMm = UnitUtils.ConvertFromInternalUnits(currentValueFeet, UnitTypeId.Millimeters);
                    }
                    else if (targetParam.StorageType == StorageType.Integer)
                    {
                        currentValueMm = targetParam.AsInteger();
                    }
                }

                // Обновляем только если значение изменилось
                if (Math.Abs(currentValueMm - newOffsetMm.Value) > 0.1) // Допуск 0.1 мм
                {
                    if (targetParam.StorageType == StorageType.Double)
                    {
                        double newValueFeet = UnitUtils.ConvertToInternalUnits(newOffsetMm.Value, UnitTypeId.Millimeters);
                        targetParam.Set(newValueFeet);
                    }
                    else if (targetParam.StorageType == StorageType.Integer)
                    {
                        targetParam.Set((int)Math.Round(newOffsetMm.Value));
                    }
                }

                return true;
            }
            catch
            {
                return true; // Не критическая ошибка для отдельного элемента
            }
        }
        #endregion
    }
}
