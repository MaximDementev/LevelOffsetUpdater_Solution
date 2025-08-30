using Autodesk.Revit.DB;
using System;
using System.Reflection.Emit;
using System.Xml.Linq;

namespace LevelOffsetUpdater.Core
{
    // Класс для вычисления отметки расположения элементов
    public class OffsetCalculator
    {
        #region Fields
        private readonly Document _document;
        #endregion

        #region Constructor
        public OffsetCalculator(Document document)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
        }
        #endregion

        #region Public Methods
        // Вычисляет отметку расположения для элемента
        public double? CalculateOffset(FamilyInstance familyInstance, int roundingStepMm)
        {
            try
            {
                // Получаем уровень элемента
                Level level = GetElementLevel(familyInstance);
                if (level == null)
                    return null;

                // Получаем расстояние уровня до базовой точки проекта в футах
                double levelElevationFeet = level.Elevation;

                // Конвертируем в миллиметры
                double levelElevationMm = UnitUtils.ConvertFromInternalUnits(levelElevationFeet, UnitTypeId.Millimeters);

                // Получаем параметр INSTANCE_SILL_HEIGHT_PARAM
                double sillHeightMm = GetSillHeight(familyInstance);

                // Вычисляем итоговое значение
                double totalOffsetMm = levelElevationMm + sillHeightMm;

                // Округляем согласно заданному шагу
                double roundedOffsetMm = Math.Round(totalOffsetMm / roundingStepMm) * roundingStepMm;

                return roundedOffsetMm;
            }
            catch
            {
                return null;
            }
        }
        #endregion

        #region Private Methods
        // Получает уровень элемента
        private Level GetElementLevel(FamilyInstance familyInstance)
        {
            try
            {
                Parameter levelParam = familyInstance.get_Parameter(BuiltInParameter.FAMILY_LEVEL_PARAM);
                if (levelParam != null && levelParam.HasValue)
                {
                    ElementId levelId = levelParam.AsElementId();
                    return _document.GetElement(levelId) as Level;
                }

                // Альтернативный способ получения уровня
                return familyInstance.Host as Level ??
                       _document.GetElement(familyInstance.LevelId) as Level;
            }
            catch
            {
                return null;
            }
        }

        // Получает высоту подоконника в миллиметрах
        private double GetSillHeight(FamilyInstance familyInstance)
        {
            try
            {
                Parameter sillParam = familyInstance.get_Parameter(BuiltInParameter.INSTANCE_SILL_HEIGHT_PARAM);
                if (sillParam != null && sillParam.HasValue)
                {
                    double sillHeightFeet = sillParam.AsDouble();
                    return UnitUtils.ConvertFromInternalUnits(sillHeightFeet, UnitTypeId.Millimeters);
                }
            }
            catch
            {
                // Игнорируем ошибки получения параметра
            }
            return 0.0;
        }
        #endregion
    }
}
