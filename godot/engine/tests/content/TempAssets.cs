using Engine.Content;

namespace Engine.Tests.Content;

/// <summary>A throwaway assets folder for one check, with the files the check writes into it.</summary>
public sealed class TempAssets : IDisposable
{
    /// <summary>The folder, an absolute path.</summary>
    public string Folder { get; } = Path.Combine(Path.GetTempPath(), "engine-checks", Guid.NewGuid().ToString("N"));

    /// <summary>A new, empty assets folder.</summary>
    public TempAssets()
    {
        Directory.CreateDirectory(Folder);
        Library = new ContentLibrary(Folder);
    }

    /// <summary>A library reading the folder.</summary>
    public ContentLibrary Library { get; }

    /// <summary>Writes a text file at <paramref name="path"/> within the folder.</summary>
    public TempAssets With(string path, string text)
    {
        string file = Path.Combine(Folder, path);
        Directory.CreateDirectory(Path.GetDirectoryName(file) ?? Folder);
        File.WriteAllText(file, text);
        return this;
    }

    /// <summary>Deletes the folder and everything in it.</summary>
    public void Dispose() => Directory.Delete(Folder, recursive: true);
}
