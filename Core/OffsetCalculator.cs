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

        // Вычисляет расстояние от элемента до низа стены-основы
        public double? CalculateDistanceToWallBase(FamilyInstance familyInstance, int roundingStepMm)
        {
            try
            {
                // Получаем стену-основу
                Wall hostWall = familyInstance.Host as Wall;
                if (hostWall == null)
                    return null;

                // Получаем отметку низа стены
                double wallBaseElevationMm = GetWallBaseElevation(hostWall);

                // Получаем отметку расположения элемента
                double? elementOffsetMm = CalculateOffset(familyInstance, roundingStepMm);
                if (!elementOffsetMm.HasValue)
                    return null;

                // Вычисляем расстояние
                double distanceToWallBaseMm = elementOffsetMm.Value - wallBaseElevationMm;

                // Округляем согласно заданному шагу
                double roundedDistanceMm = Math.Round(distanceToWallBaseMm / roundingStepMm) * roundingStepMm;

                return roundedDistanceMm;
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

        // Получает отметку низа стены в миллиметрах
        private double GetWallBaseElevation(Wall wall)
        {
            try
            {
                // Получаем уровень стены
                ElementId levelId = wall.get_Parameter(BuiltInParameter.WALL_BASE_CONSTRAINT)?.AsElementId();
                if (levelId == null || levelId == ElementId.InvalidElementId)
                    return 0.0;

                Level level = _document.GetElement(levelId) as Level;
                if (level == null)
                    return 0.0;

                // Получаем смещение низа стены от уровня
                Parameter baseOffsetParam = wall.get_Parameter(BuiltInParameter.WALL_BASE_OFFSET);
                double baseOffsetFeet = baseOffsetParam?.AsDouble() ?? 0.0;

                // Вычисляем итоговую отметку низа стены
                double wallBaseElevationFeet = level.Elevation + baseOffsetFeet;
                return UnitUtils.ConvertFromInternalUnits(wallBaseElevationFeet, UnitTypeId.Millimeters);
            }
            catch
            {
                return 0.0;
            }
        }
        #endregion
    }
}
