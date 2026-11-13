using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.TestSupport.FrameRendering;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Fx;

/// <summary>
/// Confirms #1706's change of how room liquids and atmospheres reach the display: a liquid published as its surface's
/// screen line with its surface strip and repeating body, an atmosphere as a repeating picture with its scroll, show on
/// every screen line the rows of the effect's transfer the overlay's two-page tilemap and its computed vertical scroll
/// showed before. The superseded page geometry is reproduced here once, as the oracle, from the code #1706 deleted; nothing
/// else keeps it.
/// </summary>
internal static class FxOverlayPlacementIdentityVerification
{
    /// <summary>The overlay pixel row a liquid's page's top edge lay on; the rows above it showed nothing (deleted with #1706).</summary>
    private const int PageLiquidTop = 248;

    /// <summary>A liquid's overlay height before it repeated down: the band of nothing and the page below it (deleted with #1706).</summary>
    private const int PageLiquidRepeat = 512;

    /// <summary>The page row an atmosphere's overlay began with (deleted with #1706).</summary>
    private const int PageAtmosphereFirstRow = 8;

    /// <summary>An atmosphere's overlay height before it repeated down (deleted with #1706).</summary>
    private const int PageAtmosphereRepeat = 256;

    /// <summary>The overlay's lines in its surface band, above the liquid's body (deleted with #1706).</summary>
    private const int PageSurfaceBandLines = 256;

    /// <summary>The lines in which the liquid's body below its surface repeated (deleted with #1706).</summary>
    private const int PageBodyRepeatLines = 32;

    /// <summary>Words in one row of an effect's transfer.</summary>
    private const int RowWords = RoomFxLayer3TilemapFormat.WidthInTiles;

    /// <summary>Pixels down one row of an effect's transfer.</summary>
    private const int RowPixels = 8;

    /// <summary>The lines below the HUD the overlay draws on.</summary>
    private const int FirstLine = DisplayScreen.HudHeight;

    /// <summary>
    /// For water, lava, acid and the Tourian entrance statue's water at every surface line from 300 above the screen to 300
    /// below it, for water and lava with each tide through whole tide cycles, one near the room's top so the surface leaves
    /// the room, and for rain, fog and spores through drifting scrolls, every gameplay line shows the same row of the
    /// effect's transfer, at the same horizontal scroll, as the page geometry showed, or nothing where it showed nothing.
    /// </summary>
    internal static void VerifyPublishedOverlayShowsThePageRows()
    {
        var pages = new PaintedTransfers(InstalledFixtures.ImportMaps.RoomFxPages);
        var session = new RoomFxSession();
        var frames = new ArtworkFrameSelections();
        int lines = 0, cases = 0;
        foreach (RoomFxType type in new[] { RoomFxType.Water, RoomFxType.Lava, RoomFxType.Acid, RoomFxType.TourianEntranceStatue })
        {
            // A still surface at line 600, the camera swept so the surface lies from 300 lines above the screen's top to 300
            // below it.
            RoomFxRecordDefinition record = Still(RetailRecord(type)) with { BaseYPosition = 600 };
            RoomFxOverlayState fx = RoomFxOverlayState.ForDefinition(session, record, 0);
            for (int surfaceLine = -300; surfaceLine <= 300; surfaceLine++)
            {
                var camera = new Position16<RoomPixels>(surfaceLine * 3 & 0x1ff, 600 - surfaceLine);
                Step(fx, frames, camera);
                lines += Compare(fx, pages, camera.Y, $"{type} with its surface {surfaceLine} lines below the screen's top");
                cases++;
            }
        }
        foreach ((RoomFxType type, RoomLiquid liquid) in new (RoomFxType, RoomLiquid)[]
        {
            (RoomFxType.Water, new RoomWater(LiquidTide.Large, SurfaceDrift: true, BackgroundWave: true, IgnoresSurfaceUntilEnabled: false)),
            (RoomFxType.Water, new RoomWater(LiquidTide.Small, SurfaceDrift: false, BackgroundWave: false, IgnoresSurfaceUntilEnabled: false)),
            (RoomFxType.Lava, new RoomLavaOrAcid(LiquidTide.Large, HeatHazeDirection.Vertical)),
            (RoomFxType.Lava, new RoomLavaOrAcid(LiquidTide.Small, HeatHazeDirection.Horizontal)),
        })
        foreach (int baseLine in new[] { 4, 160 })
        foreach (int cameraY in new[] { 0, 150 })
        {
            // The tide carries the surface through both extremes; a surface four lines below the room's top leaves the room.
            RoomFxRecordDefinition record = Still(RetailRecord(type)) with { BaseYPosition = baseLine, Liquid = liquid };
            RoomFxOverlayState fx = RoomFxOverlayState.ForDefinition(session, record, 0);
            var camera = new Position16<RoomPixels>(0, cameraY);
            int lowest = int.MaxValue, highest = int.MinValue;
            for (int frame = 0; frame < 700; frame++)
            {
                Step(fx, frames, camera);
                if (fx.Surface is int surface)
                {
                    lowest = Math.Min(lowest, surface);
                    highest = Math.Max(highest, surface);
                }
                lines += Compare(fx, pages, cameraY, $"{type} {liquid.Tide} tide from line {baseLine}, frame {frame}, camera {cameraY}");
                cases++;
            }
            AssertTrue(highest > lowest, $"{type}'s {liquid.Tide} tide moves the surface");
        }
        foreach (RoomFxType type in new[] { RoomFxType.Rain, RoomFxType.Fog, RoomFxType.Spores })
        {
            RoomFxOverlayState fx = RoomFxOverlayState.ForDefinition(session, RetailRecord(type), 0x1234);
            for (int frame = 0; frame < 700; frame++)
            {
                var camera = new Position16<RoomPixels>(frame * 5 & 0x7ff, frame * 7 & 0x7ff);
                Step(fx, frames, camera);
                lines += Compare(fx, pages, camera.Y, $"{type}, frame {frame}");
                cases++;
            }
        }

        // A liquid last placed while its surface lay above the room's top showed nothing, even once the surface returned.
        RoomFxOverlayState placedAbove = RoomFxOverlayState.ForDefinition(session, Still(RetailRecord(RoomFxType.Water)) with { BaseYPosition = 100 }, 0);
        Step(placedAbove, frames, new Position16<RoomPixels>(0, 0));
        RoomFxOverlayRenderSnapshot returned = (placedAbove.CaptureForDisplay() ?? throw new InvalidOperationException("Water publishes its effect."))
            with { LiquidSurfaceLine = null };
        AssertTrue(Enumerable.Range(FirstLine, DisplayScreen.Height - FirstLine).All(y => OldRow(returned, pages.Pages, stepSurface: -1, cameraY: 0, y) is null),
            "the page geometry showed nothing of a liquid placed above the room");
        AssertTrue(GameplayEffectCapture.CaptureRoomFxOverlay(returned, pages.Of(RoomFxType.Water).Artwork) is { Picture: null },
            "a liquid placed above the room publishes an overlay showing nothing");
        Console.WriteLine($"  Effect overlay placement: {cases} placements, {lines} lines show the page geometry's rows of every transfer.");
    }

    /// <summary>
    /// The backend draws a liquid as its description says (#1706): with its strip shown, each line above the strip as the
    /// backdrop alone, the strip's lines as the strip repeating from the strip's top line and the lines below as the body
    /// repeating from the strip's last line; with its strip not shown, every line as the body at that place in its repeat.
    /// Each expected line is drawn by the Direct3D 11 renderer as an atmosphere, whose drawing the render checks confirm.
    /// </summary>
    internal static void VerifyBackendDrawsLiquidOverlay()
    {
        var random = new Random(1706);
        var vram = new SnesVram();
        byte[] bytes = new byte[SnesVram.ByteCount]; random.NextBytes(bytes); vram.LoadBytes(0, bytes);
        var cgram = new SnesCgram();
        for (int i = 0; i < SnesCgram.ColorCount; i++) cgram.SetColor(i, RgbColor.FromWord(checked((ushort)random.Next(32768))));
        var oam = new OamBuffer(); oam.BeginFrame(); oam.FinalizeFrame();
        FramePaint memory = FramePaintConstants.CaptureGameplay(vram, cgram, oam, 3);
        // The seeded rows repeat nowhere, so the liquid is cut where water's transfer is.
        LiquidArtwork liquid = FxOverlayFixtures.ArtworkFromVram(RoomFxType.Water, vram.Bytes, cgram.Colors,
            liquidCut: new LiquidTransferRows(BodyFirstRow: 5, BodyRows: 4)).Shown as LiquidArtwork
            ?? throw new InvalidOperationException("Water's artwork is a liquid.");
        var across = new SampledScrollWave(77, [.. Enumerable.Range(0, 16).Select(sample => sample - 8)], 256, 3, 120, 224);
        RgbaPixel[] backdrop = DisplayRendering.Render(new LayeredScene(memory, [], 15));
        int drawn = 0;
        var wrong = new List<string>();
        foreach (int surfaceLine in new[] { -300, -41, -1, 0, 1, 7, 8, 40, 100, 186, 223, 224, 230, 231, 300 })
        {
            var overlay = new LiquidOverlay(liquid, surfaceLine);
            RgbaPixel[] actual = Render(memory, overlay, across);
            int top = overlay.SurfaceTopLine, bodyTop = top + liquid.SurfaceSize.Y;
            RgbaPixel[] strip = Render(memory, new AtmosphereOverlay(new AtmosphereArtwork(liquid.Surface), ScrollBands.Uniform(-top)), across);
            RgbaPixel[] body = Render(memory, new AtmosphereOverlay(new AtmosphereArtwork(liquid.Body), ScrollBands.Uniform(-bodyTop)), across);
            int mismatched = 0;
            string? first = null;
            for (int y = FirstLine; y < DisplayScreen.Height; y++)
            {
                RgbaPixel[] expected = !overlay.ShowsSurface || y >= bodyTop ? body : y >= top ? strip : backdrop;
                if (Line(actual, y).SequenceEqual(Line(expected, y)))
                    continue;
                mismatched++;
                int line = y;
                int pixels = Enumerable.Range(0, DisplayScreen.Width).Count(x => actual[line * DisplayScreen.Width + x] != expected[line * DisplayScreen.Width + x]);
                first ??= $"line {y} ({(ReferenceEquals(expected, body) ? "body" : ReferenceEquals(expected, strip) ? "strip" : "nothing")} expected, {pixels} pixels differ; "
                    + $"actual is {(Line(actual, y).SequenceEqual(Line(backdrop, y)) ? "nothing" : Line(actual, y).SequenceEqual(Line(strip, y)) ? "the strip" : Line(actual, y).SequenceEqual(Line(body, y)) ? "the body" : "none of them")})";
            }
            if (mismatched > 0)
                wrong.Add($"surface line {surfaceLine} (strip from {top}, body from {bodyTop}): {mismatched} lines differ, first {first}");
            drawn++;
        }
        AssertTrue(wrong.Count == 0, $"the backend draws {wrong.Count} liquid placements otherwise than described:{Environment.NewLine}{string.Join(Environment.NewLine, wrong)}");
        Console.WriteLine($"  Effect overlay placement: the backend draws {drawn} liquid placements line by line as described.");
    }

    /// <summary>Screen line <paramref name="y"/> of <paramref name="frame"/>.</summary>
    private static ReadOnlySpan<RgbaPixel> Line(RgbaPixel[] frame, int y) => frame.AsSpan(y * DisplayScreen.Width, DisplayScreen.Width);

    /// <summary>The frame the renderer draws of <paramref name="picture"/> alone, added below the HUD, scrolled across by <paramref name="across"/>.</summary>
    private static RgbaPixel[] Render(FramePaint memory, OverlayPicture picture, LineScroll across) =>
        DisplayRendering.Render(new LayeredScene(memory,
            new DisplayLayer[] { new OverlayBlendLayer(picture, FirstLine, BlendOperation.Add, across) }, 15));

    /// <summary>The first retail record of effect <paramref name="type"/> for its room's default entry.</summary>
    private static RoomFxRecordDefinition RetailRecord(RoomFxType type) =>
        RoomFxDefinitions.AllRecords.First(candidate => candidate.Type == type);

    /// <summary><paramref name="record"/> with its liquid neither rising nor falling.</summary>
    private static RoomFxRecordDefinition Still(RoomFxRecordDefinition record) =>
        record with { TargetYPosition = null, YVelocity = default };

    /// <summary>Runs one effect step of <paramref name="fx"/> under <paramref name="camera"/>.</summary>
    private static void Step(RoomFxOverlayState fx, ArtworkFrameSelections frames, Position16<RoomPixels> camera) =>
        fx.Step(frames, new RoomFxOverlayFrame
        {
            Camera = camera,
            TimeIsFrozen = false,
            EarthquakeSoundDelay = SuperMetroid.Core.Compatibility.PowerOnEarthquakeSoundDelay.OneFrame,
        });

    /// <summary>Each effect's transfer words and the artwork it shows of them, cut as the import cuts them, painted once from blank characters.</summary>
    private sealed class PaintedTransfers(RoomFxLayer3Pages pages)
    {
        /// <summary>The painted transfers by effect.</summary>
        private readonly Dictionary<RoomFxType, (FxOverlayArtwork Artwork, ushort[] Transfer)> painted = [];

        /// <summary>The import's transfers.</summary>
        internal RoomFxLayer3Pages Pages { get; } = pages;

        /// <summary>The artwork effect <paramref name="type"/> shows and its transfer's words, painted on first use.</summary>
        internal (FxOverlayArtwork Artwork, ushort[] Transfer) Of(RoomFxType type)
        {
            if (!painted.TryGetValue(type, out (FxOverlayArtwork, ushort[]) shown))
            {
                shown = (FxOverlayFixtures.ArtworkOf(FxOverlayPlane.Of(type), Pages, new byte[SnesVram.ByteCount], new RgbColor[SnesCgram.ColorCount]),
                    BackgroundTileWords.ToWords(Pages.Resolve(type).Span));
                painted.Add(type, shown);
            }
            return shown;
        }
    }

    /// <summary>
    /// Compares, on every gameplay line, the transfer row and horizontal scroll the published overlay of <paramref name="fx"/>
    /// shows with those the page geometry showed of the same step; returns the lines compared.
    /// </summary>
    private static int Compare(RoomFxOverlayState fx, PaintedTransfers pages, int cameraY, string place)
    {
        RoomFxOverlayRenderSnapshot snapshot = fx.CaptureForDisplay() ?? throw new InvalidOperationException($"{place}: the effect publishes nothing.");
        (FxOverlayArtwork artwork, ushort[] transfer) = pages.Of(fx.Type);
        OverlayBlendLayer? layer = GameplayEffectCapture.CaptureRoomFxOverlay(snapshot, artwork);
        LineScroll? oldAcross = OldHorizontal(snapshot);
        AssertEqual(oldAcross is null, layer is null, $"{place}: the overlay is captured where the page geometry captured it");
        if (layer is null)
            return 0;
        AssertTrue(layer.FirstLine == FirstLine && layer.Operation == (snapshot.Blend ?? throw new InvalidOperationException($"{place}: no blend.")).Operation,
            $"{place}: the overlay blends from the line and by the operation it did");
        // Each message is built only for a failing line.
        for (int y = FirstLine; y < DisplayScreen.Height; y++)
        {
            ushort oldScroll = ScrollShapeLines.Scroll(oldAcross, y), newScroll = ScrollShapeLines.Scroll(layer.Horizontal, y);
            if (oldScroll != newScroll)
                AssertTrue(false, $"{place}: line {y} scrolls across by {newScroll} where it scrolled by {oldScroll}");
            int? before = OldRow(snapshot, pages.Pages, fx.Surface, cameraY, y);
            int? now = NewRow(layer.Picture, transfer.Length / RowWords * RowPixels, y);
            if ((before is null) != (now is null))
                AssertTrue(false, $"{place}: line {y} shows transfer row {now?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "nothing"} where it showed {before?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "nothing"}");
            if (before is int oldRow && now is int newRow && !(oldRow % RowPixels == newRow % RowPixels
                    && transfer.AsSpan(oldRow / RowPixels * RowWords, RowWords).SequenceEqual(transfer.AsSpan(newRow / RowPixels * RowWords, RowWords))))
                AssertTrue(false, $"{place}: line {y} shows transfer row {newRow}, which does not picture transfer row {oldRow}'s words and line");
        }
        return DisplayScreen.Height - FirstLine;
    }

    /// <summary>
    /// The transfer row the page geometry showed on screen line <paramref name="y"/> (#1514, deleted with #1706), or null for
    /// nothing: a liquid's page lay below a band of nothing, repeating every two pages, scrolled from the surface placed by
    /// the step's <paramref name="stepSurface"/> and the camera's top; an atmosphere's page showed from its eighth row.
    /// </summary>
    private static int? OldRow(RoomFxOverlayRenderSnapshot fx, RoomFxLayer3Pages pages, int? stepSurface, int cameraY, int y)
    {
        int pageHeight = pages.Resolve(fx.Type).Length / RowWords * RowPixels;
        if (!RoomFxTypes.ShowsLiquid(fx.Type))
        {
            int row = ((y + fx.Scroll.Y) & 0xffff) % PageAtmosphereRepeat;
            return row < pageHeight - PageAtmosphereFirstRow ? PageAtmosphereFirstRow + row : null;
        }
        if (fx.Surface is not >= 0 || fx.WaterSurfaceScreenY is not int surfaceLine)
            return null;
        int firstSurfaceLine = surfaceLine - (ScreenGeometry.BackgroundTileSizePixels - 1);
        int scroll = ScrollShapeLines.Scroll(ScrollBands.Split(0, firstSurfaceLine, OldVerticalScroll(stepSurface, cameraY)), y);
        int offset = ((y + scroll) & 0xffff) % PageLiquidRepeat - PageLiquidTop;
        return offset >= 0 && offset < pageHeight ? offset : null;
    }

    /// <summary>The overlay's vertical scroll the step computed from its liquid surface and the camera's top (deleted with #1706).</summary>
    private static int OldVerticalScroll(int? surfaceY, int cameraY)
    {
        if (surfaceY is not int line || line < 0)
            return 0;
        int belowCameraTop = CoordinateAxis.Difference(line, cameraY);
        if (belowCameraTop <= 0)
            return PageSurfaceBandLines + (-belowCameraTop + PageBodyRepeatLines - 1) % PageBodyRepeatLines;
        return belowCameraTop < PageSurfaceBandLines ? PageSurfaceBandLines - 1 - belowCameraTop : 0;
    }

    /// <summary>The overlay's horizontal scroll the page geometry's capture described, or null where it captured no overlay.</summary>
    private static LineScroll? OldHorizontal(RoomFxOverlayRenderSnapshot fx)
    {
        if (!RoomFxTypes.ShowsLiquid(fx.Type))
            return ScrollBands.Uniform(fx.Scroll.X);
        if (fx.Surface is not >= 0 || fx.WaterSurfaceScreenY is not int surfaceLine)
            return null;
        int belowSurface = surfaceLine + 1;
        return RoomFxTypes.UsesWater(fx.Type)
            ? new SampledScrollWave(fx.Scroll.X, LiquidEffects.WaterWaveform, EightEight.FromWhole(1),
                SampledScrollWave.PhaseWhere(-fx.WaterOverlayWavePhase, belowSurface, LiquidEffects.WaterWaveform.Length),
                Math.Clamp(belowSurface, FirstLine, DisplayScreen.Height), DisplayScreen.Height)
            : ScrollBands.Uniform(fx.Scroll.X);
    }

    /// <summary>
    /// The transfer row <paramref name="picture"/> shows on screen line <paramref name="y"/> as its description says, or null
    /// for nothing: a liquid's strip from its top line and body below, or the body alone; an atmosphere's picture, the
    /// transfer's bottom rows, repeating at its height.
    /// </summary>
    private static int? NewRow(OverlayPicture? picture, int transferPixels, int y)
    {
        switch (picture)
        {
            case null:
                return null;
            case AtmosphereOverlay atmosphere:
            {
                int height = atmosphere.Artwork.Size.Y;
                return transferPixels - height + ((y + ScrollShapeLines.Scroll(atmosphere.Vertical, y)) & 0xffff) % height;
            }
            case LiquidOverlay liquid:
            {
                int fromTop = y - liquid.SurfaceTopLine;
                int strip = liquid.Artwork.SurfaceSize.Y, body = liquid.Artwork.BodySize.Y;
                if (liquid.ShowsSurface && fromTop < 0)
                    return null;
                if (liquid.ShowsSurface && fromTop < strip)
                    return fromTop;
                int bodyRow = (fromTop - strip) % body;
                return strip + (bodyRow < 0 ? bodyRow + body : bodyRow);
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(picture), picture, "Undefined effect overlay picture.");
        }
    }

    /// <summary>Runs the effect overlay placement checks.</summary>
    [VerificationCommand("--fx-overlay-placement")]
    internal static int FxOverlayPlacementCommand()
    {
        Suite(nameof(VerifyPublishedOverlayShowsThePageRows), VerifyPublishedOverlayShowsThePageRows);
        Suite(nameof(VerifyBackendDrawsLiquidOverlay), VerifyBackendDrawsLiquidOverlay);
        return 0;
    }
}
