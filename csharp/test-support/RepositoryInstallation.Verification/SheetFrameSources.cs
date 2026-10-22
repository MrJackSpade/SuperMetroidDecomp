using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Artwork;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.TestSupport.RepositoryInstallation.Verification;

/// <summary>
/// Verification access (#1392): the composition each installed Samus and enemy sheet frame was composed from, so a draw of
/// that frame can be compared with cartridge OAM oracles as the parts it shows. Frames are found by sheet, rectangle
/// and origin, since a runtime and the fixtures load the sheets separately.
/// </summary>
internal static class SheetFrameSources
{
    /// <summary>Each projectile sheet frame's composition and the page its characters index.</summary>
    private static readonly Lazy<System.Collections.Concurrent.ConcurrentDictionary<FrameKey, FrameSource>> sources = new(Build);

    /// <summary>Pairs a fixture sheet's frames with what they were composed from.</summary>
    internal static void Register(SpriteSheet sheet, SpriteSheetSources composed)
    {
        Ensure.NotNull(sheet);
        Ensure.NotNull(composed);
        Add(sources.Value, sheet, composed);
    }

    /// <summary>Pairs each of <paramref name="sheet"/>'s frames with what it was composed from; frames already paired keep theirs.</summary>
    private static void Add(System.Collections.Concurrent.ConcurrentDictionary<FrameKey, FrameSource> target, SpriteSheet sheet,
        SpriteSheetSources composed)
    {
        foreach (SheetFrameSource frame in composed.Frames)
        {
            SpriteSheetFrame image = sheet.Frame(frame.Name);
            target.TryAdd(Key(image), new FrameSource(frame.Frame, frame.Characters, composed.PaletteBits, frame.Palette, composed.PartsKeepOwnRows,
                composed.ObjectSizes));
            // Each part, drawn at the frame's origin, is its part alone at its place in the frame, as a frame the
            // object limit cuts short shows it.
            for (int part = 0; part < image.Parts.Count; part++)
                target.TryAdd(Key(image.Parts[part].Image), new FrameSource(
                    new SpriteFrame($"{frame.Name}#{part}", [frame.Frame.Parts[part]]),
                    frame.Characters, composed.PaletteBits, frame.Palette, composed.PartsKeepOwnRows, composed.ObjectSizes));
        }
    }

    /// <summary>The character draw an image draw of a projectile sheet frame shows: its composition over its page, placed and mirrored alike.</summary>
    /// <exception cref="InvalidOperationException">The frame is not a projectile sheet frame.</exception>
    internal static CharacterSpriteDraw CharacterDraw(ImageSpriteDraw draw) => CharacterDraw(draw, null);

    /// <summary>
    /// The character draw an image draw of a sheet frame shows, its composition placed and mirrored alike, over
    /// <paramref name="characters"/> in place of the page it was composed over when given.
    /// </summary>
    /// <exception cref="InvalidOperationException">The frame is not a sheet frame.</exception>
    internal static CharacterSpriteDraw CharacterDraw(ImageSpriteDraw draw, SpriteArtwork? characters)
    {
        Ensure.NotNull(draw);
        FrameSource source = Find(Key(draw.Image)) ?? throw new InvalidOperationException($"{draw} shows a frame no sheet was composed with.");
        SnesTileFlipFlags flip = ((draw.Mirror & SpriteMirror.Horizontal) != 0 ? SnesTileFlipFlags.Horizontal : SnesTileFlipFlags.None) |
            ((draw.Mirror & SpriteMirror.Vertical) != 0 ? SnesTileFlipFlags.Vertical : SnesTileFlipFlags.None);
        // A frontend draw's override places the parts that take their draw's palette in its row and leaves the others.
        if (source.PartsKeepOwnRows && draw.PaletteOverride is int row)
            return new CharacterSpriteDraw(draw.Source, source.Frame, characters ?? source.Page, draw.Origin, flip, row, 0);
        // A family's palette bits, or the draw's override in their place, OR into each part's own row, as the
        // cartridge ORs an enemy's.
        int bits = draw.PaletteOverride ?? source.PaletteBits;
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
    private static readonly Lazy<bool> enemySources = new(() =>
    {
        SuperMetroid.AssetExtraction.Enemies.EnemySheetInputs inputs =
            SuperMetroid.AssetExtraction.Enemies.EnemySheetInputs.Load(RepositoryInstallation.Installation.ContentDirectory);
        foreach (SuperMetroid.Core.Enemies.RoomEnemyGraphicsSetDefinition set in SuperMetroid.Core.Enemies.RoomEnemyGraphicsSetDefinitions.DrawnUnder)
        {
            SuperMetroid.AssetExtraction.Enemies.GraphicsSetSheetSources setSources =
                SuperMetroid.AssetExtraction.Enemies.EnemySheetComposer.Sources(inputs, set);
            foreach ((SuperMetroid.Core.Enemies.EnemyDefinitionId enemy, SpriteSheetSources composed, _) in setSources.Enemies)
                Register(InstalledFixtures.EnemySheets.For(set.Name, enemy), composed);
            foreach ((SuperMetroid.AssetExtraction.Enemies.SpawnedObjectSheetPlan plan, SpriteSheetSources composed, _) in setSources.Spawned)
                Register(InstalledFixtures.EnemySheets.Spawned(set.Name, plan.TileBase).Sheet, composed);
        }
        return true;
    });

    /// <summary>Pairs every installed projectile sheet frame with what it was composed from.</summary>
    private static System.Collections.Concurrent.ConcurrentDictionary<FrameKey, FrameSource> Build()
    {
        var result = new System.Collections.Concurrent.ConcurrentDictionary<FrameKey, FrameSource>();
        var catalog = new SpriteSheetSourceCatalog(RepositoryInstallation.Installation.ContentDirectory);
        // Screen tile sheets hold background characters, not frames composed from sprite parts.
        foreach (SpriteSheetId id in SpriteSheetDefinitions.Sheets.Where(id => id != SpriteSheetId.EscapeTimer && !ScreenTileSheets.Holds(id)))
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
