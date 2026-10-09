using System.Collections.Generic;

namespace GameDevCheatsheet.Models;

public class ComponentCategory
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsExpanded { get; set; } = true;

    // Navigation property
    public List<ComponentItem> Items { get; set; } = new();
}