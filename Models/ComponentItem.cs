using System.Collections.Generic;

namespace GameDevCheatsheet.Models;

public class ComponentItem
{
    // --- Database Entity Fields (for Class Diagram / EF Core) ---
    public int ComponentId { get; set; }
    public int CategoryId { get; set; }
    public ComponentCategory? CategoryNavigation { get; set; }

    // --- Cheatsheet Domain / UI Fields (Resolves the 91 build errors) ---
    public string Id { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string SubRoute { get; set; } = string.Empty;
    public string ScriptFilename { get; set; } = string.Empty;
    public string GDScript { get; set; } = string.Empty;
    public string CSharpScript { get; set; } = string.Empty;
    public string SceneTreeGuide { get; set; } = string.Empty;
    public List<string> SetupSteps { get; set; } = new();
}