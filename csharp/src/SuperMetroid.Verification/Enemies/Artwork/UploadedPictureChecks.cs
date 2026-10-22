using System.Collections.Immutable;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.VerificationAssert;

namespace SuperMetroid.Verification.Enemies.Artwork;

/// <summary>
/// Checks a picture drawn over characters an encounter uploads (#1392): it paints exactly what its frame's parts paint over
/// the room's live OBJ characters, the path such frames drew through before pre-coloured sheets.
/// </summary>
internal static class UploadedPictureChecks
{
    /// <summary>
    /// Asserts <paramref name="pictures"/> paint exactly what <paramref name="parts"/> paint in the runtime's live colours:
    /// every pixel's colour, palette row and priority, with at least one pixel painted.
    /// </summary>
    internal static void AssertPaintsLikeParts(SuperMetroidRuntime runtime, ImmutableArray<SpriteDraw> pictures,
        ImmutableArray<SpriteDraw> parts, string message)
    {
        AssertTrue(pictures.Length > 0 && pictures.All(draw => draw is ImageSpriteDraw), $"{message}: draws from a sheet");
        var cgram = new SnesCgram();
        for (int colour = 0; colour < 256; colour++)
            cgram.SetColor(colour, runtime.Cgram.Colors[colour]);
        int count = SnesPpuLayout.ScreenSize.X * SnesPpuLayout.ScreenSize.Y;
        var expected = new ObjRasterTargets(new Rgba32[count], new byte[count], new byte[count]);
        var actual = new ObjRasterTargets(new Rgba32[count], new byte[count], new byte[count]);
        SnesObjRenderer.RenderResolved(parts, cgram, expected);
        SnesObjRenderer.RenderResolved(pictures, cgram, actual);
        int painted = 0;
        for (int pixel = 0; pixel < count; pixel++)
        {
            if (expected.Pixels[pixel] != actual.Pixels[pixel] || expected.Priorities[pixel] != actual.Priorities[pixel] ||
                expected.Palettes[pixel] != actual.Palettes[pixel])
                throw new InvalidOperationException($"{message}: the picture differs from the frame over the live characters at screen pixel " +
                    $"({pixel % SnesPpuLayout.ScreenSize.X},{pixel / SnesPpuLayout.ScreenSize.X}): colour {actual.Pixels[pixel]} for {expected.Pixels[pixel]}, " +
                    $"row {actual.Palettes[pixel]} for {expected.Palettes[pixel]}, priority {actual.Priorities[pixel]} for {expected.Priorities[pixel]}.");
            if (expected.Palettes[pixel] != 0)
                painted++;
        }
        AssertTrue(painted > 0, $"{message}: paints pixels");
    }

    /// <summary>
    /// Makes <paramref name="uploads"/> over the runtime's characters in order and asserts, before the first and after
    /// each, that the draw selects for every region the loaded set's uploads make the upload made last over it, or the
    /// room load's characters before any is, and that the live characters there are exactly the bytes that picture
    /// composes from.
    /// </summary>
    internal static void AssertRegionsFollowUploads(SuperMetroidRuntime runtime, IReadOnlyList<RuntimeCharacterUpload> uploads, string message)
    {
        IEnemySheetSource sheets = runtime.EnemySession.Sheets ?? throw new InvalidOperationException("The runtime loads the enemy sheets.");
        IReadOnlyList<RuntimeCharacterUpload> setUploads = sheets.Uploads.Of(runtime.Enemies.GraphicsSet.LoadedSet);
        IReadOnlyList<CharacterRegion> regions = RuntimeCharacterUploadCatalog.Regions(setUploads);
        AssertTrue(regions.Count > 0, $"{message}: the loaded set's encounter uploads over its characters");
        byte[] loaded = runtime.Artwork.GameplayObjectPage(runtime.EnemySession.ObjCharacters).Characters.ToArray();
        var made = new List<RuntimeCharacterUpload>();
        for (int step = 0; step <= uploads.Count; step++)
        {
            if (step > 0)
            {
                runtime.EnemySession.ObjCharacters.Record(uploads[step - 1]);
                made.Add(uploads[step - 1]);
            }
            ReadOnlySpan<byte> live = runtime.Artwork.GameplayObjectPage(runtime.EnemySession.ObjCharacters).Characters;
            foreach (CharacterRegion region in regions)
            {
                RuntimeCharacterUpload? expected = made.LastOrDefault(upload => RuntimeCharacterUploadCatalog.Over([upload], region).Any());
                AssertEqual(expected, sheets.Uploads.Showing(runtime.EnemySession.ObjCharacters, setUploads, region).Upload,
                    $"{message}: after {step} uploads the draw selects the upload made last over table byte {region.FirstByte:X4}");
                ReadOnlySpan<byte> composed = expected is null ? loaded.AsSpan(region.FirstByte, region.ByteCount)
                    : expected.Source.Resolve(runtime.Artwork).Span.Slice(region.FirstByte - expected.FirstByte, region.ByteCount);
                AssertTrue(live.Slice(region.FirstByte, region.ByteCount).SequenceEqual(composed),
                    $"{message}: after {step} uploads the live characters at table byte {region.FirstByte:X4} are those its picture composes from");
            }
        }
    }
}
