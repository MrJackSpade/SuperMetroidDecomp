using Godot;

namespace Engine.Content;

/// <summary>
/// Loads the game's assets: art, sound, room layouts and data files, each named by its path within the assets tree.
/// It is the one place that turns such a path into a file, and the one place that knows where the assets tree is:
/// the project's own assets folder while developing, the installed folder in an exported game.
/// </summary>
public sealed class ContentLibrary
{
    /// <summary>The project setting naming the folder an exported game's assets are installed in.</summary>
    public const string InstalledFolderSetting = "engine/installed_assets";

    /// <summary>The assets tree's folder, as an absolute path.</summary>
    private readonly string _root;

    /// <summary>The textures loaded so far, by path.</summary>
    private readonly Dictionary<string, Texture2D> _textures = new(StringComparer.Ordinal);

    /// <summary>A library reading the assets tree at <paramref name="rootFolder"/>, an absolute path.</summary>
    public ContentLibrary(string rootFolder)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rootFolder);
        _root = Path.GetFullPath(rootFolder);
    }

    /// <summary>The library for the running program: the project's assets folder, or the installed one when exported.</summary>
    public static ContentLibrary ForProject()
    {
#if TOOLS
        return new ContentLibrary(ProjectSettings.GlobalizePath("res://assets"));
#else
        string folder = ProjectSettings.GetSetting(InstalledFolderSetting, "").AsString();
        if (folder.Length == 0)
            throw new InvalidOperationException($"The project setting \"{InstalledFolderSetting}\" names no installed assets folder.");
        return new ContentLibrary(ProjectSettings.GlobalizePath(folder));
#endif
    }

    /// <summary>Whether the assets tree holds a file at <paramref name="path"/>.</summary>
    public bool Has(string path) => File.Exists(Resolve(path));

    /// <summary>The text of the file at <paramref name="path"/>.</summary>
    public string ReadText(string path) => File.ReadAllText(RequireFile(path));

    /// <summary>The data file at <paramref name="path"/>.</summary>
    public DataNode ReadData(string path) => DataNode.Parse(path, ReadText(path));

    /// <summary>The picture at <paramref name="path"/>, as a texture. A picture is loaded once and shared.</summary>
    public Texture2D LoadTexture(string path)
    {
        if (_textures.TryGetValue(path, out Texture2D? loaded))
            return loaded;
        Image image = Image.LoadFromFile(RequireFile(path))
            ?? throw new ContentException(path, "is not a picture the game can read.");
        ImageTexture texture = ImageTexture.CreateFromImage(image);
        _textures[path] = texture;
        return texture;
    }

    /// <summary>The sound at <paramref name="path"/>.</summary>
    public AudioStream LoadSound(string path) =>
        AudioStreamWav.LoadFromFile(RequireFile(path)) ?? throw new ContentException(path, "is not a sound the game can read.");

    /// <summary>The file at <paramref name="path"/>, which must exist.</summary>
    private string RequireFile(string path)
    {
        string file = Resolve(path);
        return File.Exists(file) ? file : throw new ContentException(path, "does not exist in the assets folder.");
    }

    /// <summary>The absolute file a path within the assets tree names. The path must stay inside the tree.</summary>
    private string Resolve(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        string file = Path.GetFullPath(Path.Combine(_root, path));
        if (!file.StartsWith(_root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            throw new ContentException(path, "is not a path inside the assets folder.");
        return file;
    }
}
