using System;

namespace GameDevCheatsheet.Models;

public class UserBookmark
{
    public int Id { get; set; }
    public string ComponentItemId { get; set; } = string.Empty;
    public ComponentItem? ComponentItem { get; set; }
    public DateTime PinnedAt { get; set; } = DateTime.UtcNow;
}