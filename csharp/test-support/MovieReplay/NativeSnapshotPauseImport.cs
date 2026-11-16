using SuperMetroid.Core;
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
        var menu = new PauseMenuState(new PauseMenuEntry { Samus = runtime.Player.Samus ?? throw new InvalidDataException("Paused snapshot has no Samus."), System = runtime.System, Area = room.Area, RoomMapPosition = room.MapPosition, Audio = game.Session.audio, GameplayHud = runtime.Hud.Shown(), MapRevealMode = game.Session.Assists.MapReveal, ScreenContent = game.Artwork.ScreenContent, SpriteSheets = game.Artwork.SpriteSheets });
        menu.ScrollMapTo(Offset16<MapPixels>.Wrapped(W(PauseMemory.MapHorizontalScroll), W(PauseMemory.MapVerticalScroll)));
        var highlight = menu.mapHighlight;
        PrivateState.Set(highlight, nameof(highlight.Timer), (int)memory[PauseMemory.PaletteAnimationTimer]);
        PrivateState.Set(highlight, nameof(highlight.Frame), (int)memory[PauseMemory.PaletteAnimationFrame]);
        // $0776 is a byte offset into $82:BA25's word tables; the port keeps the phase index.
        PrivateState.Set(menu, nameof(PauseMenuState.MapIndicatorAnimationFrame), W(PauseMemory.IndicatorAnimationFrame) / 2);
        PrivateState.Set(menu, nameof(PauseMenuState.MapIndicatorAnimationTimer), (int)W(PauseMemory.IndicatorAnimationTimer));
        game.Session.Show(menu);
        game.Session.screenFade.SetCadence(W(PauseMemory.ScreenFadeDelay), W(PauseMemory.ScreenFadeCounter));

        PrivateState.Set(runtime.System, nameof(SharedGameState.TimedHeldInputTimer), WrappingCountdown.Start(W(PauseMemory.TimedHeldTimer)));
        PrivateState.Set(runtime.System, nameof(SharedGameState.TimedHeldInputTimerReset), (int)W(PauseMemory.TimedHeldReset));
        PrivateState.Set(runtime.System, nameof(SharedGameState.TimedHeldInput),
            ControllerWordCodec.FromJoypadWord(W(PauseMemory.TimedHeldInput), "native timed held input"));
    }
}
