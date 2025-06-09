using CommandLine;
using ChangelogGenerator.Services;

namespace ChangelogGenerator;

public class Options
{
    [Option("output", Required = false, Default = "CHANGELOG.md", HelpText = "Output file path")]
    public string Output { get; set; } = "CHANGELOG.md";

    [Option("repository", Required = false, Default = "./repository", HelpText = "Repository path")]
    public string Repository { get; set; } = "./repository";
}

class Program
{
    static async Task<int> Main(string[] args)
    {
        // 优先使用环境变量，如果没有则使用命令行参数
        var options = GetOptionsFromEnvironmentOrArgs(args);

        return await RunAsync(options);
    }

    static Options GetOptionsFromEnvironmentOrArgs(string[] args)
    {
        // 从环境变量获取配置
        var repositoryPath = Environment.GetEnvironmentVariable("CHANGELOG_REPOSITORY_PATH") ?? ".";
        var outputPath = Environment.GetEnvironmentVariable("CHANGELOG_OUTPUT_PATH") ?? "CHANGELOG.md";

        // 如果有命令行参数，则解析命令行参数
        if (args.Length > 0)
        {
            var result = Parser.Default.ParseArguments<Options>(args);
            if (result is Parsed<Options> parsed)
            {
                if (!Directory.Exists(parsed.Value.Repository))
                {
                    Directory.CreateDirectory(parsed.Value.Repository);
                }

                return parsed.Value;
            }
        }

        if (!Directory.Exists(repositoryPath))
        {
            Directory.CreateDirectory(repositoryPath);
        }

        // 使用环境变量创建Options
        return new Options
        {
            Repository = repositoryPath,
            Output = outputPath
        };
    }

    static async Task<int> RunAsync(Options options)
    {
        try
        {
            Console.WriteLine("Starting changelog generation...");
            Console.WriteLine($"Repository: {Path.GetFullPath(options.Repository)}");
            Console.WriteLine($"Output file: {Path.GetFullPath(options.Output)}");

            var gitService = new GitService(options.Repository);
            var changelogService = new ChangelogService(gitService);

            await changelogService.GenerateChangelogAsync(options.Output);

            Console.WriteLine("Changelog generation completed successfully!");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            Console.Error.WriteLine($"Stack trace: {ex.StackTrace}");
            return 1;
        }
    }
}