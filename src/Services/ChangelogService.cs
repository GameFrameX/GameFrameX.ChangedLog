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
            // 只处理每个标签之间的更改，不处理未发布的内容
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

    private Task AddTagSectionAsync(StringBuilder changelog, GitTag currentTag, GitTag? previousTag)
    {
        changelog.AppendLine($"## [{currentTag.Name}] - {currentTag.Date:yyyy-MM-dd}");
        changelog.AppendLine();

        // if (!string.IsNullOrEmpty(currentTag.Message))
        // {
        //     changelog.AppendLine($"**Release Notes:** {currentTag.Message}");
        //     changelog.AppendLine();
        // }

        try
        {
            var commits = _gitService.GetCommitsBetweenTags(previousTag?.Name, currentTag.Name);

            if (commits.Any())
            {
                // 按照[]中的内容分组提交
                var groupedCommits = GroupCommitsByBracketContent(commits);

                foreach (var group in groupedCommits)
                {
                    if (!string.IsNullOrEmpty(group.Key))
                    {
                        changelog.AppendLine($"### {group.Key}");
                        changelog.AppendLine();
                    }

                    // 合并相同消息内容的提交
                    var mergedCommits = MergeCommitsByMessage(group.Value);

                    foreach (var mergedCommit in mergedCommits)
                    {
                        changelog.AppendLine($"- {mergedCommit.Message} {mergedCommit.CommitLinks}");
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

    /// <summary>
    /// 根据提交消息中[]内的内容进行分组
    /// 例如: "[feat] 添加新功能" -> 分组到 "feat"
    /// </summary>
    /// <param name="commits">提交列表</param>
    /// <returns>按[]内容分组的提交字典</returns>
    private Dictionary<string, List<CommitInfo>> GroupCommitsByBracketContent(List<CommitInfo> commits)
    {
        var groups = new Dictionary<string, List<CommitInfo>>();

        foreach (var commit in commits)
        {
            var bracketContent = ExtractBracketContent(commit.Message);
            var groupKey = string.IsNullOrEmpty(bracketContent) ? "Other" : bracketContent;

            if (!groups.ContainsKey(groupKey))
            {
                groups[groupKey] = new List<CommitInfo>();
            }

            groups[groupKey].Add(commit);
        }

        // 按组名排序，但将"Other"放在最后
        var sortedGroups = groups
            .OrderBy(g => g.Key == "Other" ? "zzz" : g.Key)
            .ToDictionary(g => g.Key, g => g.Value);

        return sortedGroups;
    }

    /// <summary>
    /// 合并相同消息内容的提交记录
    /// </summary>
    /// <param name="commits">提交列表</param>
    /// <returns>合并后的提交信息</returns>
    private List<MergedCommitInfo> MergeCommitsByMessage(List<CommitInfo> commits)
    {
        var mergedCommits = new List<MergedCommitInfo>();
        var groupedByMessage = commits
            .GroupBy(c => FormatCommitMessage(c.Message))
            .ToList();

        foreach (var group in groupedByMessage)
        {
            var commitList = group.ToList();
            var commitLinks = string.Join(" ", commitList.Select(c => $"([{c.ShortSha}](../../commit/{c.Sha}))"));
            
            mergedCommits.Add(new MergedCommitInfo
            {
                Message = group.Key,
                CommitLinks = commitLinks,
                CommitCount = commitList.Count
            });
        }

        return mergedCommits;
    }

    /// <summary>
    /// 提取提交消息中[]内的内容
    /// 例如: "[feat] 添加新功能" -> "feat"
    /// </summary>
    /// <param name="message">提交消息</param>
    /// <returns>[]内的内容，如果没有则返回空字符串</returns>
    private string ExtractBracketContent(string message)
    {
        if (string.IsNullOrEmpty(message))
            return string.Empty;

        var openBracketIndex = message.IndexOf('[');
        var closeBracketIndex = message.IndexOf(']');

        if (openBracketIndex == -1 || closeBracketIndex == -1 || closeBracketIndex <= openBracketIndex)
            return string.Empty;

        return message.Substring(openBracketIndex + 1, closeBracketIndex - openBracketIndex - 1).Trim();
    }

    /// <summary>
    /// 格式化提交消息，移除]之后第一个空格之前的内容
    /// 例如: "[scope] message" -> "message"
    /// </summary>
    /// <param name="message">原始提交消息</param>
    /// <returns>格式化后的消息</returns>
    private string FormatCommitMessage(string message)
    {
        if (string.IsNullOrEmpty(message))
            return message;

        var closeBracketIndex = message.IndexOf(']');
        if (closeBracketIndex == -1)
            return message;

        // 找到]之后的第一个空格
        var searchStart = closeBracketIndex + 1;
        if (searchStart >= message.Length)
            return message;

        var firstSpaceIndex = message.IndexOf(' ', searchStart);
        if (firstSpaceIndex == -1)
            return message;

        // 返回第一个空格之后的内容，去掉前后空白
        return message.Substring(firstSpaceIndex + 1).Trim();
    }

    /// <summary>
    /// 合并后的提交信息
    /// </summary>
    private class MergedCommitInfo
    {
        public string Message { get; set; } = string.Empty;
        public string CommitLinks { get; set; } = string.Empty;
        public int CommitCount { get; set; }
    }
}