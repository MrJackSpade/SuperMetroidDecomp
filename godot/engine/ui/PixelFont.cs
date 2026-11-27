using Engine.Content;
using Godot;

namespace Engine.Ui;

/// <summary>
/// A font of pictures: every character it has is one cell of a picture, all cells the same size, side by side in
/// one row in the order the font lists its characters. A space is a cell that shows nothing.
/// </summary>
/// <remarks>
/// <code>
/// { "picture": "text/fonts/small.png", "cell": { "width": 8, "height": 8 }, "characters": "ABCDEFG0123.,'!" }
/// </code>
/// A font may give "line_height", how far apart its lines lie, when that is not its cell's height.
/// </remarks>
public sealed class PixelFont
{
    private readonly string _file;
    private readonly Dictionary<char, int> _cells;

    private PixelFont(string file, Texture2D picture, string picturePath, Vector2I cell, int lineHeight, Dictionary<char, int> cells)
    {
        _file = file;
        Picture = picture;
        PicturePath = picturePath;
        Cell = cell;
        LineHeight = lineHeight;
        _cells = cells;
    }

    /// <summary>The picture its characters are cells of.</summary>
    public Texture2D Picture { get; }

    /// <summary>The path, within the assets tree, of the picture its characters are cells of.</summary>
    public string PicturePath { get; }

    /// <summary>The width and height of one character's cell, in pixels.</summary>
    public Vector2I Cell { get; }

    /// <summary>How far below one line the next begins, in pixels.</summary>
    public int LineHeight { get; }

    /// <summary>Reads the font in the data file at <paramref name="file"/>, a path within the assets tree.</summary>
    /// <exception cref="ContentException">The file is missing something, or its picture is not as wide as its characters need.</exception>
    public static PixelFont Read(ContentLibrary content, string file)
    {
        ArgumentNullException.ThrowIfNull(content);
        DataNode data = content.ReadData(file);
        var cell = new Vector2I(data.Get("cell").Int("width"), data.Get("cell").Int("height"));
        if (cell.X <= 0 || cell.Y <= 0)
            throw data.Get("cell").Problem("must be a positive number of pixels each way");
        string characters = data.Text("characters");
        var cells = new Dictionary<char, int>();
        for (int index = 0; index < characters.Length; index++)
        {
            if (!cells.TryAdd(characters[index], index))
                throw data.Get("characters").Problem($"lists the character \"{characters[index]}\" twice");
        }

        Texture2D picture = content.LoadTexture(data.Text("picture"));
        if (picture.GetWidth() < cell.X * characters.Length || picture.GetHeight() < cell.Y)
            throw data.Problem($"lists {characters.Length} characters of {cell.X} by {cell.Y}, which its picture is too small to hold");
        int lineHeight = data.Has("line_height") ? data.Int("line_height") : cell.Y;
        if (lineHeight <= 0)
            throw data.Get("line_height").Problem("must be a positive number of pixels");
        return new PixelFont(file, picture, content.Named(data.Text("picture")), cell, lineHeight, cells);
    }

    /// <summary>Whether the font can show <paramref name="character"/>: it has a cell for it, or it is a space.</summary>
    public bool Has(char character) => character == ' ' || _cells.ContainsKey(character);

    /// <summary>The part of the picture that is <paramref name="character"/>; null for a space, which shows nothing.</summary>
    /// <exception cref="ContentException">The font has no such character.</exception>
    public Rect2? RegionOf(char character)
    {
        if (character == ' ')
            return null;
        if (!_cells.TryGetValue(character, out int cell))
            throw new ContentException(_file, $"has no character \"{character}\" (U+{(int)character:X4}).");
        return new Rect2(cell * Cell.X, 0, Cell.X, Cell.Y);
    }
}
