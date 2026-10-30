using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
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
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.LandingSite);
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        GameplayHudArtwork artwork = GameplayHudArtwork.Of((runtime.Artwork.MapPresentation ?? throw new InvalidOperationException(
            "The retail fixture installs the HUD artwork.")).GameplayHud);
        ShownHud first = runtime.AcceptedHud;
        ushort[] firstWords = runtime.AcceptedHudWords;
        ushort[] topRow = BackgroundTileWordBytes.Words(artwork.TopRowTiles());
        AssertTrue(firstWords[..Layer3Plane.Columns].SequenceEqual(topRow), "row zero is the HUD's static top row");
        AssertTrue(firstWords[Layer3Plane.Columns..(Layer3PlaneConstants.HudRows * Layer3Plane.Columns)].SequenceEqual(runtime.Hud.TileWords),
            "rows one through three are the rows the HUD's values draw");

        // A HUD change reaches the next NMI's plane; the published one keeps its rows. Lost energy redraws the energy
        // count with digits the HUD's artwork shows, so the NMI can picture them (#1392).
        ushort[] before = runtime.Hud.TileWords;
        SamusState samus = runtime.Player.IntroducedSamus;
        samus.Health = samus.Health == 42 ? 37 : 42;
        runtime.Hud.UpdateGameplayCounters(samus);
        ushort[] after = runtime.Hud.TileWords;
        int cell = Enumerable.Range(0, after.Length).First(index => after[index] != before[index]);
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        AssertEqual(after[cell], runtime.AcceptedHudWords[Layer3Plane.Columns + cell], "the next NMI's HUD shows the HUD change");
        AssertEqual(before[cell], BackgroundTileWordBytes.Words(artwork.Rows(first))[Layer3Plane.Columns + cell], "an earlier latched HUD keeps its images");
        Console.WriteLine("  BG3 plane: the HUD's images publish from the HUD's values at each NMI.");
    }

    /// <summary>A water effect at a constructed level shows its authored page below the HUD and captures from that plane.</summary>
    internal static void VerifyWaterEffectPlane()
    {
        var runtime = CreateRetailRuntimeFixture();
        var presentation = runtime.Artwork.MapPresentation ?? throw new InvalidOperationException("The retail fixture installs the FX artwork.");
        RoomFxRecordDefinition record = RoomFxDefinitions.AllRecords.First(candidate =>
            candidate.Type == RoomFxType.Water && candidate.DoorPointer == 0) with { BaseYPosition = 0x0080 };
        var session = new RoomFxSession { PaletteBlendColors = presentation.RoomFxPaletteBlends };
        RoomLayer3FxState water = RoomLayer3FxState.ForDefinition(session, new GameplayColours(new SpriteColourSets()), record, 0);
        AssertEqual<RoomFxType?>(RoomFxType.Water, water.EffectPageType, "the water room shows the water page");

        // The plane names the water page (#1466); the installed pages spell the words the cartridge writes for it.
        Layer3Plane shownPlane = water.EffectPlane;
        AssertEqual<RoomFxType?>(RoomFxType.Water, shownPlane.EffectPage, "the room's BG3 plane shows the water page");
        RoomFxLayer3Pages pages = InstalledFixtures.Maps.RoomFxLayer3Tilemaps;
        ushort[] plane = shownPlane.WordsIn(pages);
        ReadOnlySpan<byte> page = pages.ResolveBytes(RoomFxType.Water);
        for (int row = Layer3PlaneConstants.HudRows; row < Layer3Plane.Rows; row++)
        for (int column = 0; column < Layer3Plane.Columns; column++)
        {
            int pageWord = (row - Layer3Plane.EffectPageFirstRow) * Layer3Plane.Columns + column;
            ushort expected = pageWord >= 0 && pageWord * 2 < page.Length
                ? LittleEndianBytes.Word(page[pageWord * 2], page[pageWord * 2 + 1])
                : Layer3PlaneConstants.ClearedCell.PackedWord;
            AssertEqual(expected, plane[row * Layer3Plane.Columns + column], $"BG3 row {row} column {column}: cleared, with the water page from row 31");
        }
        AssertTrue(plane[..(Layer3PlaneConstants.HudRows * Layer3Plane.Columns)].All(word => word == Layer3PlaneConstants.ClearedCell.PackedWord),
            "the rows the HUD's images cover stay cleared above the effect");

        RoomLayer3FxRenderSnapshot shown = water.CaptureForDisplay() ?? throw new InvalidOperationException("Water is a renderable effect.");
        AssertEqual((ushort)0x0080, shown.Surface.Y, "the published effect carries the constructed liquid level");
        Layer3Cells cells = Layer3CellsFixtures.FromWords(plane, new byte[SnesVram.ByteCount], new RgbColor[SnesCgram.ColorCount]);
        OverlayBlendLayer layer = GameplayEffectCapture.CaptureRoomLayer3Fx(shown, cells)
            ?? throw new InvalidOperationException("Water at a visible level captures a BG3 layer.");
        AssertTrue(ReferenceEquals(cells, layer.Plane) && layer.FirstRow == 0 && layer.MapHeightTiles == Layer3Plane.Rows,
            "liquid BG3 samples the whole published plane");
        Console.WriteLine("  BG3 plane: a water effect shows its page under the HUD and captures from the plane.");
    }

    /// <summary>After N steps the typewriter has typed exactly the glyphs it reports, each at its program's next destination.</summary>
    internal static void VerifyTypewriterGlyphs()
    {
        var program = new EscapeTypewriterProgram(EscapeTypewriterProgramId.Ceres,
            [new EscapeTypewriterLine(0x105, "AB C!")]);
        var text = new EscapeTypewriterState(program, EscapeTypewriterDefinitions.ZebesTileBase);
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
        int[] cells = [0x105, 0x106, 0x108, 0x109];
        ushort zebesBase = EscapeTypewriterDefinitions.ZebesTileBase.PackedWord;
        ushort[] tiles = [zebesBase, (ushort)(zebesBase + 1), (ushort)(zebesBase + 2), (ushort)(zebesBase + 26)];
        for (int index = 0; index < typed.Count; index++)
        {
            AssertEqual(cells[index], typed[index].Cell, $"glyph {index} lands at its program's cell");
            AssertEqual(tiles[index], typed[index].Tile.PackedWord, $"glyph {index} tile");
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
