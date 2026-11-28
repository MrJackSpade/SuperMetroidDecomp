using Godot;

namespace Engine.Content;

/// <summary>
/// Loads the game's assets: art, sound, room layouts and data files, each named by its path within the assets tree.
/// It is the one place that turns such a path into a file, and the one place that knows where the assets tree is:
/// the project's own assets folder while developing, the installed folder in an exported game.
/// </summary>
public sealed class ContentLibrary
{
    /// <summary>
    /// The folder an exported game's assets are installed in. They are made from the player's own ROM and never
    /// ship with the game, so they lie outside it, in a folder named "assets": beside the game's executable, or on
    /// Android in the game's own folder of the device's storage, which a computer can copy into and the game needs
    /// no permission to read.
    /// </summary>
    public static string InstalledFolder => Path.Combine(
        OS.GetName() == "Android"
            ? OS.GetSystemDir(OS.SystemDir.Documents, sharedStorage: false)
            : Path.GetDirectoryName(OS.GetExecutablePath()) ?? throw new InvalidOperationException("The game's executable has no folder."),
        "assets");

    /// <summary>The assets tree's folder, as an absolute path.</summary>
    private readonly string _root;

    /// <summary>The textures loaded so far, by path. A library and those made from it for a set of graphics share them.</summary>
    private readonly Dictionary<string, Texture2D> _textures;

    /// <summary>The scenes loaded so far, by path. A library and those made from it for a set of graphics share them.</summary>
    private readonly Dictionary<string, PackedScene> _scenes;

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
        _scenes = new Dictionary<string, PackedScene>(StringComparer.Ordinal);
    }

    private ContentLibrary(ContentLibrary whole, string graphics)
    {
        _root = whole._root;
        _textures = whole._textures;
        _scenes = whole._scenes;
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
        return new ContentLibrary(InstalledFolder);
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
    /// The scene at <paramref name="path"/>, a path of the project's. A scene is loaded once and kept for as long as
    /// the library is, and everything made from a scene named by its path is made from this one: a room, a shot's
    /// projectile, a weapon's. A scene loaded for one use and let go is found again by Godot's loader for as long as
    /// it exists, and one found after the collector has taken its managed half and before its finalizer has run has
    /// none: for a scene that is an error in the log, and for the scripted resources a scene carries (a room's
    /// versions and their conditions) it ends the process. Kept here, a scene and what it carries live as long as it
    /// can be asked for.
    /// </summary>
    /// <exception cref="InvalidOperationException">There is no scene at the path.</exception>
    public PackedScene LoadScene(string path)
    {
        if (_scenes.TryGetValue(path, out PackedScene? loaded))
            return loaded;
        PackedScene scene = ResourceLoader.Load<PackedScene>(path)
            ?? throw new InvalidOperationException($"There is no scene at \"{path}\".");
        _scenes[path] = scene;
        return scene;
    }

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
