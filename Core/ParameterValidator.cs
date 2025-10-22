using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;

namespace LevelOffsetUpdater.Core
{
    // Валидатор параметров проекта
    public static class ParameterValidator
    {
        // Результат валидации параметра
        public class ValidationResult
        {
            public bool IsValid { get; set; }
            public string ErrorMessage { get; set; }
        }

        // Проверяет параметр на уровне проекта
        public static ValidationResult ValidateParameter(Document doc, string parameterName)
        {
            var result = new ValidationResult { IsValid = true };

            // Получаем определение параметра из проекта
            DefinitionBindingMapIterator iterator = doc.ParameterBindings.ForwardIterator();
            Definition paramDefinition = null;
            ElementBinding binding = null;

            while (iterator.MoveNext())
            {
                Definition def = iterator.Key;
                if (def.Name == parameterName)
                {
                    paramDefinition = def;
                    binding = iterator.Current as ElementBinding;
                    break;
                }
            }

            // Проверка 1: Параметр существует в проекте
            if (paramDefinition == null)
            {
                result.IsValid = false;
                result.ErrorMessage = $"Параметр '{parameterName}' не найден в проекте.\n" +
                    $"Обратиться в ОИМ";
                return result;
            }

            // Проверка 2: Параметр является параметром экземпляра
            if (binding is TypeBinding)
            {
                result.IsValid = false;
                result.ErrorMessage = $"Параметр '{parameterName}' является параметром типа, а должен быть параметром экземпляра.\n" +
                    $"Обратиться в ОИМ";
                return result;
            }

            // Проверка 3: Параметр применим к категориям Двери и Окна
            if (binding != null)
            {
                var categories = binding.Categories;
                bool hasDoors = false;
                bool hasWindows = false;

                foreach (Category cat in categories)
                {
                    if (cat.Id.IntegerValue == (int)BuiltInCategory.OST_Doors)
                        hasDoors = true;
                    if (cat.Id.IntegerValue == (int)BuiltInCategory.OST_Windows)
                        hasWindows = true;
                }

                if (!hasDoors || !hasWindows)
                {
                    result.IsValid = false;
                    result.ErrorMessage = $"Параметр '{parameterName}' не применим к категориям 'Двери' и 'Окна'.\n" +
                    $"Обратиться в ОИМ";
                    return result;
                }
            }

            //// Проверка 4: Параметр может изменяться по экземплярам группы
            //if (paramDefinition is InternalDefinition internalDef)
            //{
            //    if (!internalDef.VariesAcrossGroups)
            //    {
            //        result.IsValid = false;
            //        result.ErrorMessage = $"Параметр '{parameterName}' не может изменяться по экземплярам группы.";
            //        return result;
            //    }
            //}

            // выключено, потому что параметр отметки от нуля - это числовой параметр,
            // его нельзя сделать иземняющимся по экземплярам групп.
            // Нужно вместо него использовать тип парамтера - расстояние

            return result;
        }
    }
}
