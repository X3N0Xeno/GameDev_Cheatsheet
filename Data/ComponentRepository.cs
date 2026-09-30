using System.Collections.Generic;
using GameDevCheatsheet.Models;
using GameDevCheatsheet.Data.Categories;

namespace GameDevCheatsheet.Data;

public static class ComponentRepository
{
    public static List<ComponentCategory> GetCategories()
    {
        return new List<ComponentCategory>
        {
            UiUxCategory.GetCategory(),
            MenusCategory.GetCategory(),
            MovementCategory.GetCategory(),
            CombatCategory.GetCategory(),
            SystemsCategory.GetCategory()
        };
    }
}