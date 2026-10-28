using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Assets;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Escape;
using SuperMetroid.Core.Escape.Artwork;
using SuperMetroid.AssetExtraction.Video;

namespace SuperMetroid.Verification.Fx;

/// <summary>Checks the HUD, room effect and typewriter descriptions a frame publishes instead of BG3 VRAM (#1348).</summary>
internal static class Layer3PlaneVerification
{
    /// <summary>The published BG3 plane's top rows are the HUD's: its static top row and the rows HudState owns.</summary>
    internal static void VerifyPublishedHudRows()
    {
        var runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.LandingSite);
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        Layer3Plane first = Published(runtime);
        ushort[] topRow = BackgroundTileWordBytes.Words((runtime.Artwork.MapPresentation ?? throw new InvalidOperationException(
            "The retail fixture installs the HUD artwork.")).GameplayHud.TopRow());
        AssertTrue(first.Words[..Layer3Plane.Columns].SequenceEqual(topRow), "row zero is the HUD's static top row");
        AssertTrue(first.Words[Layer3Plane.Columns..(Layer3Plane.HudRows * Layer3Plane.Columns)].SequenceEqual(BackgroundTileWordBytes.Words(runtime.Hud.MutableRows)),
            "rows one through three are the rows HudState owns");

        // A HUD change reaches the next NMI's plane; the published one keeps its rows. The new word is one the HUD's
        // artwork shows, so the NMI can picture it (#1392).
        const int cell = 7;
        ushort before = runtime.Hud.Tiles[cell].PackedWord;
        ushort changed = topRow.First(word => word != before);
        runtime.Hud.Tiles[cell] = BackgroundTileWordBytes.Tile(changed);
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        AssertEqual(changed, Published(runtime).Words[Layer3Plane.Columns + cell], "the next NMI's plane shows the HUD change");
        AssertEqual(before, first.Words[Layer3Plane.Columns + cell], "an earlier published plane keeps its rows");
        Console.WriteLine("  BG3 plane: the HUD rows publish from HudState at each NMI.");
    }

    /// <summary>A water effect at a constructed level shows its authored page below the HUD and captures from that plane.</summary>
    internal static void VerifyWaterEffectPlane()
    {
        var runtime = CreateRetailRuntimeFixture();
        var presentation = runtime.Artwork.MapPresentation ?? throw new InvalidOperationException("The retail fixture installs the FX artwork.");
        RoomFxRecordDefinition record = RoomFxDefinitions.AllRecords.First(candidate =>
            candidate.Type == RoomFxType.Water && candidate.DoorPointer == 0) with { BaseYPosition = 0x0080 };
        var session = new RoomFxSession { Layer3Tilemaps = presentation.RoomFxLayer3Tilemaps, PaletteBlendColors = presentation.RoomFxPaletteBlends };
        RoomLayer3FxState water = RoomLayer3FxState.ForDefinition(session, new GameplayColours(new SpriteColourSets()), record, 0);
        AssertEqual<RoomFxType?>(RoomFxType.Water, water.EffectPageType, "the water room shows the water page");

        var hudRows = new ushort[Layer3Plane.HudRows * Layer3Plane.Columns];
        for (int index = 0; index < hudRows.Length; index++)
            hudRows[index] = (ushort)(0x2000 + index);
        Layer3Plane plane = Layer3Plane.Compose(BackgroundTileWordBytes.Tiles(hudRows), water.EffectPageTiles());
        ReadOnlySpan<byte> page = presentation.RoomFxLayer3Tilemaps.ResolveBytes(RoomFxType.Water);
        for (int row = Layer3Plane.HudRows; row < Layer3Plane.Rows; row++)
        for (int column = 0; column < Layer3Plane.Columns; column++)
        {
            int pageWord = (row - Layer3Plane.EffectPageFirstRow) * Layer3Plane.Columns + column;
            ushort expected = pageWord >= 0 && pageWord * 2 < page.Length
                ? LittleEndianBytes.Word(page[pageWord * 2], page[pageWord * 2 + 1])
                : Layer3Plane.ClearedCell.PackedWord;
            AssertEqual(expected, plane.Words[row * Layer3Plane.Columns + column], $"BG3 row {row} column {column}: cleared, with the water page from row 31");
        }
        AssertTrue(plane.Words[..hudRows.Length].SequenceEqual(hudRows), "the HUD rows stay above the effect");

        RoomLayer3FxRenderSnapshot shown = water.CaptureForDisplay() ?? throw new InvalidOperationException("Water is a renderable effect.");
        AssertEqual((ushort)0x0080, shown.Surface.Y, "the published effect carries the constructed liquid level");
        Layer3Cells cells = Layer3CellsFixtures.FromWords(plane.Words, new byte[SnesVram.ByteCount], new RgbColor[SnesCgram.ColorCount]);
        FxPlaneBlendRenderLayer layer = GameplayEffectCapture.CaptureRoomLayer3Fx(shown, cells)
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

    /// <summary>The BG3 plane's words the runtime's last NMI latched.</summary>
    private static Layer3Plane Published(SuperMetroidRuntime runtime) => runtime.AcceptedLayer3Plane;

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
