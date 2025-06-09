using CommandLine;
using ChangelogGenerator.Services;

namespace ChangelogGenerator;

public class Options
{
    [Option("output", Required = false, Default = "CHANGELOG.md", HelpText = "Output file path")]
    public string Output { get; set; } = "CHANGELOG.md";

    [Option("repository", Required = false, Default = ".", HelpText = "Repository path")]
    public string Repository { get; set; } = ".";
}

class Program
{
    static async Task<int> Main(string[] args)
    {
        return await Parser.Default.ParseArguments<Options>(args)
                           .MapResult(
                               async (Options opts) => await RunAsync(opts),
                               errs => Task.FromResult(1)
                           );
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