using Autodesk.Revit.DB;
using LevelOffsetUpdater.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using Settings = LevelOffsetUpdater.Core.Settings;

namespace LevelOffsetUpdater.Services
{
    // Общий сервис для обновления параметров элементов
    public static class ElementUpdateService
    {
        // Обновляет отметку расположения для элемента
        public static bool UpdateElementOffset(FamilyInstance familyInstance, OffsetCalculator calculator, Settings settings)
        {
            try
            {
                Parameter targetParam = familyInstance.LookupParameter(Constants.TARGET_PARAMETER_NAME);
                if (targetParam == null || targetParam.IsReadOnly)
                    return false;

                double? newOffsetMm = calculator.CalculateOffset(familyInstance, settings.RoundingStepMm);
                if (!newOffsetMm.HasValue)
                    return false;

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
            catch
            {
                return false;
            }
        }

        // Обновляет расстояние до низа стены для элемента
        public static bool UpdateWallDistance(FamilyInstance familyInstance, OffsetCalculator calculator, Settings settings)
        {
            try
            {
                Parameter wallDistanceParam = familyInstance.LookupParameter(Constants.WALL_DISTANCE_PARAMETER_NAME);
                if (wallDistanceParam == null || wallDistanceParam.IsReadOnly)
                    return false;

                double? distanceMm = calculator.CalculateDistanceToWallBase(familyInstance, settings.RoundingStepMm);
                if (!distanceMm.HasValue)
                    return false;

                if (wallDistanceParam.StorageType == StorageType.Double)
                {
                    double valueFeet = UnitUtils.ConvertToInternalUnits(distanceMm.Value, UnitTypeId.Millimeters);
                    wallDistanceParam.Set(valueFeet);
                }
                else if (wallDistanceParam.StorageType == StorageType.Integer)
                {
                    wallDistanceParam.Set((int)Math.Round(distanceMm.Value));
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        // Получает все целевые элементы в документе
        public static List<FamilyInstance> GetTargetElements(Document doc, IElementFilter elementFilter)
        {
            var elements = new List<FamilyInstance>();

            foreach (var category in Constants.TARGET_CATEGORIES)
            {
                var collector = new FilteredElementCollector(doc)
                    .OfCategory(category)
                    .WhereElementIsNotElementType()
                    .Cast<FamilyInstance>()
                    .Where(fi => elementFilter.IsValidElement(fi));

                elements.AddRange(collector);
            }

            return elements;
        }
    }
}
