using System.Text;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>
/// The installed sheets, as the source of the Godot game's pictures. The installation keeps each thing's frames on a
/// sheet, each frame a rectangle with the point of it that is the thing's place. The Godot game shows one picture
/// file for each frame and puts every picture of a thing at one fixed offset from the thing's place, so a sheet is
/// cut into one picture a frame, all on one canvas big enough for every frame with the thing's place at the same
/// point of each.
/// </summary>
internal sealed partial class EngineSheets
{
    private readonly InstalledPictureStore _store;
    private readonly Dictionary<(string Family, string Sheet), InstalledSheetDocument> _documents = [];
    private readonly Dictionary<(string Family, string Sheet, string Folder), EngineSheetPictures> _cut = [];

    /// <summary>The sheets of the installation whose content is in <paramref name="content"/>.</summary>
    internal EngineSheets(string content)
    {
        _store = InstalledPictureStore.Open(Path.Combine(content, PictureStoreFiles.DirectoryName));
        foreach (InstalledSheetManifest manifest in InstalledSheetManifests.Read(content))
            _documents[(manifest.Family, manifest.Sheet)] = manifest.Document;
    }

    /// <summary>The names of a family's sheets that begin with <paramref name="prefix"/>, in order.</summary>
    internal IReadOnlyList<string> SheetsBeginning(string family, string prefix) =>
        [.. _documents.Keys.Where(key => key.Family == family && key.Sheet.StartsWith(prefix, StringComparison.Ordinal))
            .Select(key => key.Sheet).Order(StringComparer.Ordinal)];

    /// <summary>
    /// The one sheet of a family named <paramref name="name"/>, or named that with a mark of which colours it is in
    /// after a full stop; of several such, the first in order.
    /// </summary>
    /// <exception cref="InvalidDataException">The family has no such sheet.</exception>
    internal string SheetNamed(string family, string name)
    {
        if (_documents.ContainsKey((family, name)))
            return name;
        IReadOnlyList<string> marked = SheetsBeginning(family, name + ".");
        return marked.Count > 0 ? marked[0] : throw new InvalidDataException($"The installation has no sheet {name} in {family}.");
    }

    /// <summary>
    /// Writes each frame of a sheet as a picture under <paramref name="folder"/> of <paramref name="tree"/>, named for
    /// the frame, and says where each is and the offset they all share.
    /// </summary>
    internal EngineSheetPictures Cut(EngineAssetTree tree, string family, string sheet, string folder)
    {
        if (_cut.TryGetValue((family, sheet, folder), out EngineSheetPictures? done))
            return done;
        InstalledSheetDocument document = _documents.TryGetValue((family, sheet), out InstalledSheetDocument? found)
            ? found
            : throw new InvalidDataException($"The installation has no sheet {sheet} in {family}.");
        SpriteImagePixels image = _store.Assemble(sheet, document);

        // The canvas reaches as far each way from the thing's place as any frame does.
        int left = 0, top = 0, right = 1, bottom = 1;
        foreach (InstalledSheetFrameDocument frame in document.Frames.Values)
        {
            left = Math.Min(left, -frame.Origin.X);
            top = Math.Min(top, -frame.Origin.Y);
            right = Math.Max(right, frame.Size.X - frame.Origin.X);
            bottom = Math.Max(bottom, frame.Size.Y - frame.Origin.Y);
        }

        int width = right - left, height = bottom - top;
        var paths = new Dictionary<string, string>(StringComparer.Ordinal);
        var canvas = new RgbaPixel[width * height];
        foreach ((string name, InstalledSheetFrameDocument frame) in document.Frames)
        {
            if (frame.Picture is null)
            {
                // A frame that shows nothing has no picture, and a step that shows it shows nothing.
                paths[name] = "";
                continue;
            }

            Array.Clear(canvas);
            int across = -frame.Origin.X - left, down = -frame.Origin.Y - top;
            for (int y = 0; y < frame.Size.Y; y++)
            {
                image.Pixels.AsSpan((frame.Corner.Y + y) * document.Size.X + frame.Corner.X, frame.Size.X)
                    .CopyTo(canvas.AsSpan((down + y) * width + across, frame.Size.X));
            }

            string path = $"{folder}/{FileName(name)}.png";
            tree.WritePicture(path, width, height, canvas);
            paths[name] = path;
        }

        foreach (string blank in document.Blank ?? [])
            paths.TryAdd(blank, "");
        var pictures = new EngineSheetPictures(left, top, width, height, paths);
        _cut[(family, sheet, folder)] = pictures;
        return pictures;
    }

    /// <summary>A frame's name as a file name: lower-case words joined by underscores.</summary>
    internal static string FileName(string name)
    {
        var text = new StringBuilder(name.Length + 8);
        for (int index = 0; index < name.Length; index++)
        {
            char letter = name[index];
            if (!char.IsLetterOrDigit(letter))
            {
                if (text.Length > 0 && text[^1] != '_')
                    text.Append('_');
                continue;
            }

            bool startsWord = char.IsUpper(letter) && index > 0 && char.IsLetterOrDigit(name[index - 1])
                && (!char.IsUpper(name[index - 1]) || (index + 1 < name.Length && char.IsLower(name[index + 1])));
            if (startsWord && text.Length > 0 && text[^1] != '_')
                text.Append('_');
            text.Append(char.ToLowerInvariant(letter));
        }

        return text.ToString().TrimEnd('_');
    }
}

/// <summary>The pictures a sheet was cut into.</summary>
/// <param name="OffsetX">Where every picture's left edge lies from the thing's place; never right of it.</param>
/// <param name="OffsetY">Where every picture's top edge lies from the thing's place; never below it.</param>
/// <param name="Width">The width of every picture.</param>
/// <param name="Height">The height of every picture.</param>
/// <param name="Paths">Each frame's picture, by the frame's name; empty for a frame that shows nothing.</param>
internal sealed partial record EngineSheetPictures(int OffsetX, int OffsetY, int Width, int Height, IReadOnlyDictionary<string, string> Paths)
{
    /// <summary>The picture of the frame named <paramref name="frame"/>.</summary>
    /// <exception cref="InvalidDataException">The sheet has no such frame.</exception>
    internal string Of(string frame) => Paths.TryGetValue(frame, out string? path)
        ? path
        : throw new InvalidDataException($"A sheet cut for the engine has no frame named {frame}.");
}
