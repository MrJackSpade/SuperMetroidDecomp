using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Frontend.Pause;
namespace SuperMetroid.Verification.Harness;

using SuperMetroid.Verification.Frontend.Pause;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.World.RoomStates;

/// <summary>Restores stable pause-map state from a native WRAM snapshot for frame comparison.</summary>
internal static class NativeSnapshotImportPauseVerification
{
    /// <summary>
    /// Imports a snapshot taken on the pause menu's stable map page (game state $0F). The
    /// menu is built as state $0D builds it, then takes the words the native menu has changed
    /// since: map scroll, animation phases and the delayed-held input filter. Gameplay colors
    /// come from <c>BackupOfPalettesDuringMenu</c>, where pausing moved them.
    /// </summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    internal static void ImportNativePauseMenu(SuperMetroidGame game, SuperMetroidRuntime runtime,
        Func<int, ushort> W, byte[] memory)
    {
        AssertEqual((ushort)SuperMetroidGameState.PausedB, W(NativeSnapshotMemory.GameState),
            "pause import covers the stable pause menu");
        AssertEqual((ushort)0, W(PauseMemory.MenuIndex), "imported pause menu is on its map page");
        AssertEqual((ushort)0, W(PauseMemory.ButtonLabelMode), "imported pause menu shows map labels");
        runtime.Cgram.LoadBytes(memory.AsSpan(PauseMemory.GameplayPaletteBackup, CgramPalettes.ByteCount));

        CartridgeRoomHeader room = runtime.Room.ActiveRoom ?? throw new InvalidDataException("Paused snapshot has no room.");
        var menu = new PauseMenuState(new PauseMenuEntry { Samus = runtime.Player.Samus ?? throw new InvalidDataException("Paused snapshot has no Samus."), System = runtime.System, AreaIndex = room.AreaIndex, RoomMapPosition = room.MapPosition, Audio = game.Session.audio, GameplayLayer3 = runtime.FrameBoundary.CaptureLayer3Plane(), MapRevealMode = game.ConfiguredOptions.MapReveal, MapPresentation = game.Artwork.MapPresentation, SpriteSheets = game.Artwork.SpriteSheets ?? throw new InvalidDataException("Paused snapshot has no map presentation.") });
        menu.MapScrollPosition = new Position16<MapPixels>(W(PauseMemory.MapHorizontalScroll), W(PauseMemory.MapVerticalScroll));
        var palette = menu.paletteAnimation;
        palette.Timer = memory[PauseMemory.PaletteAnimationTimer];
        palette.Frame = memory[PauseMemory.PaletteAnimationFrame];
        // $0776 is a byte offset into $82:BA25's word tables; the port keeps the phase index.
        menu.MapIndicatorAnimationFrame = W(PauseMemory.IndicatorAnimationFrame) / 2;
        menu.MapIndicatorAnimationTimer = W(PauseMemory.IndicatorAnimationTimer);
        game.Session.Show(menu);
        game.Session.screenFade.Delay = W(PauseMemory.ScreenFadeDelay);
        game.Session.screenFade.Counter = W(PauseMemory.ScreenFadeCounter);

        runtime.System.TimedHeldInputTimer = WrappingCountdown.FromRaw(W(PauseMemory.TimedHeldTimer));
        runtime.System.TimedHeldInputTimerReset = W(PauseMemory.TimedHeldReset);
        runtime.System.TimedHeldInput = (SnesButton)W(PauseMemory.TimedHeldInput);
        runtime.System.NewlyTimedHeldInput = (SnesButton)W(PauseMemory.TimedHeldNew);
        runtime.System.TimedHeldInputPrevious = (SnesButton)W(PauseMemory.TimedHeldPrevious);
    }
}
