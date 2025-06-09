using LibGit2Sharp;
using ChangelogGenerator.Models;

namespace ChangelogGenerator.Services;

public class GitService
{
    private readonly string _repositoryPath;
    
    public GitService(string repositoryPath = ".")
    {
        _repositoryPath = repositoryPath;
    }
    
    public List<GitTag> GetAllTags()
    {
        using var repo = new Repository(_repositoryPath);
        
        var tags = repo.Tags
            .Where(tag => tag.Target is Commit)
            .Select(tag => new GitTag
            {
                Name = tag.FriendlyName,
                Date = ((Commit)tag.Target).Author.When.DateTime,
                Sha = tag.Target.Sha,
                Message = tag.Annotation?.Message ?? string.Empty
            })
            .OrderByDescending(t => t.Date)
            .ToList();
            
        return tags;
    }
    
    public List<CommitInfo> GetCommitsBetweenTags(string? fromTag, string toTag)
    {
        using var repo = new Repository(_repositoryPath);
        
        var toCommit = repo.Tags[toTag]?.Target as Commit;
        if (toCommit == null)
        {
            throw new ArgumentException($"Tag '{toTag}' not found or is not a commit");
        }
        
        Commit? fromCommit = null;
        if (!string.IsNullOrEmpty(fromTag))
        {
            fromCommit = repo.Tags[fromTag]?.Target as Commit;
            if (fromCommit == null)
            {
                throw new ArgumentException($"Tag '{fromTag}' not found or is not a commit");
            }
        }
        
        var commits = new List<CommitInfo>();
        var commitFilter = new CommitFilter
        {
            IncludeReachableFrom = toCommit,
            ExcludeReachableFrom = fromCommit,
            SortBy = CommitSortStrategies.Topological | CommitSortStrategies.Time
        };
        
        foreach (var commit in repo.Commits.QueryBy(commitFilter))
        {
            // 跳过合并提交（可选）
            if (commit.Parents.Count() > 1)
                continue;
                
            commits.Add(new CommitInfo
            {
                Sha = commit.Sha,
                Message = commit.MessageShort,
                Author = commit.Author.Name,
                Date = commit.Author.When.DateTime
            });
        }
        
        return commits.OrderByDescending(c => c.Date).ToList();
    }
    
    public List<CommitInfo> GetCommitsSinceLastTag(string lastTag)
    {
        using var repo = new Repository(_repositoryPath);
        
        var lastCommit = repo.Tags[lastTag]?.Target as Commit;
        if (lastCommit == null)
        {
            throw new ArgumentException($"Tag '{lastTag}' not found or is not a commit");
        }
        
        var commits = new List<CommitInfo>();
        var commitFilter = new CommitFilter
        {
            IncludeReachableFrom = repo.Head.Tip,
            ExcludeReachableFrom = lastCommit,
            SortBy = CommitSortStrategies.Topological | CommitSortStrategies.Time
        };
        
        foreach (var commit in repo.Commits.QueryBy(commitFilter))
        {
            // 跳过合并提交（可选）
            if (commit.Parents.Count() > 1)
                continue;
                
            commits.Add(new CommitInfo
            {
                Sha = commit.Sha,
                Message = commit.MessageShort,
                Author = commit.Author.Name,
                Date = commit.Author.When.DateTime
            });
        }
        
        return commits.OrderByDescending(c => c.Date).ToList();
    }
}