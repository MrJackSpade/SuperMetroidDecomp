using System.Text;

namespace SuperMetroid.FrameDefinitionsGenerator;

/// <summary>One generated source file: where it goes, its content, and what to say when it is current or written.</summary>
/// <param name="Path">The absolute output path.</param>
/// <param name="Content">The generated content, with LF line endings.</param>
/// <param name="CurrentMessage">The message printed when the committed file matches.</param>
/// <param name="WrittenMessage">The message printed after writing it.</param>
internal sealed record GeneratedFile(string Path, string Content, string CurrentMessage, string WrittenMessage)
{
    /// <summary>The command that regenerates the files, as the stale message names it.</summary>
    public const string Command = "dotnet run --project tools/frame-definitions-generator -c Release";

    /// <summary>The repository-relative path of the generator, as the generated headers name it.</summary>
    public const string GeneratorPath = "tools/frame-definitions-generator";

    /// <summary>The AssetExtraction directory both files are written to.</summary>
    public static string OutputDirectory(string repository) =>
        System.IO.Path.Combine(repository, "csharp", "src", "SuperMetroid.AssetExtraction", "Enemies", "Artwork");

    /// <summary>Compares every file with its committed copy; returns nonzero when any differs.</summary>
    public static int Check(IEnumerable<GeneratedFile> files)
    {
        int stale = 0;
        foreach (GeneratedFile file in files)
        {
            string current = File.Exists(file.Path) ? File.ReadAllText(file.Path).Replace("\r\n", "\n", StringComparison.Ordinal) : "";
            if (current == file.Content)
            {
                Console.WriteLine(file.CurrentMessage);
                continue;
            }
            Console.Error.WriteLine($"{file.Path} is stale; run {Command}");
            stale++;
        }
        return stale == 0 ? 0 : 1;
    }

    /// <summary>Writes every file as UTF-8 without a byte-order mark.</summary>
    public static int WriteAll(IEnumerable<GeneratedFile> files)
    {
        foreach (GeneratedFile file in files)
        {
            File.WriteAllText(file.Path, file.Content, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            Console.WriteLine(file.WrittenMessage);
        }
        return 0;
    }
}
