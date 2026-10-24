using SuperMetroid.Core.Enemies;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using System.Collections.Immutable;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.VerificationAssert;
using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Enemies.Artwork;

/// <summary>
/// Checks a picture drawn over characters an encounter uploads (#1392): it paints exactly what its frame's parts paint over
/// the room's live OBJ characters, the path such frames drew through before pre-coloured sheets.
/// </summary>
internal static class UploadedPictureChecks
{
    /// <summary>
    /// Asserts <paramref name="pictures"/> paint exactly what <paramref name="parts"/> paint in the runtime's live colours:
    /// every pixel's colour, whether it blends, and its priority, with at least one pixel painted. A sprite pixel's row
    /// decides only whether it blends (#1405), so that is what is compared.
    /// </summary>
    internal static void AssertPaintsLikeParts(SuperMetroidRuntime runtime, ImmutableArray<SpriteDraw> pictures,
        ImmutableArray<CharacterSpriteDraw> parts, string message)
    {
        AssertTrue(pictures.Length > 0 && pictures.All(draw => draw is ImageSpriteDraw), $"{message}: draws from a sheet");
        var cgram = new SnesCgram();
        for (int colour = 0; colour < 256; colour++)
            cgram.SetColor(colour, runtime.Cgram.Colors[colour]);
        int count = SnesPpuLayout.ScreenSize.X * SnesPpuLayout.ScreenSize.Y;
        var expected = new ObjRasterTargets(new Rgba32[count], new byte[count], new byte[count]);
        var actual = new ObjRasterTargets(new Rgba32[count], new byte[count], new byte[count]);
        CharacterSpriteRaster.RenderResolved(parts, cgram, expected);
        SnesObjRenderer.RenderResolved(DrawColours.ResolveAll(pictures, cgram.Colors), actual);
        int painted = 0;
        for (int pixel = 0; pixel < count; pixel++)
        {
            if (expected.Pixels[pixel] != actual.Pixels[pixel] || expected.Priorities[pixel] != actual.Priorities[pixel] ||
                Blending(expected.Palettes[pixel]) != Blending(actual.Palettes[pixel]))
                throw new InvalidOperationException($"{message}: the picture differs from the frame over the live characters at screen pixel " +
                    $"({pixel % SnesPpuLayout.ScreenSize.X},{pixel / SnesPpuLayout.ScreenSize.X}): colour {actual.Pixels[pixel]} for {expected.Pixels[pixel]}, " +
                    $"row {actual.Palettes[pixel]} for {expected.Palettes[pixel]}, priority {actual.Priorities[pixel]} for {expected.Priorities[pixel]}.");
            if (expected.Palettes[pixel] != 0)
                painted++;
        }
        AssertTrue(painted > 0, $"{message}: paints pixels");
    }

    /// <summary>Whether a sprite pixel of row <paramref name="row"/> is drawn, and if so whether it blends.</summary>
    private static int Blending(byte row) => row == SnesObjRenderer.TransparentPriority ? -1 : row >= 4 ? 1 : 0;

    /// <summary>
    /// Asserts each of <paramref name="pictures"/> paints exactly what its frame's parts paint over the runtime's live
    /// characters: its own composition drawn over those characters in place of the page it was composed over.
    /// </summary>
    internal static void AssertPicturesPaintOverLive(SuperMetroidRuntime runtime, ImmutableArray<SpriteDraw> pictures, string message)
    {
        SpriteArtwork live = runtime.Artwork.GameplayObjectPage(runtime.EnemySession.SpriteCharacters);
        AssertPaintsLikeParts(runtime, pictures,
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
