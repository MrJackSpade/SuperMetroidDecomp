using SuperMetroid.Core.Enemies;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using System.Collections.Immutable;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.VerificationAssert;
using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Enemies.Artwork;

/// <summary>
/// Checks a picture drawn over characters an encounter uploads (#1392): it shows exactly the inks its frame's parts show
/// over the room's live OBJ characters, the path such frames drew through before pre-coloured sheets.
/// </summary>
internal static class UploadedPictureChecks
{
    /// <summary>
    /// Asserts <paramref name="pictures"/> show exactly the inks <paramref name="parts"/> show over their characters: at
    /// every on-screen pixel the same palette entry, which decides the colour and whether it blends (#1405), and the same
    /// priority, with at least one pixel inked.
    /// </summary>
    internal static void AssertPaintsLikeParts(ImmutableArray<SpriteDraw> pictures, ImmutableArray<CharacterSpriteDraw> parts,
        string message)
    {
        AssertTrue(pictures.Length > 0 && pictures.All(draw => draw is ImageSpriteDraw), $"{message}: draws from a sheet");
        AssertSameInks([.. pictures.Cast<ImageSpriteDraw>()], parts, message);
    }

    /// <summary>
    /// Asserts <paramref name="images"/> show exactly the inks <paramref name="parts"/> show over their characters at every
    /// on-screen pixel: the frontmost inked pixel's palette entry and priority, with at least one pixel inked.
    /// </summary>
    internal static void AssertSameInks(IReadOnlyList<ImageSpriteDraw> images, IReadOnlyList<CharacterSpriteDraw> parts, string message)
    {
        Dictionary<(int X, int Y), (int Entry, int Priority)> expected = [], actual = [];
        foreach (CharacterSpriteDraw draw in parts)
        {
            for (int part = 0; part < draw.Frame.Parts.Count; part++)
            {
                SpriteScreenPart placed = draw.Place(part);
                for (ushort y = 0; y < placed.Size.Y; y++)
                for (ushort x = 0; x < placed.Size.X; x++)
                {
                    int character = placed.CharacterAt(new(x, y), out Position16<CharacterPixels> within);
                    byte colour = draw.Artwork.ColorIndex(character, within);
                    if (colour != 0)
                        Ink(expected, placed.Position.X + x, placed.Position.Y + y, placed.Palette * 16 + colour, placed.Priority);
                }
            }
        }
        foreach (ImageSpriteDraw image in images)
        {
            for (ushort y = 0; y < image.Image.Size.Y; y++)
            for (ushort x = 0; x < image.Image.Size.X; x++)
            {
                var cell = new Position16<ScreenPixels>(x, y);
                int entry = image.PaletteIndexAt(cell);
                if (entry != 0)
                    Ink(actual, image.Corner.X + x, image.Corner.Y + y, entry, image.PriorityAt(cell));
            }
        }
        foreach ((int X, int Y) pixel in expected.Keys.Union(actual.Keys))
        {
            (int Entry, int Priority)? want = expected.TryGetValue(pixel, out var e) ? e : null, got = actual.TryGetValue(pixel, out var g) ? g : null;
            if (want != got)
                throw new InvalidOperationException($"{message}: the picture differs from the frame over the live characters at screen pixel " +
                    $"({pixel.X},{pixel.Y}): ink {got?.ToString() ?? "none"} for {want?.ToString() ?? "none"}.");
        }
        AssertTrue(expected.Count > 0, $"{message}: paints pixels");

        // The frontmost ink at an on-screen pixel; a later, further-back draw does not replace it.
        static void Ink(Dictionary<(int X, int Y), (int Entry, int Priority)> inks, int x, int y, int entry, int priority)
        {
            if (x is >= 0 and < SnesPpuLayout.ScreenWidthPixels && y is >= 0 and < SnesPpuLayout.ScreenHeightPixels)
                inks.TryAdd((x, y), (entry, priority));
        }
    }

    /// <summary>
    /// Asserts each of <paramref name="pictures"/> paints exactly what its frame's parts paint over the runtime's live
    /// characters: its own composition drawn over those characters in place of the page it was composed over.
    /// </summary>
    internal static void AssertPicturesPaintOverLive(SuperMetroidRuntime runtime, ImmutableArray<SpriteDraw> pictures, string message)
    {
        SpriteArtwork live = runtime.Artwork.GameplayObjectPage(runtime.EnemySession.SpriteCharacters);
        AssertPaintsLikeParts(pictures,
            [.. pictures.Select(draw => SheetFrameSources.CharacterDraw(AssertIs<ImageSpriteDraw>(draw, $"{message}: draws from a sheet"), live))],
            message);
    }

    /// <summary>
    /// The pictures <paramref name="frame"/> draws as, front first, once <paramref name="made"/> are made: the frame whole,
    /// or each of its <paramref name="layers"/> showing in each region it reads the upload made last over it, or the
    /// set's own characters before any is.
    /// </summary>
    internal static IReadOnlyList<string> ExpectedPictures(IEnemySheetSource sheets, RoomEnemyGraphicsSetDefinition set, string frame,
        IReadOnlyList<CharacterRegion[]> layers, IReadOnlyList<SpriteCharacterPlacement> made)
    {
        SetCharacterCoverage coverage = sheets.Coverage(set);
        return layers.Count == 0 ? [frame] : [.. layers.Select((regions, layer) => sheets.Encounters.PictureName(frame, layer,
            [.. regions.Select(region => new CharacterRegionContent(region,
                made.LastOrDefault(upload => coverage.Covers(upload.Source, region))))]))];
    }

    /// <summary>Asserts <paramref name="drawn"/> are exactly <paramref name="sheet"/>'s pictures named <paramref name="expected"/>, in order.</summary>
    internal static void AssertDrawnPictures(SpriteSheet sheet, IReadOnlyList<string> expected, ImmutableArray<SpriteDraw> drawn, string message)
    {
        AssertEqual(expected.Count, drawn.Length, $"{message}: draws one image for each layer");
        for (int layer = 0; layer < expected.Count; layer++)
            AssertTrue(drawn[layer] is ImageSpriteDraw image && ReferenceEquals(sheet.Frame(expected[layer]), image.Image),
                $"{message}: layer {layer} shows {expected[layer]}");
    }

    /// <summary>
    /// Makes <paramref name="uploads"/> over the runtime's characters in order and asserts, before the first and after
    /// each, that the draw selects for every region the loaded set's uploads make the upload made last over it, or the
    /// room load's characters before any is, and that the live characters there are exactly the bytes that picture
    /// composes from.
    /// </summary>
    internal static void AssertRegionsFollowUploads(SuperMetroidRuntime runtime, IReadOnlyList<SpriteCharacterPlacement> uploads, string message)
    {
        IEnemySheetSource sheets = runtime.EnemySession.Sheets ?? throw new InvalidOperationException("The runtime loads the enemy sheets.");
        RoomEnemyGraphicsSetDefinition set = runtime.Enemies.GraphicsSet.LoadedSet;
        IReadOnlyList<SpriteCharacterPlacement> setUploads = sheets.Encounters.Of(set);
        SetCharacterCoverage coverage = sheets.Coverage(set);
        SetRegions ranges = InstalledSetRegions.Of(set);
        IReadOnlyList<CharacterRegion> regions = [.. coverage.Regions.Where(region => setUploads.Any(upload => coverage.Covers(upload.Source, region)))];
        AssertTrue(regions.Count > 0, $"{message}: the loaded set's encounter uploads over its characters");
        byte[] loaded = runtime.Artwork.GameplayObjectPage(runtime.EnemySession.SpriteCharacters).Characters.ToArray();
        var made = new List<SpriteCharacterPlacement>();
        for (int step = 0; step <= uploads.Count; step++)
        {
            if (step > 0)
            {
                runtime.EnemySession.SpriteCharacters.Record(uploads[step - 1]);
                made.Add(uploads[step - 1]);
            }
            ReadOnlySpan<byte> live = runtime.Artwork.GameplayObjectPage(runtime.EnemySession.SpriteCharacters).Characters;
            foreach (CharacterRegion region in regions)
            {
                SpriteCharacterPlacement? expected = made.LastOrDefault(upload => coverage.Covers(upload.Source, region));
                AssertEqual(expected, sheets.Encounters.Showing(runtime.EnemySession.SpriteCharacters, setUploads, region, new SuperMetroid.Core.Assets.ArtworkFrameSelections(), coverage).Placement,
                    $"{message}: after {step} uploads the draw selects the upload made last over region {region.Name}");
                CharacterRange range = ranges.RangeOf(region);
                ReadOnlySpan<byte> composed = expected is null ? loaded.AsSpan(range.FirstByte, range.ByteCount)
                    : expected.Source.Resolve(runtime.Artwork).Span.Slice(range.FirstByte - expected.FirstByte, range.ByteCount);
                AssertTrue(live.Slice(range.FirstByte, range.ByteCount).SequenceEqual(composed),
                    $"{message}: after {step} uploads the live characters of region {region.Name} are those its picture composes from");
            }
        }
    }
}
