using SuperMetroid.TestSupport.WindowsConsole;

namespace SuperMetroid.FrameDefinitionsGenerator;

/// <summary>
/// Generates csharp/src/SuperMetroid.AssetExtraction/Enemies/Artwork/EnemyDefinitionFrameDefinitions.cs and
/// SpawnedObjectFrameDefinitions.cs (#1392, #1484, #1513) from SuperMetroid.Core's semantic call graph: the frames each
/// enemy definition's behaviour shows, the projectile frames and room sprite objects it spawns, and what code outside
/// every behaviour spawns, by the common spawner whose code spawns it. Run from the repository root; --check exits nonzero when a committed file differs.
/// </summary>
internal static class Program
{
    /// <summary>The command line this tool accepts.</summary>
    private const string Usage =
        "Usage (from the repository root): dotnet run --project tools/frame-definitions-generator -c Release -- [--check]";

    /// <summary>Generates or checks both files behind the no-dialog process boundary.</summary>
    private static async Task<int> Main(string[] args)
    {
        NativeConsoleErrors.DisableDialogs();
        try
        {
            bool check = args switch
            {
                [] => false,
                ["--check"] => true,
                _ => throw new ArgumentException(Usage),
            };
            string repository = Directory.GetCurrentDirectory();
            CoreSource core = await CoreSource.LoadAsync(repository);
            var model = new CoreModel(core);
            SpawnAnalysis analysis = SpawnAnalysis.Run(model);
            analysis.Report(Console.Out);
            GeneratedFile[] files =
            [
                EnemyDefinitionFramesFile.Create(repository, model, analysis),
                SpawnedObjectFramesFile.Create(repository, model, analysis),
            ];
            return check ? GeneratedFile.Check(files) : GeneratedFile.WriteAll(files);
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception.ToString());
            return 1;
        }
    }
}
