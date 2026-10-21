using SuperMetroid.Core.Samus.Projectiles;
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
    [PpuLayoutBoundary("exact hardware test: checks the pause tilemaps' native VRAM words")]
    internal static void VerifyPauseScreenPublication()
    {
        var pause = CreateRetailPauseFixture(new PauseMenuEntry
        {
            Samus = new SamusState { CollectedBeams = SamusBeamFlags.Charge, EquippedBeams = SamusBeamFlags.Charge },
            System = new Bank80SystemState(), AreaIndex = AreaId.Crateria, RoomMapPosition = new(28, 1),
        });
        for (int frame = 0; frame < 4; frame++) pause.Step(0, 0);
        AssertPublished(pause, "map page");
        pause.Step(SnesButton.R, SnesButton.R);
        while (pause.ScreenMode == PauseScreenMode.Map) pause.Step(0, 0);
        for (int frame = 0; frame < 4; frame++) pause.Step(0, 0);
        AssertPublished(pause, "equipment page");

        Rgba32[] shown = pause.Render();
        using var state = new MemoryStream();
        DebuggerObjectGraphSerializer.Serialize(state, pause);
        state.Position = 0;
        var restored = DebuggerObjectGraphSerializer.Deserialize<PauseMenuState>(state);
        restored.BindMapPresentation(RetailPresentationFixture());
        AssertTrue(shown.SequenceEqual(restored.Render()), "a restored pause screen shows the same frame once its artwork is rebound");
        Console.WriteLine("  Pause screen: map and equipment pages publish their own tilemaps, palette and sprite entries, and survive a debugger round trip.");
    }

    /// <summary>Asserts that pause's captured frame is composed from its screen's published state.</summary>
    private static void AssertPublished(PauseMenuState pause, string page)
    {
        PauseScreen screen = pause.screen;
        PpuMemorySnapshot memory = pause.CaptureRenderSnapshot().Memory;
        AssertWords(memory, PauseMenuLayout.Bg1TilemapWord, screen.Tilemaps.Bg1, $"{page} BG1");
        AssertWords(memory, PauseMenuLayout.Bg2TilemapWord, screen.Tilemaps.Bg2, $"{page} BG2");
        AssertWords(memory, GameplayVramLayout.HudTilemapWord, screen.Tilemaps.Bg3, $"{page} BG3");
        int tilemapsFirst = PauseMenuLayout.Bg1TilemapWord * sizeof(ushort);
        int tilemapsEnd = (PauseMenuLayout.Bg2TilemapWord + screen.Tilemaps.Bg2.Length) * sizeof(ushort);
        AssertTrue(memory.Vram[..tilemapsFirst].SequenceEqual(screen.Characters[..tilemapsFirst]) &&
            memory.Vram[tilemapsEnd..(GameplayVramLayout.HudTilemapWord * sizeof(ushort))]
                .SequenceEqual(screen.Characters[tilemapsEnd..(GameplayVramLayout.HudTilemapWord * sizeof(ushort))]),
            $"{page} draws its bound characters");
        AssertTrue(memory.Cgram.SequenceEqual(screen.Palette.Colors), $"{page} draws its own palette");
        AssertTrue(screen.Objects.Draws.Length > 0 && memory.SpriteDraws.SequenceEqual(screen.Objects.Draws), $"{page} draws its own sprite entries");
    }

    /// <summary>Asserts that the words from <paramref name="firstWord"/> are exactly <paramref name="expected"/>.</summary>
    private static void AssertWords(PpuMemorySnapshot memory, int firstWord, ReadOnlySpan<ushort> expected, string context)
    {
        for (int word = 0; word < expected.Length; word++)
        {
            int at = (firstWord + word) * sizeof(ushort);
            AssertEqual(expected[word], SnesLittleEndian.Word(memory.Vram[at], memory.Vram[at + 1]), $"{context} word {word}");
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
