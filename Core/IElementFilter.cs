using Autodesk.Revit.DB;
using System.Xml.Linq;

namespace LevelOffsetUpdater.Core
{
    // Интерфейс для фильтрации элементов, подлежащих обновлению
    public interface IElementFilter
    {
        bool IsValidElement(Element element);
    }
}
