using System.Text;
using ChangelogGenerator.Models;

namespace ChangelogGenerator.Services;

public class ChangelogService
{
    private readonly GitService _gitService;

    public ChangelogService(GitService gitService)
    {
        _gitService = gitService;
    }

    public async Task GenerateChangelogAsync(string outputPath = "Changelog.md")
    {
        var tags = _gitService.GetAllTags();
        var changelog = new StringBuilder();

        // 添加标题
        changelog.AppendLine("# Changelog");
        changelog.AppendLine();
        changelog.AppendLine("All notable changes to this project will be documented in this file.");
        changelog.AppendLine();

        if (tags.Count == 0)
        {
            changelog.AppendLine("No tags found in this repository.");
        }
        else
        {
            // 处理未发布的更改（如果有的话）
            await AddUnreleasedChangesAsync(changelog, tags.First().Name);

            // 处理每个标签之间的更改
            for (int i = 0; i < tags.Count; i++)
            {
                var currentTag = tags[i];
                var previousTag = i < tags.Count - 1 ? tags[i + 1] : null;

                await AddTagSectionAsync(changelog, currentTag, previousTag);
            }
        }

        await File.WriteAllTextAsync(outputPath, changelog.ToString());
        Console.WriteLine($"Changelog generated successfully: {outputPath}");
    }

    private Task AddUnreleasedChangesAsync(StringBuilder changelog, string lastTagName)
    {
        try
        {
            var unreleasedCommits = _gitService.GetCommitsSinceLastTag(lastTagName);

            if (unreleasedCommits.Any())
            {
                changelog.AppendLine("## [Unreleased]");
                changelog.AppendLine();

                foreach (var commit in unreleasedCommits)
                {
                    changelog.AppendLine($"- {commit.Message} ([{commit.ShortSha}](../../commit/{commit.Sha}))");
                }

                changelog.AppendLine();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Could not get unreleased changes: {ex.Message}");
        }

        return Task.CompletedTask;
    }

    private Task AddTagSectionAsync(StringBuilder changelog, GitTag currentTag, GitTag? previousTag)
    {
        changelog.AppendLine($"## [{currentTag.Name}] - {currentTag.Date:yyyy-MM-dd}");
        changelog.AppendLine();

        if (!string.IsNullOrEmpty(currentTag.Message))
        {
            changelog.AppendLine($"**Release Notes:** {currentTag.Message}");
            changelog.AppendLine();
        }

        try
        {
            var commits = _gitService.GetCommitsBetweenTags(previousTag?.Name, currentTag.Name);

            if (commits.Any())
            {
                // 按类型分组提交（可选的增强功能）
                var groupedCommits = GroupCommitsByType(commits);

                foreach (var group in groupedCommits)
                {
                    if (group.Key != "Other")
                    {
                        changelog.AppendLine($"### {group.Key}");
                        changelog.AppendLine();
                    }

                    foreach (var commit in group.Value)
                    {
                        changelog.AppendLine($"- {commit.Message} ([{commit.ShortSha}](../../commit/{commit.Sha}))");
                    }

                    changelog.AppendLine();
                }
            }
            else
            {
                changelog.AppendLine("No changes in this release.");
                changelog.AppendLine();
            }
        }
        catch (Exception ex)
        {
            changelog.AppendLine($"Error retrieving commits for this tag: {ex.Message}");
            changelog.AppendLine();
        }

        return Task.CompletedTask;
    }

    private Dictionary<string, List<CommitInfo>> GroupCommitsByType(List<CommitInfo> commits)
    {
        var groups = new Dictionary<string, List<CommitInfo>>
        {
            ["Features"] = new(),
            ["Bug Fixes"] = new(),
            ["Documentation"] = new(),
            ["Other"] = new()
        };

        foreach (var commit in commits)
        {
            var message = commit.Message.ToLowerInvariant();

            if (message.StartsWith("feat") || message.Contains("feature") || message.Contains("add"))
            {
                groups["Features"].Add(commit);
            }
            else if (message.StartsWith("fix") || message.Contains("bug") || message.Contains("error"))
            {
                groups["Bug Fixes"].Add(commit);
            }
            else if (message.StartsWith("docs") || message.Contains("documentation") || message.Contains("readme"))
            {
                groups["Documentation"].Add(commit);
            }
            else
            {
                groups["Other"].Add(commit);
            }
        }

        // 移除空组
        return groups.Where(g => g.Value.Any()).ToDictionary(g => g.Key, g => g.Value);
    }
}