using Autodesk.Revit.DB;
using System.Linq;
using System.Xml.Linq;

namespace LevelOffsetUpdater.Core
{
    // Фильтр для определения дверей и окон, подлежащих обновлению
    public class DoorWindowFilter : IElementFilter
    {
        #region IElementFilter Implementation
        // Проверяет, является ли элемент дверью или окном
        public bool IsValidElement(Element element)
        {
            if (element == null || !element.IsValidObject)
                return false;

            if (!(element is FamilyInstance familyInstance))
                return false;

            var category = element.Category;
            if (category == null)
                return false;

            return Constants.TARGET_CATEGORIES.Contains((BuiltInCategory)category.Id.IntegerValue);
        }
        #endregion
    }
}
