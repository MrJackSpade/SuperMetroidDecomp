using System.Text;
using System.Text.Json.Nodes;
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
    private readonly Dictionary<(string Family, string Sheets, string Folder), EngineSheetPictures> _cut = [];

    /// <summary>The sheets of the installation whose content is in <paramref name="content"/>.</summary>
    internal EngineSheets(string content)
    {
        _store = InstalledPictureStore.Open(Path.Combine(content, PictureStoreFiles.DirectoryName));
        foreach (InstalledSheetManifest manifest in InstalledSheetManifests.Read(content))
            _documents[(manifest.Family, manifest.Sheet)] = manifest.Document;
        string looks = Path.Combine(content, GameInstallationLayout.EnemySheetDirectoryName, EnemySheetFiles.ManifestFileName);
        _enemyLooks = JsonNode.Parse(File.ReadAllText(looks))?.AsObject()
            ?? throw new InvalidDataException($"The list of the enemies' sheets {looks} is empty.");
    }

    /// <summary>
    /// The installed list of which sheet each room's enemies, and the things they spawn, are drawn from. The old game
    /// draws some of them differently from room to room: an enemy is given a set of its pictures for each look it has
    /// (<see cref="EnemyLooks"/>); what they spawn is drawn in one look of each.
    /// </summary>
    private readonly JsonObject _enemyLooks;

    /// <summary>The name of the sheets' family that holds the enemies and what they spawn.</summary>
    private const string EnemyFamily = GameInstallationLayout.EnemySheetDirectoryName;

    /// <summary>The one of <paramref name="counted"/> met most often; of those met equally often, the first by name.</summary>
    private static string? MostCommon(IEnumerable<string> counted) => counted
        .GroupBy(name => name, StringComparer.Ordinal)
        .OrderByDescending(group => group.Count())
        .ThenBy(group => group.Key, StringComparer.Ordinal)
        .Select(group => group.Key)
        .FirstOrDefault();

    /// <summary>
    /// The sheet an enemy is drawn from: the one the most rooms draw it from, where the installation lists rooms that
    /// hold it, and otherwise the sheet of its name.
    /// </summary>
    internal string EnemySheet(string enemy) => EnemyLooks(enemy)[0].Sheet;

    /// <summary>
    /// The looks the old game draws an enemy in, room by room, each with its sheet and the rooms' enemy populations
    /// that draw it so: the one the most rooms draw first, and of those drawn by as many, the first by the
    /// installation's name for it. A look is named by a word, the name of the first population that draws it, which
    /// no other look of the enemy can share. An enemy no room's population lists has one look, the sheet of its name.
    /// </summary>
    internal IReadOnlyList<EngineEnemyLook> EnemyLooks(string enemy)
    {
        var drawnBy = new Dictionary<string, List<string>>(StringComparer.Ordinal);
        foreach ((string population, JsonNode? listed) in _enemyLooks["populations"]!.AsObject())
        {
            if (listed!["enemies"]![enemy]?.GetValue<string>() is not { } look)
                continue;
            if (!drawnBy.TryGetValue(look, out List<string>? populations))
                drawnBy[look] = populations = [];
            populations.Add(population);
        }

        if (drawnBy.Count == 0)
            return [new EngineEnemyLook(FileName(enemy), SheetNamed(EnemyFamily, enemy), [])];
        return [.. drawnBy
            .OrderByDescending(look => look.Value.Count)
            .ThenBy(look => look.Key, StringComparer.Ordinal)
            .Select(look => new EngineEnemyLook(FileName(look.Value[0]), _enemyLooks["variants"]![look.Key]!["sheet"]!.GetValue<string>(), look.Value))];
    }

    /// <summary>The sheet the rooms whose enemy populations are <paramref name="populations"/> all draw an enemy from.</summary>
    /// <exception cref="InvalidDataException">One of the populations does not draw the enemy, or they draw it in more than one look.</exception>
    internal string EnemySheetIn(string enemy, IEnumerable<string> populations)
    {
        var wanted = populations.ToHashSet(StringComparer.Ordinal);
        IReadOnlyList<EngineEnemyLook> looks = EnemyLooks(enemy);
        if (wanted.FirstOrDefault(population => !looks.Any(look => look.Populations.Contains(population))) is { } missing)
            throw new InvalidDataException($"The rooms of the population {missing} do not draw {enemy}.");
        EngineEnemyLook[] drawing = [.. looks.Where(look => look.Populations.Any(wanted.Contains))];
        return drawing.Length == 1
            ? drawing[0].Sheet
            : throw new InvalidDataException($"The rooms of the populations {string.Join(", ", wanted)} draw {enemy} in {drawing.Length} looks, not one.");
    }

    /// <summary>The look of what <paramref name="spawner"/> spawns that the most rooms show.</summary>
    private JsonObject SpawnedLook(string spawner)
    {
        string look = MostCommon(_enemyLooks["populations"]!.AsObject()
            .Select(population => population.Value!["spawned"]![spawner]?.GetValue<string>())
            .OfType<string>())
            ?? throw new InvalidDataException($"The installation lists no room that draws what {spawner} spawns.");
        return _enemyLooks["spawnedVariants"]![look]!.AsObject();
    }

    /// <summary>
    /// The look of the shared things that the most rooms holding <paramref name="spawner"/> show. What every room
    /// shares is not drawn alike in every room, and some of it is drawn only where the enemy that leaves it is.
    /// </summary>
    private JsonObject SharedLookBeside(string spawner)
    {
        string look = MostCommon(_enemyLooks["populations"]!.AsObject()
            .Where(population => population.Value!["spawned"]![spawner] is not null)
            .Select(population => population.Value!["spawned"]![SharedSpawner]?.GetValue<string>())
            .OfType<string>())
            ?? throw new InvalidDataException($"The installation lists no room that draws what {spawner} spawns.");
        return _enemyLooks["spawnedVariants"]![look]!.AsObject();
    }

    /// <summary>The look of the shared things in a room that holds no enemies, which has some pictures no other look has.</summary>
    private JsonObject StandardLook() =>
        _enemyLooks["spawnedVariants"]![_enemyLooks["standard"]!["spawned"]![SharedSpawner]!.GetValue<string>()]!.AsObject();

    /// <summary>The name the installation gives the things every room's enemies share: explosions, drops, dust.</summary>
    internal const string SharedSpawner = "Shared";

    /// <summary>
    /// How many pixels above its place the cartridge shows what it draws on a room's own layers, against its sprites:
    /// one. It shows a room's tiles and background from the row below the view's and draws a sprite where its place
    /// says, so the Godot game lays a room's terrain and backdrops a row up, and a picture made of the room's blocks,
    /// or a boss's body, lies a row above its place with them. Its place, where it is in play, does not move.
    /// </summary>
    internal const int LayersRowAbove = 1;

    /// <summary>
    /// Where a look keeps the picture named <paramref name="frame"/>: its sheet, or an empty name for a picture that
    /// shows nothing; null when the look has no such picture.
    /// </summary>
    private string? SheetOfSpawned(JsonObject look, string frame)
    {
        if (look["sheet"]?.GetValue<string>() is { } own && _documents[(EnemyFamily, own)].Frames.ContainsKey(frame))
            return own;
        foreach ((string sheet, JsonNode? held) in look["commonPictures"]!.AsObject())
        {
            if (held!.AsArray().Any(name => name!.GetValue<string>() == frame))
                return sheet;
        }

        return look["empty"]?.AsArray().Any(name => name!.GetValue<string>() == frame) == true ? "" : null;
    }

    /// <summary>
    /// The sheet that holds a picture of what <paramref name="spawner"/> spawns, or an empty name for one that shows
    /// nothing. It is looked for among the spawner's own pictures; then among the shared ones as the rooms that hold
    /// the spawner show them; then as most rooms show them; then as a room with no enemies shows them; and last in
    /// whichever look of the shared things holds it that the most rooms show, since some shared pictures are drawn
    /// only in the few rooms that need them.
    /// </summary>
    /// <exception cref="InvalidDataException">No sheet holds the picture.</exception>
    internal string SheetHolding(string spawner, string frame)
    {
        // The old game's point at the thing's place is its picture of nothing, and is on no sheet.
        if (frame == SuperMetroid.Core.Enemies.Artwork.EnemySheetDefinitions.EffectActorFrameName(SuperMetroid.Core.Enemies.Artwork.EnemyFrameId.PointAtOrigin))
            return "";
        if ((SheetOfSpawned(SpawnedLook(spawner), frame) ?? SheetOfSpawned(SharedLookBeside(spawner), frame)
            ?? SheetOfSpawned(SpawnedLook(SharedSpawner), frame) ?? SheetOfSpawned(StandardLook(), frame)) is { } found)
            return found;
        IEnumerable<string> sharedLooks = _enemyLooks["populations"]!.AsObject()
            .Select(population => population.Value!["spawned"]![SharedSpawner]?.GetValue<string>())
            .OfType<string>()
            .GroupBy(look => look, StringComparer.Ordinal)
            .OrderByDescending(group => group.Count())
            .ThenBy(group => group.Key, StringComparer.Ordinal)
            .Select(group => group.Key);
        foreach (string look in sharedLooks)
        {
            if (SheetOfSpawned(_enemyLooks["spawnedVariants"]![look]!.AsObject(), frame) is { } sheet)
                return sheet;
        }

        throw new InvalidDataException($"No sheet of what {spawner} spawns holds the picture {frame}.");
    }

    /// <summary>
    /// Cuts named pictures of what an enemy spawns, or of what every room shares, into one picture each, all on one
    /// canvas of their own, under <paramref name="folder"/>. A picture is looked for as <see cref="SheetHolding"/> looks.
    /// </summary>
    /// <exception cref="InvalidDataException">A picture is on no sheet.</exception>
    internal EngineSheetPictures CutSpawned(EngineAssetTree tree, string folder, string spawner, IEnumerable<string> frames)
    {
        var placed = new List<(string Name, string Sheet)>();
        foreach (string frame in frames.Distinct(StringComparer.Ordinal))
            placed.Add((frame, SheetHolding(spawner, frame)));

        return CutFrames(tree, EnemyFamily, folder, placed);
    }

    /// <summary>
    /// The pixels of one picture of what an enemy spawns, or of what every room shares, as it is cut on its sheet,
    /// with its width and height. It is looked for as <see cref="CutSpawned"/> looks.
    /// </summary>
    /// <exception cref="InvalidDataException">The picture is on no sheet, or shows nothing.</exception>
    internal (RgbaPixel[] Pixels, int Width, int Height) SpawnedFrame(string spawner, string frame)
    {
        string sheet = SheetHolding(spawner, frame);
        if (sheet.Length == 0)
            throw new InvalidDataException($"The picture {frame} of what {spawner} spawns shows nothing.");
        InstalledSheetFrameDocument placed = _documents[(EnemyFamily, sheet)].Frames[frame];
        RgbaPixel[] pixels = FramePixels(EnemyFamily, sheet, frame, placed.Size.X, placed.Size.Y)
            ?? throw new InvalidDataException($"The picture {frame} of what {spawner} spawns shows nothing.");
        return (pixels, placed.Size.X, placed.Size.Y);
    }

    /// <summary>Cuts named frames, each from the sheet given for it or showing nothing where no sheet is given, onto one canvas.</summary>
    private EngineSheetPictures CutFrames(
        EngineAssetTree tree, string family, string folder, IReadOnlyList<(string Name, string Sheet)> frames, IReadOnlyList<string>? shades = null,
        Func<string, string>? fileName = null, bool regionMaps = false)
    {
        InstalledSheetFrameDocument FrameOf((string Name, string Sheet) frame) =>
            _documents[(family, frame.Sheet)].Frames.TryGetValue(frame.Name, out InstalledSheetFrameDocument? found)
                ? found
                : throw new InvalidDataException($"The sheet {frame.Sheet} has no frame {frame.Name}.");

        int left = 0, top = 0, right = 1, bottom = 1;
        foreach ((string Name, string Sheet) frame in frames.Where(frame => frame.Sheet.Length > 0))
        {
            InstalledSheetFrameDocument placed = FrameOf(frame);
            left = Math.Min(left, -placed.Origin.X);
            top = Math.Min(top, -placed.Origin.Y);
            right = Math.Max(right, placed.Size.X - placed.Origin.X);
            bottom = Math.Max(bottom, placed.Size.Y - placed.Origin.Y);
        }

        int width = right - left, height = bottom - top;
        var paths = new Dictionary<string, string>(StringComparer.Ordinal);
        var canvas = new RgbaPixel[width * height];
        foreach ((string Name, string Sheet) frame in frames)
        {
            if (frame.Sheet.Length == 0 || FrameOf(frame) is not { Picture: not null } placed)
            {
                paths[frame.Name] = "";
                continue;
            }

            InstalledSheetDocument document = _documents[(family, frame.Sheet)];
            if (!_images.TryGetValue((family, frame.Sheet), out SpriteImagePixels? image))
                _images[(family, frame.Sheet)] = image = _store.Assemble(frame.Sheet, document);
            Array.Clear(canvas);
            int across = -placed.Origin.X - left, down = -placed.Origin.Y - top;
            Dictionary<(int Colour, int Region), RgbaPixel>? shown = shades is null ? null : ShadesOf(frame.Sheet, document, shades);
            for (int y = 0; y < placed.Size.Y; y++)
            {
                int from = (placed.Corner.Y + y) * document.Size.X + placed.Corner.X, to = (down + y) * width + across;
                image.Pixels.AsSpan(from, placed.Size.X).CopyTo(canvas.AsSpan(to, placed.Size.X));
                if (shown is null)
                    continue;
                // A shade shows some of the picture's colours as others, some of them only in one region of it.
                for (int x = 0; x < placed.Size.X; x++)
                {
                    RgbaPixel pixel = canvas[to + x];
                    int region = image.RecolourRegions is { } regions ? regions[from + x] : 0;
                    if (pixel.A != 0 && shown.TryGetValue(((pixel.R << 16) | (pixel.G << 8) | pixel.B, region), out RgbaPixel recoloured))
                        canvas[to + x] = recoloured with { A = pixel.A };
                }
            }

            string path = $"{folder}/{(fileName is null ? FileName(frame.Name) : fileName(frame.Name))}.png";
            tree.WritePicture(path, width, height, canvas);
            paths[frame.Name] = path;
            if (regionMaps && image.RecolourRegions is { } placedRegions)
            {
                // The picture's map of regions lies beside it, where any pixel it shows is in a region but the first.
                var canvasRegions = new int[width * height];
                bool told = false;
                for (int y = 0; y < placed.Size.Y; y++)
                {
                    int from = (placed.Corner.Y + y) * document.Size.X + placed.Corner.X, to = (down + y) * width + across;
                    for (int x = 0; x < placed.Size.X; x++)
                    {
                        if (image.Pixels[from + x].A == 0)
                            continue;
                        canvasRegions[to + x] = placedRegions[from + x];
                        told |= placedRegions[from + x] != 0;
                    }
                }

                if (told)
                    tree.WriteRegionMap(path, width, height, canvasRegions);
            }
        }

        return new EngineSheetPictures(left, top, width, height, paths);
    }

    /// <summary>
    /// Cuts every frame of one sheet as one of the sheet's shades shows it: the old game's change of colours for a
    /// glow or a flash, kept as pictures of their own. The pictures are of the size, and lie where, the sheet's own do.
    /// </summary>
    /// <exception cref="InvalidDataException">The sheet has no such shade.</exception>
    internal EngineSheetPictures CutShade(EngineAssetTree tree, string family, string folder, string sheet, string shade)
    {
        InstalledSheetDocument document = _documents.TryGetValue((family, sheet), out InstalledSheetDocument? found)
            ? found
            : throw new InvalidDataException($"The installation has no sheet {sheet} in {family}.");
        return CutFrames(tree, family, folder, [.. document.Frames.Keys.Select(name => (name, sheet))], [shade]);
    }

    /// <summary>
    /// What each colour of a sheet shows as in one of its shades, by the colour and the region of the picture it is in.
    /// Several shades named with a plus sign between them are laid one over another as the old game laid them: each
    /// says what some of the sheet's own colours show as, and a later one's word stands over an earlier one's.
    /// </summary>
    private static Dictionary<(int Colour, int Region), RgbaPixel> ShadeOf(string sheet, InstalledSheetDocument document, string shade)
    {
        static int Colour(string text) => text.Length == 7 && text[0] == '#'
            ? Convert.ToInt32(text[1..], 16)
            : throw new InvalidDataException($"A shade names the colour {text}, which is not written as #rrggbb.");
        var shown = new Dictionary<(int Colour, int Region), RgbaPixel>();
        foreach (string laid in shade.Split('+'))
        {
            if (document.Shades is null || !document.Shades.TryGetValue(laid, out InstalledSheetShadeDocument? listed) || listed?.Substitutions is not { } substitutions)
                throw new InvalidDataException($"The sheet {sheet} has no shade {laid}.");
            foreach (InstalledColourSubstitutionDocument substitution in substitutions)
            {
                int to = Colour(substitution.Shows);
                shown[(Colour(substitution.Colour), substitution.Region)] = new RgbaPixel((byte)(to >> 16), (byte)(to >> 8), (byte)to);
            }
        }

        return shown;
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
    /// Writes each frame of one or more sheets as a picture under <paramref name="folder"/> of
    /// <paramref name="tree"/>, named for the frame, and says where each is and the offset they all share. Sheets cut
    /// together are the parts of one thing, which the Godot game shows at one offset: a body and its wings.
    /// </summary>
    internal EngineSheetPictures Cut(EngineAssetTree tree, string family, string folder, params ReadOnlySpan<string> sheets)
    {
        string together = string.Join('+', sheets);
        if (_cut.TryGetValue((family, together, folder), out EngineSheetPictures? done))
            return done;
        var documents = new List<(string Sheet, InstalledSheetDocument Document)>();
        foreach (string sheet in sheets)
        {
            documents.Add((sheet, _documents.TryGetValue((family, sheet), out InstalledSheetDocument? found)
                ? found
                : throw new InvalidDataException($"The installation has no sheet {sheet} in {family}.")));
        }

        // The canvas reaches as far each way from the thing's place as any frame does.
        int left = 0, top = 0, right = 1, bottom = 1;
        foreach (InstalledSheetFrameDocument frame in documents.SelectMany(sheet => sheet.Document.Frames.Values))
        {
            left = Math.Min(left, -frame.Origin.X);
            top = Math.Min(top, -frame.Origin.Y);
            right = Math.Max(right, frame.Size.X - frame.Origin.X);
            bottom = Math.Max(bottom, frame.Size.Y - frame.Origin.Y);
        }

        int width = right - left, height = bottom - top;
        var paths = new Dictionary<string, string>(StringComparer.Ordinal);
        var canvas = new RgbaPixel[width * height];
        foreach ((string sheet, InstalledSheetDocument document) in documents)
        {
            SpriteImagePixels image = _store.Assemble(sheet, document);
            foreach ((string name, InstalledSheetFrameDocument frame) in document.Frames)
            {
                if (paths.ContainsKey(name))
                    continue;
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
                if (image.RecolourRegions is { } regions)
                {
                    // The picture's map of regions lies beside it, where any pixel it shows is in a region but the
                    // first: a change of colours that holds in one region of the sheet finds it by this.
                    var canvasRegions = new int[width * height];
                    bool told = false;
                    for (int y = 0; y < frame.Size.Y; y++)
                    {
                        int from = (frame.Corner.Y + y) * document.Size.X + frame.Corner.X, to = (down + y) * width + across;
                        for (int x = 0; x < frame.Size.X; x++)
                        {
                            if (image.Pixels[from + x].A == 0)
                                continue;
                            canvasRegions[to + x] = regions[from + x];
                            told |= regions[from + x] != 0;
                        }
                    }

                    if (told)
                        tree.WriteRegionMap(path, width, height, canvasRegions);
                }
            }

            foreach (string blank in document.Blank ?? [])
                paths.TryAdd(blank, "");
        }

        // Where the frames lie against the room's tiles, when every one of them lies at the one depth.
        ActorDepth?[] depths = [.. documents.SelectMany(sheet => sheet.Document.Frames.Values).Select(frame => frame.Depth).Distinct()];
        var pictures = new EngineSheetPictures(left, top, width, height, paths) { Depth = depths.Length == 1 ? depths[0] : null };
        _cut[(family, together, folder)] = pictures;
        return pictures;
    }

    private readonly Dictionary<(string Family, string Sheet), SpriteImagePixels> _images = [];

    /// <summary>
    /// The pixels of one frame of a sheet, row by row, where the frame is of the size given; null where the sheet has
    /// no such frame or the frame shows nothing.
    /// </summary>
    /// <exception cref="InvalidDataException">The family has no such sheet, or the frame is of another size.</exception>
    internal RgbaPixel[]? FramePixels(string family, string sheet, string frame, int width, int height)
    {
        InstalledSheetDocument document = _documents.TryGetValue((family, sheet), out InstalledSheetDocument? found)
            ? found
            : throw new InvalidDataException($"The installation has no sheet {sheet} in {family}.");
        if (!document.Frames.TryGetValue(frame, out InstalledSheetFrameDocument? placed) || placed.Picture is null)
            return null;
        if (placed.Size.X != width || placed.Size.Y != height)
            throw new InvalidDataException($"The frame {frame} of the sheet {sheet} is {placed.Size.X} by {placed.Size.Y}, not {width} by {height}.");
        if (!_images.TryGetValue((family, sheet), out SpriteImagePixels? image))
            _images[(family, sheet)] = image = _store.Assemble(sheet, document);
        var pixels = new RgbaPixel[width * height];
        for (int y = 0; y < height; y++)
            image.Pixels.AsSpan((placed.Corner.Y + y) * document.Size.X + placed.Corner.X, width).CopyTo(pixels.AsSpan(y * width, width));
        return pixels;
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
    /// <summary>
    /// The depth against the room's tiles that every frame cut has, where they all have the one and the cut says it;
    /// null where they differ, have none, or the cut does not say.
    /// </summary>
    internal ActorDepth? Depth { get; init; }

    /// <summary>The picture of the frame named <paramref name="frame"/>.</summary>
    /// <exception cref="InvalidDataException">The sheet has no such frame.</exception>
    internal string Of(string frame) => Paths.TryGetValue(frame, out string? path)
        ? path
        : throw new InvalidDataException($"A sheet cut for the engine has no frame named {frame}.");

    /// <summary>
    /// Whether these pictures lie as <paramref name="other"/> do: on a canvas of the same size at the same offset, at
    /// the same depth, with the same frames, each showing something or nothing alike. Two such differ only in colours.
    /// </summary>
    internal bool LaidOutAs(EngineSheetPictures other) =>
        (OffsetX, OffsetY, Width, Height, Depth) == (other.OffsetX, other.OffsetY, other.Width, other.Height, other.Depth)
        && Paths.Count == other.Paths.Count
        && Paths.All(path => other.Paths.TryGetValue(path.Key, out string? its) && (its.Length == 0) == (path.Value.Length == 0));
}

/// <summary>One look the old game draws an enemy in, as some rooms draw it.</summary>
/// <param name="Word">The word it is named by, in the names of its pictures' folder and in the rooms that draw it so.</param>
/// <param name="Sheet">The installed sheet it is drawn from.</param>
/// <param name="Populations">The enemy populations of the rooms that draw it so.</param>
internal sealed partial record EngineEnemyLook(string Word, string Sheet, IReadOnlyList<string> Populations);
