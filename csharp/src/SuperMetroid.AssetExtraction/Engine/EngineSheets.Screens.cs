using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>
/// The installed sheets of what is shown outside play and over it: the cinematics' sprites and pictures, the menus,
/// the map, the HUD. Their frames are cut by name, in one of the sheet's shades where the old game drew them in one.
/// </summary>
internal sealed partial class EngineSheets
{
    /// <summary>The name of the sheets' family that holds everything that is neither an enemy, a boss's body nor a room's blocks.</summary>
    internal const string SpriteFamily = GameInstallationLayout.SpriteSheetDirectoryName;

    /// <summary>The installed name of one of the old game's sprite sheets.</summary>
    internal static string SheetOf(SpriteSheetId sheet) => SpriteSheetDefinitions.FileNames[sheet];

    /// <summary>The frame of a sprite sheet named <paramref name="frame"/>.</summary>
    /// <exception cref="InvalidDataException">The installation has no such sheet, or the sheet no such frame.</exception>
    private (InstalledSheetDocument Document, InstalledSheetFrameDocument Frame) SpriteFrame(SpriteSheetId sheet, string frame)
    {
        string name = SheetOf(sheet);
        InstalledSheetDocument document = _documents.TryGetValue((SpriteFamily, name), out InstalledSheetDocument? found)
            ? found
            : throw new InvalidDataException($"The installation has no sheet {name} in {SpriteFamily}.");
        return document.Frames.TryGetValue(frame, out InstalledSheetFrameDocument? placed)
            ? (document, placed)
            : throw new InvalidDataException($"The sheet {name} has no frame {frame}.");
    }

    /// <summary>Whether a sprite sheet has a frame named <paramref name="frame"/>.</summary>
    internal bool HasSprite(SpriteSheetId sheet, string frame) =>
        _documents.TryGetValue((SpriteFamily, SheetOf(sheet)), out InstalledSheetDocument? document) && document.Frames.ContainsKey(frame);

    /// <summary>Whether a sprite sheet's frame named <paramref name="frame"/> shows anything.</summary>
    internal bool SpriteShows(SpriteSheetId sheet, string frame) => SpriteFrame(sheet, frame).Frame.Picture is not null;

    /// <summary>Whether the installation lists <paramref name="frame"/> among the pictures of a sprite sheet that show nothing.</summary>
    internal bool SpriteIsBlank(SpriteSheetId sheet, string frame) =>
        _documents.TryGetValue((SpriteFamily, SheetOf(sheet)), out InstalledSheetDocument? document) && document.Blank is { } blank && blank.Contains(frame);

    /// <summary>The names of a sprite sheet's frames, in the sheet's order.</summary>
    internal IReadOnlyList<string> SpriteNames(SpriteSheetId sheet) =>
        _documents.TryGetValue((SpriteFamily, SheetOf(sheet)), out InstalledSheetDocument? document)
            ? [.. document.Frames.Keys]
            : throw new InvalidDataException($"The installation has no sheet {SheetOf(sheet)} in {SpriteFamily}.");

    /// <summary>The names of a sprite sheet's shades that begin with <paramref name="prefix"/>, in the sheet's order.</summary>
    internal IReadOnlyList<string> SpriteShades(SpriteSheetId sheet, string prefix) =>
        _documents.TryGetValue((SpriteFamily, SheetOf(sheet)), out InstalledSheetDocument? document)
            ? [.. (document.Shades?.Keys.AsEnumerable() ?? []).Where(shade => shade.StartsWith(prefix, StringComparison.Ordinal))]
            : throw new InvalidDataException($"The installation has no sheet {SheetOf(sheet)} in {SpriteFamily}.");

    /// <summary>Whether a sheet of any family has a frame named <paramref name="frame"/> that shows something.</summary>
    internal bool Shows(string family, string sheet, string frame) =>
        _documents.TryGetValue((family, sheet), out InstalledSheetDocument? document)
        && document.Frames.TryGetValue(frame, out InstalledSheetFrameDocument? placed) && placed.Picture is not null;

    /// <summary>The pixels of one frame of a sheet of any family, row by row, at the frame's own size and in the sheet's own colours.</summary>
    /// <exception cref="InvalidDataException">The sheet or the frame is not there, or the frame shows nothing.</exception>
    internal (RgbaPixel[] Pixels, int Width, int Height) PictureOf(string family, string sheet, string frame)
    {
        InstalledSheetDocument document = _documents.TryGetValue((family, sheet), out InstalledSheetDocument? found)
            ? found
            : throw new InvalidDataException($"The installation has no sheet {sheet} in {family}.");
        if (!document.Frames.TryGetValue(frame, out InstalledSheetFrameDocument? placed) || placed.Picture is null)
            throw new InvalidDataException($"The sheet {sheet} has no frame {frame} that shows anything.");
        RgbaPixel[] pixels = FramePixels(family, sheet, frame, placed.Size.X, placed.Size.Y)
            ?? throw new InvalidDataException($"The frame {frame} of the sheet {sheet} shows nothing.");
        return (pixels, placed.Size.X, placed.Size.Y);
    }

    /// <summary>The size of a sprite sheet's frame, and the point of it that is the place of what it shows.</summary>
    internal (int Width, int Height, int OriginX, int OriginY) SpriteSize(SpriteSheetId sheet, string frame)
    {
        InstalledSheetFrameDocument placed = SpriteFrame(sheet, frame).Frame;
        return (placed.Size.X, placed.Size.Y, placed.Origin.X, placed.Origin.Y);
    }

    /// <summary>
    /// The pixels of one frame of a sprite sheet, row by row, at the frame's own size; as the shades named show it,
    /// one laid over another, where any are named.
    /// </summary>
    /// <exception cref="InvalidDataException">The frame is not there or shows nothing, or the sheet lacks a shade.</exception>
    internal (RgbaPixel[] Pixels, int Width, int Height) SpritePixels(SpriteSheetId sheet, string frame, params string[] shades)
    {
        (InstalledSheetDocument document, InstalledSheetFrameDocument placed) = SpriteFrame(sheet, frame);
        string name = SheetOf(sheet);
        if (placed.Picture is null)
            throw new InvalidDataException($"The frame {frame} of the sheet {name} shows nothing.");
        if (!_images.TryGetValue((SpriteFamily, name), out SpriteImagePixels? image))
            _images[(SpriteFamily, name)] = image = _store.Assemble(name, document);
        Dictionary<(int Colour, int Region), RgbaPixel>? shown = shades.Length == 0 ? null : ShadesOf(name, document, shades);
        int width = placed.Size.X, height = placed.Size.Y;
        var pixels = new RgbaPixel[width * height];
        for (int y = 0; y < height; y++)
        {
            int from = (placed.Corner.Y + y) * document.Size.X + placed.Corner.X;
            image.Pixels.AsSpan(from, width).CopyTo(pixels.AsSpan(y * width, width));
            if (shown is null)
                continue;
            for (int x = 0; x < width; x++)
            {
                RgbaPixel pixel = pixels[y * width + x];
                int region = image.RecolourRegions is { } regions ? regions[from + x] : 0;
                if (pixel.A != 0 && shown.TryGetValue(((pixel.R << 16) | (pixel.G << 8) | pixel.B, region), out RgbaPixel recoloured))
                    pixels[y * width + x] = recoloured with { A = pixel.A };
            }
        }

        return (pixels, width, height);
    }

    /// <summary>
    /// Cuts named frames of a sprite sheet into one picture each under <paramref name="folder"/>, all on one canvas
    /// with the place of what they show at the same point of each; in the shades named, where any are. A picture
    /// some of whose pixels the sheet tells apart by region has its map of regions beside it, so that a change of
    /// colours that reaches only one region of it can be shown on it.
    /// </summary>
    internal EngineSheetPictures CutSprites(EngineAssetTree tree, SpriteSheetId sheet, string folder, IEnumerable<string> frames, params string[] shades)
    {
        string name = SheetOf(sheet);
        if (!_documents.ContainsKey((SpriteFamily, name)))
            throw new InvalidDataException($"The installation has no sheet {name} in {SpriteFamily}.");
        return CutFrames(
            tree, SpriteFamily, folder, [.. frames.Distinct(StringComparer.Ordinal).Select(frame => (frame, name))], shades.Length == 0 ? null : shades,
            fileName: null, regionMaps: true);
    }

    /// <summary>
    /// Writes one frame of a sprite sheet as the picture <paramref name="path"/>, at the frame's own size, and gives
    /// the path back: a whole picture, such as a card or a floor.
    /// </summary>
    internal string WriteSprite(EngineAssetTree tree, SpriteSheetId sheet, string frame, string path, params string[] shades)
    {
        (RgbaPixel[] pixels, int width, int height) = SpritePixels(sheet, frame, shades);
        tree.WritePicture(path, width, height, pixels);
        return path;
    }

    /// <summary>
    /// Writes one frame of a sprite sheet as <see cref="WriteSprite"/> does, in the sheet's own colours, with its map
    /// of regions beside it where the sheet tells regions apart: a picture whose colours are changed where it is
    /// drawn.
    /// </summary>
    internal string WriteSpriteWithRegions(EngineAssetTree tree, SpriteSheetId sheet, string frame, string path)
    {
        (RgbaPixel[] pixels, int width, int height) = SpritePixels(sheet, frame);
        tree.WritePicture(path, width, height, pixels);
        string name = SheetOf(sheet);
        if (HasRegions(SpriteFamily, name))
            tree.WriteRegionMap(path, width, height, FrameRegions(SpriteFamily, name, frame));
        return path;
    }

    /// <summary>
    /// Writes a font: the frames given, each one character's cell and all of one size, side by side in one picture,
    /// with the data file that lists the characters in that order.
    /// </summary>
    /// <param name="tree">The tree the font is written to.</param>
    /// <param name="file">The font's data file; its picture is written beside it under the same name.</param>
    /// <param name="sheet">The sheet the characters' frames are on.</param>
    /// <param name="characters">Each character and the frame that shows it.</param>
    /// <param name="lineHeight">How far apart the font's lines lie, where that is not a cell's height.</param>
    /// <param name="shades">The shades the frames are cut in, where any are named.</param>
    /// <exception cref="InvalidDataException">There are no characters, one is listed twice, or the frames are not all of one size.</exception>
    internal void WriteFont(
        EngineAssetTree tree, string file, SpriteSheetId sheet, IReadOnlyList<(char Character, string Frame)> characters, int? lineHeight = null, params string[] shades)
    {
        if (characters.Count == 0)
            throw new InvalidDataException($"The font {file} is given no characters.");
        if (characters.Select(character => character.Character).Distinct().Count() != characters.Count)
            throw new InvalidDataException($"The font {file} is given a character twice.");
        (RgbaPixel[] first, int width, int height) = SpritePixels(sheet, characters[0].Frame, shades);
        var strip = new RgbaPixel[width * characters.Count * height];
        for (int index = 0; index < characters.Count; index++)
        {
            (RgbaPixel[] pixels, int cellWidth, int cellHeight) = index == 0 ? (first, width, height) : SpritePixels(sheet, characters[index].Frame, shades);
            if (cellWidth != width || cellHeight != height)
                throw new InvalidDataException($"The font {file} has characters of different sizes: {characters[index].Frame} is {cellWidth} by {cellHeight}, not {width} by {height}.");
            for (int y = 0; y < height; y++)
                pixels.AsSpan(y * width, width).CopyTo(strip.AsSpan((y * characters.Count + index) * width, width));
        }

        string picture = file[..file.LastIndexOf('.')] + ".png";
        tree.WritePicture(picture, width * characters.Count, height, strip);
        var data = new System.Text.Json.Nodes.JsonObject
        {
            ["picture"] = picture,
            ["cell"] = new System.Text.Json.Nodes.JsonObject { ["width"] = width, ["height"] = height },
            ["characters"] = new string([.. characters.Select(character => character.Character)]),
        };
        if (lineHeight is { } apart)
            data["line_height"] = apart;
        tree.WriteData(file, data);
    }
}
