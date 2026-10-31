using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Frontend.Map;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Diagnostics.SaveStates;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Frontend.Pause;

/// <summary>Checks that pause publishes its own screen rather than owning PPU memory (#1358).</summary>
internal static class PauseScreenPublicationVerification
{
    /// <summary>
    /// On the map and equipment pages, pause's captured frame is composed from its screen: the BG1, BG2 and BG3
    /// tilemaps at their native words over the bound characters, its palette and its sprite entries. A debugger round
    /// trip keeps that screen state and, once the artwork is rebound, shows the same frame.
    /// </summary>
    internal static void VerifyPauseScreenPublication()
    {
        var pause = CreateRetailPauseFixture(new PauseMenuEntry
        {
            Samus = new SamusState(InstalledFixtures.AnimationContinuation) { CollectedBeams = SamusBeamFlags.Charge, EquippedBeams = SamusBeamFlags.Charge },
            System = new SharedGameState(), AreaIndex = AreaId.Crateria, RoomMapPosition = new(28, 1),
        });
        for (int frame = 0; frame < 4; frame++) pause.Step(0, 0);
        AssertPublished(pause, "map page");
        pause.Step(ControllerButton.R, ControllerButton.R);
        while (pause.ScreenMode == PauseScreenMode.Map) pause.Step(0, 0);
        for (int frame = 0; frame < 4; frame++) pause.Step(0, 0);
        AssertPublished(pause, "equipment page");

        RgbaPixel[] shown = pause.Render();
        using var state = new MemoryStream();
        DebuggerObjectGraphSerializer.Serialize(state, pause);
        state.Position = 0;
        var restored = DebuggerObjectGraphSerializer.Deserialize<PauseMenuState>(state);
        restored.BindFixtureArtwork(RetailPresentationFixture());
        AssertTrue(shown.SequenceEqual(restored.Render()), "a restored pause screen shows the same frame once its artwork is rebound");
        Console.WriteLine("  Pause screen: map and equipment pages publish their own tilemaps, palette and sprite entries, and survive a debugger round trip.");
    }

    /// <summary>Asserts that pause's captured frame is composed from its screen's published state.</summary>
    private static void AssertPublished(PauseMenuState pause, string page)
    {
        PauseScreen screen = pause.screen;
        FramePaint memory = pause.CaptureRenderSnapshot().Paint;
        byte[] vram = pause.ShownVram();
        BackgroundTile[] frame = pause.ShownBg2Tiles;
        AssertTiles(vram, PauseMenuLayoutConstants.Bg1TilemapWord, pause.ShownBg1Tiles, $"{page} BG1");
        AssertTiles(vram, PauseMenuLayoutConstants.Bg2TilemapWord, frame, $"{page} BG2");
        AssertTiles(vram, GameplayVramLayoutConstants.HudTilemapWord, screen.Bg3Tiles, $"{page} BG3");
        AssertShownPlanes(pause, page);
        int tilemapsFirst = PauseMenuLayoutConstants.Bg1TilemapWord * sizeof(ushort);
        int tilemapsEnd = (PauseMenuLayoutConstants.Bg2TilemapWord + frame.Length) * sizeof(ushort);
        AssertTrue(vram.AsSpan(0, tilemapsFirst).SequenceEqual(screen.Characters[..tilemapsFirst]) &&
            vram.AsSpan(tilemapsEnd, GameplayVramLayoutConstants.HudTilemapWord * sizeof(ushort) - tilemapsEnd)
                .SequenceEqual(screen.Characters[tilemapsEnd..(GameplayVramLayoutConstants.HudTilemapWord * sizeof(ushort))]),
            $"{page} draws its bound characters");
        AssertTrue(memory.Colors.SequenceEqual(screen.Palette.Colors), $"{page} draws its own palette");
        AssertTrue(screen.Objects.Draws.Length > 0 && PaintedSpriteEntries.AreOwn(memory.SpriteDraws, screen.Objects.Draws), $"{page} draws its own sprite entries");
    }

    /// <summary>
    /// Asserts that the captured frame draws the map page as the grid of the shown map's cell pictures, or the equipment
    /// page, and the frame as the area's backdrop, button rows and labels in the highlight's shades (#1467).
    /// </summary>
    private static void AssertShownPlanes(PauseMenuState pause, string page)
    {
        DisplayLayer[] layers = [.. pause.CaptureRenderSnapshot().Layers];
        PicturedPlane frame = pause.screen.Frame(pause.area, pause.ButtonHighlight);
        AssertTrue(layers.OfType<PicturedPlaneLayer>().Any(layer => layer.Plane.Equals(frame)), $"{page} draws the area's frame pictures");
        bool grid = layers.OfType<PictureGridLayer>().Any(layer => layer.Grid.Equals(AreaMapPictures.Grid(pause.ShownMap, pause.screen.Pictures)));
        AssertEqual(pause.ScreenMode == PauseScreenMode.Map, grid, $"{page} draws the shown map's cell pictures only on the map page");
        bool equipment = pause.ShownEquipment is { } shown
            && layers.OfType<PicturedPlaneLayer>().Any(layer => layer.Plane.Equals(pause.screen.EquipmentPage(shown)));
        AssertEqual(pause.ScreenMode != PauseScreenMode.Map, equipment, $"{page} draws the uploaded equipment page's pictures only on the equipment page");
    }

    /// <summary>Asserts that the words of <paramref name="vram"/> from <paramref name="firstWord"/> place exactly the tiles <paramref name="expected"/>.</summary>
    private static void AssertTiles(byte[] vram, int firstWord, ReadOnlySpan<BackgroundTile> expected, string context)
    {
        for (int cell = 0; cell < expected.Length; cell++)
        {
            int at = (firstWord + cell) * sizeof(ushort);
            AssertEqual(expected[cell], BackgroundTileWords.ToTile(LittleEndianBytes.Word(vram[at], vram[at + 1])), $"{context} tile {cell}");
        }
    }

    /// <summary>Runs the pause screen publication check.</summary>
    [VerificationCommand("--pause-screen")]
    internal static int PauseScreenCommand()
    {
        Suite(nameof(VerifyPauseScreenPublication), () => VerifyPauseScreenPublication());
        return 0;
    }
}
