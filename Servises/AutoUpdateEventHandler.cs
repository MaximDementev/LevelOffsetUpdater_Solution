using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using LevelOffsetUpdater.Core;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private bool _offsetParameterValid = false;
        private bool _wallDistanceParameterValid = false;
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
            _isEnabled = _settings.AutoUpdateEnabled || _settings.AutoUpdateWallDistanceEnabled;
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
        private void OnDocumentChanged(object sender, DocumentChangedEventArgs e)
        {
            _settings = Settings.Load();

            if (!_settings.AutoUpdateEnabled && !_settings.AutoUpdateWallDistanceEnabled) return;

            try
            {
                _currentDocument = e.GetDocument();
                var addedElements = e.GetAddedElementIds();
                var modifiedElements = e.GetModifiedElementIds();

                bool hasElementsToProcess = false;

                foreach (var elementId in addedElements)
                {
                    var element = _currentDocument.GetElement(elementId);
                    if (_elementFilter.IsValidElement(element))
                    {
                        _elementsToProcess.Add(elementId);
                        hasElementsToProcess = true;
                    }
                }

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
        public void Execute(UIApplication app)
        {
            if (_currentDocument == null || !_elementsToProcess.Any())
                return;

            try
            {
                _settings = Settings.Load();

                ValidateParameters(_currentDocument, _settings);

                if (!_offsetParameterValid && !_wallDistanceParameterValid)
                {
                    _elementsToProcess.Clear();
                    return;
                }

                using (Transaction trans = new Transaction(_currentDocument, "Автоматическое обновление параметров"))
                {
                    trans.Start();

                    var elementsToUpdate = new List<ElementId>(_elementsToProcess);
                    _elementsToProcess.Clear();

                    var calculator = new OffsetCalculator(_currentDocument);

                    foreach (var elementId in elementsToUpdate)
                    {
                        var element = _currentDocument.GetElement(elementId);
                        if (element is FamilyInstance familyInstance && _elementFilter.IsValidElement(familyInstance))
                        {
                            if (_settings.AutoUpdateEnabled && _offsetParameterValid)
                            {
                                ElementUpdateService.UpdateElementOffset(familyInstance, calculator, _settings);
                            }

                            if (_settings.AutoUpdateWallDistanceEnabled && _wallDistanceParameterValid)
                            {
                                ElementUpdateService.UpdateWallDistance(familyInstance, calculator, _settings);
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
        private void ValidateParameters(Document doc, Settings settings)
        {
            List<string> errors = new List<string>();

            _offsetParameterValid = false;
            _wallDistanceParameterValid = false;

            if (settings.AutoUpdateEnabled)
            {
                var offsetValidation = ParameterValidator.ValidateParameter(doc, Constants.TARGET_PARAMETER_NAME);
                if (offsetValidation.IsValid)
                {
                    _offsetParameterValid = true;
                }
                else
                {
                    errors.Add($"Параметр '{Constants.TARGET_PARAMETER_NAME}':\n{offsetValidation.ErrorMessage}");
                    settings.AutoUpdateEnabled = false;
                }
            }

            if (settings.AutoUpdateWallDistanceEnabled)
            {
                var wallDistanceValidation = ParameterValidator.ValidateParameter(doc, Constants.WALL_DISTANCE_PARAMETER_NAME);
                if (wallDistanceValidation.IsValid)
                {
                    _wallDistanceParameterValid = true;
                }
                else
                {
                    errors.Add($"Параметр '{Constants.WALL_DISTANCE_PARAMETER_NAME}':\n{wallDistanceValidation.ErrorMessage}");
                    settings.AutoUpdateWallDistanceEnabled = false;
                }
            }

            if (errors.Count > 0)
            {
                string errorMessage = "Ошибка автоматического обновления\n\n" +
                    string.Join("\n\n", errors) + "\n\n" +
                    "Автоматическое обновление соответствующих параметров выключено.";

                TaskDialog.Show("Ошибка обновления", errorMessage);
                settings.Save();
            }
        }
        #endregion
    }
}
