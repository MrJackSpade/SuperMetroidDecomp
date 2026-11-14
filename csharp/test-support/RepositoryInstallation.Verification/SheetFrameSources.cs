using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.TestSupport.RepositoryInstallation.Verification;

/// <summary>
/// Verification access (#1392): the composition each installed Samus and enemy sheet frame was composed from, so a draw of
/// that frame can be compared with cartridge OAM oracles as the parts it shows. Frames are found by sheet, rectangle
/// and origin, since a runtime and the fixtures load the sheets separately.
/// </summary>
internal static class SheetFrameSources
{
    /// <summary>Each projectile sheet frame's composition and the page its characters index.</summary>
    private static readonly Lazy<System.Collections.Concurrent.ConcurrentDictionary<FrameKey, FrameSource>> sources =
        new(() => SharedFixtureTiming.Load("Fixture: sheet frame sources", Build));

    /// <summary>Pairs a fixture sheet's frames with what they were composed from.</summary>
    internal static void Register(SpriteSheet sheet, SpriteSheetSources composed)
    {
        Ensure.NotNull(sheet);
        Ensure.NotNull(composed);
        Add(sources.Value, sheet.Frame, composed);
    }

    /// <summary>
    /// Pairs the frames a graphics set's spawned-object pictures for one owner hold with what they were composed from: each
    /// from the set's own sheet or the common sheet holding it (#1513).
    /// </summary>
    internal static void Register(SuperMetroid.Core.Enemies.Artwork.SpawnedObjectSheet sheet, SpriteSheetSources composed)
    {
        Ensure.NotNull(sheet);
        Ensure.NotNull(composed);
        Add(sources.Value, sheet.Frame, composed);
    }

    /// <summary>
    /// Pairs one image a fixture pictures from OBJ characters with its composition over <paramref name="page"/>, its parts
    /// naming their own palette rows.
    /// </summary>
    /// <exception cref="InvalidOperationException">An image of the same name, rectangle and origin is registered already.</exception>
    internal static void Register(SpriteSheetFrame image, SpriteFrame composition, SpriteArtwork page, ScreenObjectSizes sizes)
    {
        Ensure.NotNull(image);
        Ensure.NotNull(composition);
        Ensure.NotNull(page);
        if (!sources.Value.TryAdd(Key(image), new FrameSource(composition, page, 0, null, true, sizes)))
            throw new InvalidOperationException($"Image {image.Sheet.Name} is registered already.");
    }

    /// <summary>
    /// Pairs each frame <paramref name="composed"/> composes, whose picture <paramref name="pictureOf"/> finds, with what it
    /// was composed from. Frames whose pictures a sheet lays out once share one rectangle, the first frame's composition
    /// standing for all of them over their own pages, which show the same picture; the others' differing compositions are
    /// kept, since over another page they may not.
    /// </summary>
    private static void Add(System.Collections.Concurrent.ConcurrentDictionary<FrameKey, FrameSource> target, Func<string, SpriteSheetFrame> pictureOf,
        SpriteSheetSources composed)
    {
        foreach (SheetFrameSource frame in composed.Frames)
        {
            SpriteSheetFrame image = pictureOf(frame.Name);
            var source = new FrameSource(frame.Frame, frame.Characters, composed.PaletteBits, frame.Palette, composed.PartsKeepOwnRows,
                composed.ObjectSizes);
            FrameKey key = Key(image);
            if (target.TryAdd(key, source))
                continue;
            FrameSource first = target[key];
            // Every empty frame shows nothing, so the first of those that share a key stands for all of them.
            if (image.Size.X == 0 || SameComposition(first, source))
                continue;
            alternates.AddOrUpdate(key, _ => [source], (_, others) => [.. others, source]);
        }
    }

    /// <summary>Whether two sources would build the same OAM: the same parts over the same page, in the same rows.</summary>
    private static bool SameComposition(FrameSource first, FrameSource second) =>
        ReferenceEquals(first.Page, second.Page) && first.PaletteBits == second.PaletteBits && first.FramePalette == second.FramePalette
        && first.PartsKeepOwnRows == second.PartsKeepOwnRows && first.Frame.Parts.SequenceEqual(second.Frame.Parts);

    /// <summary>Each shared rectangle's compositions that differ from the first frame's.</summary>
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<FrameKey, FrameSource[]> alternates = new();

    /// <summary>The character draw an image draw of a projectile sheet frame shows: its composition over its page, placed and mirrored alike.</summary>
    /// <exception cref="InvalidOperationException">The frame is not a projectile sheet frame.</exception>
    internal static CharacterSpriteDraw CharacterDraw(ImageSpriteDraw draw) => CharacterDraw(draw, null);

    /// <summary>
    /// The character draw an image draw of a sheet frame shows, its composition placed and mirrored alike, over
    /// <paramref name="characters"/> in place of the page it was composed over when given.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// The frame is not a sheet frame, or its picture is shared by frames whose compositions show different pictures over
    /// <paramref name="characters"/>, so which one the draw stands for decides the result.
    /// </exception>
    internal static CharacterSpriteDraw CharacterDraw(ImageSpriteDraw draw, SpriteArtwork? characters)
    {
        Ensure.NotNull(draw);
        FrameKey key = Key(draw.Image);
        FrameSource source = Find(key) ?? throw new InvalidOperationException($"{draw} shows a frame no sheet was composed with.");
        CharacterSpriteDraw built = Build(draw, source, characters);
        // Whether a shared picture's compositions agree over a page depends only on the frame and the page, so each pair
        // is decided once.
        if (characters is not null && alternates.TryGetValue(key, out FrameSource[]? others) &&
            agreedAlternates.GetValue(characters, _ => new()).TryAdd(key, 0))
        {
            Dictionary<(int X, int Y), int> inks = Inks(built);
            // Over the given page, compositions that differ only in the page they were composed over draw alike, so each
            // distinct composition is compared once and the frame's own is not compared at all.
            foreach (FrameSource other in others.Where(other => !CompositionOverPage.Instance.Equals(other, source)).Distinct(CompositionOverPage.Instance))
            {
                if (!SameInks(Inks(Build(draw, other, characters)), inks))
                    throw new InvalidOperationException(
                        $"{draw} shows a picture sheet {draw.Image.Sheet.Name} lays out once for frames {source.Frame.Name} and {other.Frame.Name}, whose compositions show different pictures over the given characters.");
            }
        }
        return built;
    }

    /// <summary>
    /// Every character draw an image draw of a sheet frame can stand for: its rectangle's first frame's composition, then
    /// each differing composition of the frames whose identical pictures share it. A cartridge OAM oracle matches one of
    /// them, since the sheet keeps one picture for all of them.
    /// </summary>
    /// <exception cref="InvalidOperationException">The frame is not a sheet frame.</exception>
    internal static IReadOnlyList<CharacterSpriteDraw> CharacterDrawCandidates(ImageSpriteDraw draw)
    {
        Ensure.NotNull(draw);
        FrameKey key = Key(draw.Image);
        FrameSource source = Find(key) ?? throw new InvalidOperationException($"{draw} shows a frame no sheet was composed with.");
        return [Build(draw, source, null), .. (alternates.TryGetValue(key, out FrameSource[]? others) ? others : []).Select(other => Build(draw, other, null))];
    }

    /// <summary>Compares two sources as compositions placed over another page: their parts and rows, whatever page each indexed.</summary>
    private sealed class CompositionOverPage : IEqualityComparer<FrameSource>
    {
        /// <summary>The comparer.</summary>
        internal static CompositionOverPage Instance { get; } = new();

        /// <inheritdoc/>
        public bool Equals(FrameSource? first, FrameSource? second) =>
            ReferenceEquals(first, second) || first is not null && second is not null && first.PaletteBits == second.PaletteBits
            && first.FramePalette == second.FramePalette && first.PartsKeepOwnRows == second.PartsKeepOwnRows
            && first.Frame.Parts.SequenceEqual(second.Frame.Parts);

        /// <inheritdoc/>
        public int GetHashCode(FrameSource source)
        {
            var hash = new HashCode();
            hash.Add(source.PaletteBits);
            hash.Add(source.FramePalette);
            hash.Add(source.PartsKeepOwnRows);
            foreach (SpriteFramePart part in source.Frame.Parts)
                hash.Add(part);
            return hash.ToHashCode();
        }
    }

    /// <summary>The frames whose shared picture's compositions were found to agree over each page.</summary>
    private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<SpriteArtwork, System.Collections.Concurrent.ConcurrentDictionary<FrameKey, byte>>
        agreedAlternates = [];

    /// <summary>Whether two compositions ink the same pixels with the same palette entries.</summary>
    private static bool SameInks(Dictionary<(int X, int Y), int> left, Dictionary<(int X, int Y), int> right) =>
        left.Count == right.Count && left.All(pixel => right.TryGetValue(pixel.Key, out int entry) && entry == pixel.Value);

    /// <summary>Each screen pixel <paramref name="draw"/> inks, with its palette entry; an earlier part's pixel stays over a later one's.</summary>
    private static Dictionary<(int X, int Y), int> Inks(CharacterSpriteDraw draw)
    {
        var inks = new Dictionary<(int X, int Y), int>();
        for (int part = 0; part < draw.Frame.Parts.Length; part++)
        {
            SpriteScreenPart placed = draw.Place(part);
            for (ushort y = 0; y < placed.Size.Y; y++)
            for (ushort x = 0; x < placed.Size.X; x++)
            {
                int character = placed.CharacterAt(new(x, y), out Position16<CharacterPixels> within);
                int colour = draw.Artwork.ColorIndex(character, within);
                if (colour != 0)
                    inks.TryAdd((placed.Position.X + x, placed.Position.Y + y), (placed.Palette * 16) + colour);
            }
        }
        return inks;
    }

    /// <summary>The character draw <paramref name="draw"/> shows as <paramref name="source"/>, over <paramref name="characters"/> when given.</summary>
    private static CharacterSpriteDraw Build(ImageSpriteDraw draw, FrameSource source, SpriteArtwork? characters)
    {
        PictureFlip flip = ((draw.Mirror & SpriteMirror.Horizontal) != 0 ? PictureFlip.Horizontal : PictureFlip.None) |
            ((draw.Mirror & SpriteMirror.Vertical) != 0 ? PictureFlip.Vertical : PictureFlip.None);
        // A frontend draw's drawn shade stands for the row its colours are the cartridge's of, which the cartridge ORs into
        // every part's own palette, as it ORs a draw's palette bits (#1512).
        if (source.PartsKeepOwnRows && DrawnShadeRows.RowOf(draw.Shading) is { } shaded)
            return new CharacterSpriteDraw(draw.Source, new SpriteFrame(source.Frame.Name,
                [.. source.Frame.Parts.Select(part => part with { Palette = (part.Palette ?? 0) | shaded })]), characters ?? source.Page, draw.Origin, flip, shaded, 0);
        // A family's palette bits OR into each part's own row, as the cartridge ORs an enemy's.
        // A shaded draw selects the row of its last selection shade, as a hit or freeze flash does (#1512).
        int bits = draw.Shading is { } shading ? SuperMetroid.Core.Enemies.Drawing.SpriteColouringAccess.RowSelectedBy(shading, source.PaletteBits)
            : source.PaletteBits;
        SpriteFrame frame = bits == 0 ? source.Frame
            : new SpriteFrame(source.Frame.Name, [.. source.Frame.Parts.Select(part => part with { Palette = (part.Palette ?? 0) | bits })]);
        return new CharacterSpriteDraw(draw.Source, frame, characters ?? source.Page, draw.Origin, flip, source.FramePalette ?? 0, 0);
    }

    /// <summary>The object sizes of the screen an image draw's sheet frame was composed for.</summary>
    /// <exception cref="InvalidOperationException">The frame is not a sheet frame.</exception>
    internal static ScreenObjectSizes ObjectSizes(ImageSpriteDraw draw)
    {
        Ensure.NotNull(draw);
        return (Find(Key(draw.Image)) ?? throw new InvalidOperationException($"{draw} shows a frame no sheet was composed with.")).ObjectSizes;
    }

    /// <summary>
    /// A frame's composition. An installed enemy sheet is registered on the first frame looked up on it, from each
    /// graphics set whose variants install it, since composing a set's members takes a while and a check draws few of them.
    /// </summary>
    private static FrameSource? Find(FrameKey key)
    {
        // Every set installing the sheet is registered first, even when a fixture registered the frame already: the
        // other sets' compositions of a picture the sheet keeps once are its alternates, which a draw may stand for.
        if (setsInstalling.Value.TryGetValue(key.Sheet, out HashSet<string>? sets))
        {
            foreach (SuperMetroid.AssetExtraction.Enemies.RoomEnemyGraphicsSetDefinition set in RoomEnemyGraphicsSetLists.DrawnUnder.Where(drawn => sets.Contains(drawn.Name)))
                _ = enemySources.GetOrAdd(set.Name, _ => new Lazy<bool>(() => RegisterSet(set))).Value;
        }
        return sources.Value.TryGetValue(key, out FrameSource? found) ? found : null;
    }

    /// <summary>The graphics sets whose variants install each enemy and spawned-object sheet, by the sheet's installed name.</summary>
    private static readonly Lazy<Dictionary<string, HashSet<string>>> setsInstalling = new(() =>
    {
        var result = new Dictionary<string, HashSet<string>>();
        void Add(string sheet, string set)
        {
            if (!result.TryGetValue(sheet, out HashSet<string>? sets))
                result[sheet] = sets = [];
            sets.Add(set);
        }
        foreach ((SuperMetroid.AssetExtraction.Enemies.RoomEnemyGraphicsSetDefinition set, SuperMetroid.Core.Enemies.Artwork.PopulationArtwork artwork) in InstalledFixtures.EnemySheets.BySet)
        {
            foreach (SuperMetroid.Core.Enemies.Artwork.EnemyArtworkVariant<SpriteSheet> variant in artwork.enemies.Values)
                Add(variant.Sheet.Image.Name, set.Name);
            foreach (SpriteSheet sheet in artwork.spawned.Values.SelectMany(variant => variant.Sheet.Sheets))
                Add(sheet.Image.Name, set.Name);
        }
        return result;
    });

    /// <summary>The inputs every graphics set's sheets are composed from.</summary>
    private static readonly Lazy<SuperMetroid.AssetExtraction.Enemies.EnemySheetInputs> enemySheetInputs = new(() =>
        SuperMetroid.AssetExtraction.Enemies.EnemySheetInputs.Load(CartridgeImportContent.Directory));

    /// <summary>Each graphics set whose sheets' frames are registered, once.</summary>
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, Lazy<bool>> enemySources = new();

    /// <summary>Registers every installed sheet frame of <paramref name="set"/> with what it was composed from.</summary>
    private static bool RegisterSet(SuperMetroid.AssetExtraction.Enemies.RoomEnemyGraphicsSetDefinition set) =>
        SharedFixtureTiming.Load($"Fixture: enemy sheet frame sources of {set.Name}", () =>
        {
            SuperMetroid.AssetExtraction.Enemies.GraphicsSetSheetSources setSources =
                SuperMetroid.AssetExtraction.Enemies.EnemySheetComposer.Sources(enemySheetInputs.Value, set);
            foreach ((SuperMetroid.Core.Enemies.EnemyDefinitionId enemy, SpriteSheetSources composed, _, _) in setSources.Enemies)
                Register(InstalledFixtures.EnemySheets.For(set, enemy), composed);
            foreach ((SuperMetroid.AssetExtraction.Enemies.SpawnedObjectSheetPlan plan, SpriteSheetSources composed, _, _, _) in setSources.Spawned)
                Register(InstalledFixtures.EnemySheets.Spawned(set, plan.Owner), composed);
            return true;
        });

    /// <summary>Pairs every installed projectile sheet frame with what it was composed from.</summary>
    private static System.Collections.Concurrent.ConcurrentDictionary<FrameKey, FrameSource> Build()
    {
        var result = new System.Collections.Concurrent.ConcurrentDictionary<FrameKey, FrameSource>();
        var catalog = new SpriteSheetSourceCatalog(CartridgeImportContent.Directory, InstalledFixtures.EndingColours);
        // Only sheets composed from sprite parts have frames to pair; the picture, page and floor sheets are painted directly.
        foreach (SpriteSheetId id in SpriteSheetDefinitions.Sheets.Where(SpriteSheetSourceCatalogConstants.ComposesFromSpriteParts))
        {
            // Every empty frame shows nothing, so the first of those that share a key stands for all of them.
            Add(result, InstalledFixtures.SpriteSheets.Get(id).Frame, catalog.For(id));
        }
        return result;
    }

    /// <summary>A frame's identity across separately loaded sheets.</summary>
    private static FrameKey Key(SpriteSheetFrame frame) => new(frame.Sheet.Name, frame.Corner, frame.Size, frame.Origin);

    /// <summary>A sheet frame's identity: its sheet, rectangle and origin.</summary>
    /// <param name="Sheet">The sheet's installed name.</param>
    /// <param name="Corner">The rectangle's top-left pixel.</param>
    /// <param name="Size">The rectangle's size.</param>
    /// <param name="Origin">The frame's origin.</param>
    private readonly record struct FrameKey(string Sheet, Position16<ImagePixels> Corner, Extent16<ImagePixels> Size, Offset16<ImagePixels> Origin);

    /// <summary>What a sheet frame was composed from.</summary>
    /// <param name="Frame">The composition.</param>
    /// <param name="Page">The OBJ page its characters index.</param>
    /// <param name="PaletteBits">The palette row its family's draws OR into each part's own.</param>
    /// <param name="FramePalette">The palette row of parts that take their draw's, as the frame's draw selects it.</param>
    /// <param name="PartsKeepOwnRows">Whether parts naming their own row keep it under a draw's override.</param>
    /// <param name="ObjectSizes">The object sizes of the screen the frame was composed for.</param>
    private sealed record FrameSource(SpriteFrame Frame, SpriteArtwork Page, int PaletteBits, int? FramePalette, bool PartsKeepOwnRows,
        ScreenObjectSizes ObjectSizes);
}
