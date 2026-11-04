using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Enemies;

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
        Add(sources.Value, sheet, composed);
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
    /// Pairs each of <paramref name="sheet"/>'s frames with what it was composed from. Frames whose pictures the sheet lays
    /// out once share one rectangle, the first frame's composition standing for all of them over their own pages, which
    /// show the same picture; the others' differing compositions are kept, since over another page they may not.
    /// </summary>
    private static void Add(System.Collections.Concurrent.ConcurrentDictionary<FrameKey, FrameSource> target, SpriteSheet sheet,
        SpriteSheetSources composed)
    {
        foreach (SheetFrameSource frame in composed.Frames)
        {
            SpriteSheetFrame image = sheet.Frame(frame.Name);
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
        if (characters is not null && alternates.TryGetValue(key, out FrameSource[]? others))
        {
            Dictionary<(int X, int Y), int> inks = Inks(built);
            foreach (FrameSource other in others)
            {
                if (!Inks(Build(draw, other, characters)).OrderBy(pixel => pixel.Key).SequenceEqual(inks.OrderBy(pixel => pixel.Key)))
                    throw new InvalidOperationException(
                        $"{draw} shows a picture sheet {draw.Image.Sheet.Name} lays out once for frames {source.Frame.Name} and {other.Frame.Name}, whose compositions show different pictures over the given characters.");
            }
        }
        return built;
    }

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
        TileFlip flip = ((draw.Mirror & SpriteMirror.Horizontal) != 0 ? TileFlip.Horizontal : TileFlip.None) |
            ((draw.Mirror & SpriteMirror.Vertical) != 0 ? TileFlip.Vertical : TileFlip.None);
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
    /// A frame's composition. Installed enemy sheets are registered on the first frame no other sheet holds, since
    /// composing every graphics set's members takes a while and only enemy draws need them.
    /// </summary>
    private static FrameSource? Find(FrameKey key)
    {
        if (sources.Value.TryGetValue(key, out FrameSource? found))
            return found;
        _ = enemySources.Value;
        return sources.Value.TryGetValue(key, out found) ? found : null;
    }

    /// <summary>Registers every installed enemy sheet frame with what it was composed from, once.</summary>
    private static readonly Lazy<bool> enemySources = new(() => SharedFixtureTiming.Load("Fixture: enemy sheet frame sources", () =>
    {
        SuperMetroid.AssetExtraction.Enemies.EnemySheetInputs inputs =
            SuperMetroid.AssetExtraction.Enemies.EnemySheetInputs.Load(RepositoryInstallation.Installation.ContentDirectory);
        foreach (SuperMetroid.Core.Enemies.RoomEnemyGraphicsSetDefinition set in RoomEnemyGraphicsSetLists.DrawnUnder)
        {
            SuperMetroid.AssetExtraction.Enemies.GraphicsSetSheetSources setSources =
                SuperMetroid.AssetExtraction.Enemies.EnemySheetComposer.Sources(inputs, set);
            foreach ((SuperMetroid.Core.Enemies.EnemyDefinitionId enemy, SpriteSheetSources composed, _, _) in setSources.Enemies)
                Register(InstalledFixtures.EnemySheets.For(set.Name, enemy), composed);
            foreach ((SuperMetroid.AssetExtraction.Enemies.SpawnedObjectSheetPlan plan, SpriteSheetSources composed, _, _) in setSources.Spawned)
                Register(InstalledFixtures.EnemySheets.Spawned(set.Name, plan.Owner).Sheet, composed);
        }
        return true;
    }));

    /// <summary>Pairs every installed projectile sheet frame with what it was composed from.</summary>
    private static System.Collections.Concurrent.ConcurrentDictionary<FrameKey, FrameSource> Build()
    {
        var result = new System.Collections.Concurrent.ConcurrentDictionary<FrameKey, FrameSource>();
        var catalog = new SpriteSheetSourceCatalog(RepositoryInstallation.Installation.ContentDirectory);
        // Only sheets composed from sprite parts have frames to pair; the picture, page and floor sheets are painted directly.
        foreach (SpriteSheetId id in SpriteSheetDefinitions.Sheets.Where(SpriteSheetSourceCatalogConstants.ComposesFromSpriteParts))
        {
            // Every empty frame shows nothing, so the first of those that share a key stands for all of them.
            Add(result, InstalledFixtures.SpriteSheets.Get(id), catalog.For(id));
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
