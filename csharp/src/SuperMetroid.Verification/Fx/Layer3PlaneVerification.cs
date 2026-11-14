using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.Core.Assets;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Escape;
using SuperMetroid.Core.Escape.Artwork;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.AssetExtraction.Hud;

namespace SuperMetroid.Verification.Fx;

/// <summary>Checks the HUD, room effect and typewriter descriptions a frame publishes instead of BG3 VRAM (#1348).</summary>
internal static class Layer3PlaneVerification
{
    /// <summary>
    /// The HUD each NMI latches, whose images cover the published BG3 plane's top rows (#1466), lays out its static top row
    /// and the rows its values draw.
    /// </summary>
    internal static void VerifyPublishedHudRows()
    {
        var runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomDefinitions.LandingSite);
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        GameplayHudArtwork artwork = runtime.InstalledHudArtwork;
        ShownHud first = runtime.AcceptedHud;
        ushort[] firstWords = runtime.AcceptedHudWords;
        ushort[] topRow = BackgroundTileWordBytes.Words(artwork.TopRowTiles());
        AssertTrue(firstWords[..FxOverlayPlaneConstants.Columns].SequenceEqual(topRow), "row zero is the HUD's static top row");
        AssertTrue(firstWords[FxOverlayPlaneConstants.Columns..(FxOverlayPlaneConstants.HudRows * FxOverlayPlaneConstants.Columns)].SequenceEqual(runtime.Hud.TileWords(runtime.InstalledHudArtwork)),
            "rows one through three are the rows the HUD's values draw");

        // A HUD change reaches the next NMI's plane; the published one keeps its rows. Lost energy redraws the energy
        // count with digits the HUD's artwork shows, so the NMI can picture them (#1392).
        ushort[] before = runtime.Hud.TileWords(runtime.InstalledHudArtwork);
        SamusState samus = runtime.Player.IntroducedSamus;
        samus.Health = samus.Health == 42 ? 37 : 42;
        runtime.Hud.UpdateGameplayCounters(samus, runtime.AudioRequests);
        ushort[] after = runtime.Hud.TileWords(runtime.InstalledHudArtwork);
        int cell = Enumerable.Range(0, after.Length).First(index => after[index] != before[index]);
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        AssertEqual(after[cell], runtime.AcceptedHudWords[FxOverlayPlaneConstants.Columns + cell], "the next NMI's HUD shows the HUD change");
        AssertEqual(before[cell], BackgroundTileWordBytes.Words(artwork.Rows(first))[FxOverlayPlaneConstants.Columns + cell], "an earlier latched HUD keeps its images");
        Console.WriteLine("  BG3 plane: the HUD's images publish from the HUD's values at each NMI.");
    }

    /// <summary>A water effect at a constructed level shows its authored page below the HUD and captures from that plane.</summary>
    internal static void VerifyWaterEffectPlane()
    {
        var runtime = CreateRetailRuntimeFixture();
        var presentation = runtime.Artwork.MapPresentation ?? throw new InvalidOperationException("The retail fixture installs the FX artwork.");
        RoomEffectDefinition record = RoomFxDefinitions.AllRecords.First(candidate =>
            candidate.Type == RoomFxType.Water && candidate.DoorPointer == 0) with { LiquidStartLevel = 0x0080 };
        var session = new RoomFxSession();
        RoomFxOverlayState water = RoomFxOverlayState.ForDefinition(session, record, 0);
        AssertEqual<RoomFxType?>(RoomFxType.Water, water.OverlayEffect, "the water room shows the water page");

        // The plane names the water page (#1466); the installed pages spell the words the cartridge writes for it.
        FxOverlayPlane shownPlane = water.EffectPlane;
        AssertEqual<RoomFxType?>(RoomFxType.Water, shownPlane.Effect, "the room's BG3 plane shows the water page");
        RoomFxLayer3Pages pages = InstalledFixtures.ImportMaps.RoomFxPages;
        ushort[] plane = shownPlane.WordsIn(pages);
        ReadOnlySpan<byte> page = pages.ResolveBytes(RoomFxType.Water);
        for (int row = FxOverlayPlaneConstants.HudRows; row < FxOverlayPlaneConstants.Rows; row++)
        for (int column = 0; column < FxOverlayPlaneConstants.Columns; column++)
        {
            int pageWord = (row - FxOverlayPlaneConstants.EffectPageFirstRow) * FxOverlayPlaneConstants.Columns + column;
            ushort expected = pageWord >= 0 && pageWord * 2 < page.Length
                ? LittleEndianBytes.Word(page[pageWord * 2], page[pageWord * 2 + 1])
                : FxOverlayPlaneConstants.ClearedCell.PackedWord;
            AssertEqual(expected, plane[row * FxOverlayPlaneConstants.Columns + column], $"BG3 row {row} column {column}: cleared, with the water page from row 31");
        }
        AssertTrue(plane[..(FxOverlayPlaneConstants.HudRows * FxOverlayPlaneConstants.Columns)].All(word => word == FxOverlayPlaneConstants.ClearedCell.PackedWord),
            "the rows the HUD's images cover stay cleared above the effect");

        // The surface is placed on the screen as a frame places it, from the camera at the room's origin (#1546).
        water.PrimeViewport(Position16<RoomPixels>.Zero);
        RoomFxOverlayRenderSnapshot shown = water.CaptureForDisplay() ?? throw new InvalidOperationException("Water is a renderable effect.");
        AssertEqual(0x0080, shown.NamedSurface, "the published effect carries the constructed liquid level");
        FxOverlayArtwork shownArtwork = FxOverlayFixtures.ArtworkFromPlaneWords(RoomFxType.Water, plane, new byte[SnesVram.ByteCount], new RgbColor[SnesCgram.ColorCount]);
        OverlayBlendLayer layer = GameplayEffectCapture.CaptureRoomFxOverlay(shown, shownArtwork)
            ?? throw new InvalidOperationException("Water at a visible level captures a BG3 layer.");
        // The overlay shows the water's strip and body from its surface's screen line (#1706): the camera at the room's origin
        // places the surface on the line the level names.
        LiquidOverlay overlay = layer.Picture as LiquidOverlay ?? throw new InvalidOperationException("Water shows a liquid.");
        AssertTrue(ReferenceEquals(shownArtwork.Shown, overlay.Artwork) && overlay.SurfaceLine == 0x0080 && overlay.ShowsSurface,
            "a liquid's overlay shows its artwork from its surface's screen line");
        Console.WriteLine("  BG3 plane: a water effect shows its page under the HUD and captures from the plane.");
    }

    /// <summary>After N steps the typewriter has typed exactly the glyphs it reports, each at its program's next destination.</summary>
    internal static void VerifyTypewriterGlyphs()
    {
        var program = new EscapeTypewriterMessage(EscapeTypewriterMessageId.Ceres,
            [new EscapeTypewriterLine(new Position16<PlanePixels>(40, 64), "AB C!")]);
        var text = new EscapeTypewriterState(program);
        var typed = new List<EscapeTypewriterGlyph>();
        bool completed = false;
        int steps = 0;
        while (!completed)
        {
            completed = text.Step(out EscapeTypewriterGlyph? glyph);
            steps++;
            if (glyph is { } written)
                typed.Add(written);
            AssertEqual(typed.Count, text.GlyphsWritten, $"step {steps} reports every glyph it has typed");
        }
        AssertEqual(4, typed.Count, "the space types no glyph");
        // Each character, space included, moves the next glyph a glyph's width right.
        Position16<PlanePixels>[] corners = [new(40, 64), new(48, 64), new(64, 64), new(72, 64)];
        // Each glyph names the picture of its character (#1466); the import composes that picture from the character's tile.
        char[] characters = ['A', 'B', 'C', '!'];
        for (int index = 0; index < typed.Count; index++)
        {
            AssertEqual(corners[index], typed[index].Corner, $"glyph {index} lands at its program's pixel");
            AssertEqual(EscapeTypewriterGlyph.PictureOf(EscapeTypewriterMessageId.Ceres, characters[index]), typed[index].Picture,
                $"glyph {index} names its character's picture");
        }
        Console.WriteLine($"  BG3 plane: the typewriter typed {typed.Count} glyphs over {steps} steps at their destinations.");
    }

    /// <summary>Runs the BG3 plane checks.</summary>
    [VerificationCommand("--bg3-plane")]
    internal static int Bg3PlaneCommand()
    {
        Suite(nameof(VerifyPublishedHudRows), () => VerifyPublishedHudRows());
        Suite(nameof(VerifyWaterEffectPlane), () => VerifyWaterEffectPlane());
        Suite(nameof(VerifyTypewriterGlyphs), () => VerifyTypewriterGlyphs());
        return 0;
    }
}
