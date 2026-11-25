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

    /// <summary>The textures loaded so far, by path. A library and those made from it for a set of graphics share them.</summary>
    private readonly Dictionary<string, Texture2D> _textures;

    /// <summary>What a picture's path has put for <see cref="GraphicsPlaceholder"/>; empty for a library that puts nothing.</summary>
    private readonly string _graphics = "";

    /// <summary>
    /// What stands in a picture's path for the set of graphics a room is drawn with. Something that looks different
    /// from one set of graphics to another, as a door's cap does, keeps a picture for each set and names them all by
    /// one path with this in it.
    /// </summary>
    public const string GraphicsPlaceholder = "{graphics}";

    /// <summary>A library reading the assets tree at <paramref name="rootFolder"/>, an absolute path.</summary>
    public ContentLibrary(string rootFolder)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rootFolder);
        _root = Path.GetFullPath(rootFolder);
        _textures = new Dictionary<string, Texture2D>(StringComparer.Ordinal);
    }

    private ContentLibrary(ContentLibrary whole, string graphics)
    {
        _root = whole._root;
        _textures = whole._textures;
        _graphics = graphics;
    }

    /// <summary>
    /// This library as a room drawn with the set of graphics named <paramref name="graphics"/> reads it: the same
    /// files, with that name put for <see cref="GraphicsPlaceholder"/> in the path of every picture loaded and every
    /// file read.
    /// </summary>
    public ContentLibrary DrawnWith(string graphics)
    {
        ArgumentNullException.ThrowIfNull(graphics);
        return graphics == _graphics ? this : new ContentLibrary(this, graphics);
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

    /// <summary>What a picture's map of regions is named: the picture's own name with this in place of its ending.</summary>
    public const string RegionMapEnding = ".regions.png";

    /// <summary>
    /// The picture at <paramref name="path"/>, as a texture. A picture is loaded once and shared. A picture with a map
    /// of regions beside it (the same name ending <see cref="RegionMapEnding"/>: greys, each pixel as far from white
    /// as the number of the region the picture's pixel there lies in) is a texture drawn with that map as its
    /// specular texture, which whatever changes its colours region by region reads (<see cref="Ui.Recolouring"/>).
    /// </summary>
    public Texture2D LoadTexture(string path)
    {
        path = ForSet(path);
        if (_textures.TryGetValue(path, out Texture2D? loaded))
            return loaded;
        Texture2D texture = ReadPicture(path);
        string regions = Path.ChangeExtension(path, null) + RegionMapEnding;
        if (!path.EndsWith(RegionMapEnding, StringComparison.Ordinal) && Has(regions))
            texture = new CanvasTexture { DiffuseTexture = texture, SpecularTexture = ReadPicture(regions) };
        _textures[path] = texture;
        return texture;
    }

    /// <summary>
    /// What the front part of a picture of a room's tiles, or of what lies behind a room, is named: the picture's
    /// own name with this in place of its ending.
    /// </summary>
    public const string FrontEnding = ".front.png";

    private readonly Dictionary<string, Texture2D?> _fronts = new(StringComparer.Ordinal);

    /// <summary>
    /// The part of the picture at <paramref name="path"/> that lies in front of the things standing in a room, as a
    /// texture, or null for a picture with none: a picture of a room's tiles, or of what lies behind a room, has
    /// that part beside it, named as it is with <see cref="FrontEnding"/> for its ending. Looked for once.
    /// </summary>
    public Texture2D? LoadFrontOf(string path)
    {
        path = ForSet(path);
        if (_fronts.TryGetValue(path, out Texture2D? known))
            return known;
        string front = Path.ChangeExtension(path, null) + FrontEnding;
        return _fronts[path] = !path.EndsWith(FrontEnding, StringComparison.Ordinal) && Has(front) ? LoadTexture(front) : null;
    }

    /// <summary>The picture file at <paramref name="path"/>, read as a texture.</summary>
    private ImageTexture ReadPicture(string path)
    {
        Image image = Image.LoadFromFile(RequireFile(path))
            ?? throw new ContentException(path, "is not a picture the game can read.");
        return ImageTexture.CreateFromImage(image);
    }

    /// <summary>A path as this library reads it: with its set of graphics put where the path leaves the set to be named.</summary>
    public string Named(string path) => ForSet(path);

    /// <summary>The sound at <paramref name="path"/>.</summary>
    public AudioStream LoadSound(string path) =>
        AudioStreamWav.LoadFromFile(RequireFile(path)) ?? throw new ContentException(path, "is not a sound the game can read.");

    /// <summary>The file at <paramref name="path"/>, which must exist.</summary>
    private string RequireFile(string path)
    {
        string file = Resolve(path);
        return File.Exists(file) ? file : throw new ContentException(path, "does not exist in the assets folder.");
    }

    /// <summary>A path with this library's set of graphics put where the path leaves the set to be named.</summary>
    private string ForSet(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        if (!path.Contains(GraphicsPlaceholder, StringComparison.Ordinal))
            return path;
        if (_graphics.Length == 0)
            throw new ContentException(path, "is a file for a set of graphics, asked for where no set of graphics is known.");
        return path.Replace(GraphicsPlaceholder, _graphics, StringComparison.Ordinal);
    }

    /// <summary>The absolute file a path within the assets tree names. The path must stay inside the tree.</summary>
    private string Resolve(string path)
    {
        path = ForSet(path);
        string file = Path.GetFullPath(Path.Combine(_root, path));
        if (!file.StartsWith(_root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            throw new ContentException(path, "is not a path inside the assets folder.");
        return file;
    }
}
