using SuperMetroid.Diagnostics.Input;
using SuperMetroid.TestSupport.Assertions;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Progression;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Frontend.Pause;
namespace SuperMetroid.TestSupport.MovieReplay;

using SuperMetroid.Core.Progression;
using SuperMetroid.Core.World.RoomStates;

/// <summary>Restores stable pause-map state from a native WRAM snapshot for frame comparison.</summary>
internal static class NativeSnapshotPauseImport
{
    /// <summary>
    /// Imports a snapshot taken on the pause menu's stable map page (game state $0F). The
    /// menu is built as state $0D builds it, then takes the words the native menu has changed
    /// since: map scroll, animation phases and the delayed-held input filter. Gameplay colors
    /// come from <c>BackupOfPalettesDuringMenu</c>, where pausing moved them.
    /// </summary>
    internal static void ImportNativePauseMenu(SuperMetroidGame game, SuperMetroidRuntime runtime,
        Func<int, ushort> W, byte[] memory)
    {
        VerificationAssertions.AssertEqual(SuperMetroidGameState.Paused, GameStateNumberCodec.Decode(W(NativeSnapshotMemory.GameState)),
            "pause import covers the stable pause menu");
        VerificationAssertions.AssertEqual((ushort)0, W(PauseMemory.MenuIndex), "imported pause menu is on its map page");
        VerificationAssertions.AssertEqual((ushort)0, W(PauseMemory.ButtonLabelMode), "imported pause menu shows map labels");

        SelectedRoom room = runtime.Room.ActiveRoom ?? throw new InvalidDataException("Paused snapshot has no room.");
        var menu = new PauseMenuState(new PauseMenuEntry { Samus = runtime.Player.Samus ?? throw new InvalidDataException("Paused snapshot has no Samus."), System = runtime.System, AreaIndex = room.AreaIndex, RoomMapPosition = room.MapPosition, Audio = game.Session.audio, GameplayHud = runtime.Hud.Shown(), MapRevealMode = game.Session.Assists.MapReveal, MapPresentation = game.Artwork.MapPresentation, SpriteSheets = game.Artwork.SpriteSheets ?? throw new InvalidDataException("Paused snapshot has no map presentation.") });
        menu.MapScrollPosition = new Position16<MapPixels>(W(PauseMemory.MapHorizontalScroll), W(PauseMemory.MapVerticalScroll));
        var highlight = menu.mapHighlight;
        highlight.Timer = memory[PauseMemory.PaletteAnimationTimer];
        highlight.Frame = memory[PauseMemory.PaletteAnimationFrame];
        // $0776 is a byte offset into $82:BA25's word tables; the port keeps the phase index.
        menu.MapIndicatorAnimationFrame = W(PauseMemory.IndicatorAnimationFrame) / 2;
        menu.MapIndicatorAnimationTimer = W(PauseMemory.IndicatorAnimationTimer);
        game.Session.Show(menu);
        game.Session.screenFade.Delay = W(PauseMemory.ScreenFadeDelay);
        game.Session.screenFade.Counter = W(PauseMemory.ScreenFadeCounter);

        runtime.System.TimedHeldInputTimer = WrappingCountdown.Start(W(PauseMemory.TimedHeldTimer));
        runtime.System.TimedHeldInputTimerReset = W(PauseMemory.TimedHeldReset);
        runtime.System.TimedHeldInput = ControllerWordCodec.FromJoypadWord(W(PauseMemory.TimedHeldInput), "native timed held input");
    }
}
