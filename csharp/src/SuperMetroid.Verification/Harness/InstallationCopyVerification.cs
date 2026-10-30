using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.TestSupport.RepositoryInstallation;

namespace SuperMetroid.Verification.Harness;

/// <summary>Writable copies of the repository installation for checks that change installed files (#1466, #1487).</summary>
internal static class InstallationCopyVerification
{
    /// <summary>
    /// A writable copy of the repository installation in <paramref name="root"/>, for checks that edit, delete and repair
    /// installed files: the shared installation never changes, and a copy costs far less than extracting the ROM again.
    /// </summary>
    /// <exception cref="InvalidDataException">The copy holds no ROM.</exception>
    internal static GameInstallation CopyRepositoryInstallation(string root)
    {
        string source = RepositoryInstallation.Installation.ContentDirectory;
        string destination = Path.Combine(root, GameInstallationLayout.ContentDirectoryName);
        Directory.CreateDirectory(destination);
        foreach (string directory in Directory.EnumerateDirectories(source, "*", SearchOption.AllDirectories))
            Directory.CreateDirectory(Path.Combine(destination, Path.GetRelativePath(source, directory)));
        foreach (string file in Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories))
            File.Copy(file, Path.Combine(destination, Path.GetRelativePath(source, file)));
        return GameAssetInstaller.EnsureInstalled(root)
            ?? throw new InvalidDataException($"The installation copied to {root} holds no ROM.");
    }
}
