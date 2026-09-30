using System.Collections.Generic;

namespace GameDevCheatsheet.Models;

public class ComponentItem
{
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";
    public string Category { get; set; } = "";
    public string SubRoute { get; set; } = "";
    public string ScriptFilename { get; set; } = "";
    public string GDScript { get; set; } = "";
    public string CSharpScript { get; set; } = "";
    public string SceneTreeGuide { get; set; } = "";
    public List<string> SetupSteps { get; set; } = new();
}

public class ComponentCategory
{
    public string Name { get; set; } = "";
    public string Icon { get; set; } = "";
    public bool IsExpanded { get; set; } = true;
    public List<ComponentItem> Items { get; set; } = new();
}