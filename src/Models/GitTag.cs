namespace ChangelogGenerator.Models;

public class GitTag
{
    public string Name { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Sha { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}