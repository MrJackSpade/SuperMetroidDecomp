using SuperMetroid.AssetExtraction.Installation;

namespace SuperMetroid.TestSupport.RepositoryInstallation.Verification;

/// <summary>A disposable copy of an installation root in a test scratch directory.</summary>
internal sealed class PrivateInstallationCopy : IDisposable
{
    /// <summary>Scratch root deleted when the private installation copy is disposed.</summary>
    private readonly TestScratchDirectory scratch = new("installation-copy");

    /// <summary>Copies every installed directory and file into isolated scratch storage.</summary>
    internal PrivateInstallationCopy(string sourceRoot)
    {
        Root = scratch.Root;
        CopyInstallation(sourceRoot, Root);
    }

    /// <summary>Copies every directory and file of an installation root into a new root.</summary>
    internal static void CopyInstallation(string sourceRoot, string destinationRoot)
    {
        Directory.CreateDirectory(destinationRoot);
        foreach (string directory in Directory.GetDirectories(sourceRoot, "*", SearchOption.AllDirectories))
            Directory.CreateDirectory(Path.Combine(destinationRoot, Path.GetRelativePath(sourceRoot, directory)));
        foreach (string file in Directory.GetFiles(sourceRoot, "*", SearchOption.AllDirectories))
            File.Copy(file, Path.Combine(destinationRoot, Path.GetRelativePath(sourceRoot, file)));
    }

    /// <summary>Root directory containing the isolated copy.</summary>
    internal string Root { get; }

    /// <summary>Game installation view over the isolated copy.</summary>
    internal GameInstallation Installation => new(Root);

    /// <summary>Deletes the scratch installation copy.</summary>
    public void Dispose() => scratch.Dispose();
}
