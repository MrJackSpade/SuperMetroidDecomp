using SuperMetroid.Core;
using SuperMetroid.Diagnostics.Input;
using SuperMetroid.TestSupport.Assertions;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Progression;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Frontend.Map;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Input;
namespace SuperMetroid.TestSupport.MovieReplay;

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
    internal static void ShowNativePauseMenu(SuperMetroidGame game, SuperMetroidRuntime runtime,
        Func<int, ushort> W, byte[] memory)
    {
        VerificationAssertions.AssertTrue(GameStateNumberCodec.NamesPauseMenu(W(NativeSnapshotMemory.GameState)),
            "pause import covers the stable pause menu");
        VerificationAssertions.AssertEqual((ushort)0, W(PauseMemory.MenuIndex), "imported pause menu is on its map page");
        VerificationAssertions.AssertEqual((ushort)0, W(PauseMemory.ButtonLabelMode), "imported pause menu shows map labels");

        SelectedRoom room = runtime.Room.ActiveRoom ?? throw new InvalidDataException("Paused snapshot has no room.");
        var menu = new PauseMenuState(new PauseMenuEntry { Samus = runtime.Player.Samus ?? throw new InvalidDataException("Paused snapshot has no Samus."), System = runtime.System, Area = room.Area, RoomMapPosition = room.MapPosition, Audio = game.Session.audio, GameplayHud = runtime.Hud.Shown(), MapRevealMode = game.Session.Assists.MapReveal, ScreenContent = game.Artwork.ScreenContent, SpriteSheets = game.Artwork.SpriteSheets });
        menu.ScrollMapTo(Offset16<MapPixels>.Wrapped(W(PauseMemory.MapHorizontalScroll), W(PauseMemory.MapVerticalScroll)));
        // The highlight and the indicator are brought where the native menu's updates left them as the menu's own updates
        // bring them, each update stepping each once. The highlight's native timer counts the updates its frame has left, as
        // the highlight's does.
        MapHighlightCycle highlight = game.Artwork.ScreenContent.Maps.HighlightCycle;
        int highlightUpdates = UpdatesToShow([.. highlight.Frames.Select(highlight.Duration)],
            memory[PauseMemory.PaletteAnimationFrame], memory[PauseMemory.PaletteAnimationTimer]);
        for (int update = 0; update < highlightUpdates; update++)
            _ = menu.mapHighlight.Step();
        // $0776 is a byte offset into $82:BA25's word tables; the native marker counts its timer down after its frame's
        // update and moves on at zero, one update short of the indicator's count.
        int indicatorUpdates = UpdatesToShow(
            [.. PauseMapIndicatorAnimation.Segments.Beats(MapIndicatorSegment.Pulse).OfType<AnimationBeat<MapSpriteId, MapIndicatorCue>.Frame>().Select(frame => frame.Duration)],
            W(PauseMemory.IndicatorAnimationFrame) / 2, W(PauseMemory.IndicatorAnimationTimer) + 1);
        for (int update = 0; update < indicatorUpdates; update++)
            menu.mapIndicator.Step(_ => { });
        game.Session.Show(menu);
    }

    /// <summary>
    /// How many updates a looping animation of frames lasting <paramref name="durations"/>, started on its first frame for
    /// one update as the pause map's pictures start, takes to show frame <paramref name="frame"/> with
    /// <paramref name="updatesLeft"/> of its updates left: each update counts one off the frame shown and moves on to the
    /// next frame, for its own updates, once none is left. The first frame's first update is its start.
    /// </summary>
    /// <exception cref="InvalidDataException">The frame is not one of the animation's, or has fewer updates than are left.</exception>
    private static int UpdatesToShow(IReadOnlyList<int> durations, int frame, int updatesLeft)
    {
        if (frame < 0 || frame >= durations.Count || updatesLeft < 1 || updatesLeft > durations[frame])
            throw new InvalidDataException($"A pause map picture's frame {frame} with {updatesLeft} updates left is not one its {durations.Count} frames show.");
        if (frame == 0 && updatesLeft == 1)
            return 0;
        // A first frame shows again once every later frame has shown, after its start's one update.
        int reached = frame == 0 ? durations.Count : frame;
        return 1 + durations.Skip(1).Take(reached - 1).Sum() + durations[frame] - updatesLeft;
    }

    /// <summary>
    /// Imports the cadences the native menu runs once the game is at the stable menu (<see cref="ShowNativePauseMenu"/>): the
    /// screen fade's and the delayed-held input filter's.
    /// </summary>
    internal static void ImportNativePauseMenuCadence(SuperMetroidGame game, SuperMetroidRuntime runtime, Func<int, ushort> W)
    {
        VerificationAssertions.AssertTrue(game.IsAt(PauseStep.Menu) && GameStateNumberCodec.IsAt(game, W(NativeSnapshotMemory.GameState)),
            "the imported game is at the stable pause menu");
        game.Session.screenFade.SetCadence(W(PauseMemory.ScreenFadeDelay), W(PauseMemory.ScreenFadeCounter));

        // The native countdown starts at the reset on a change and is pinned to zero once the repeat fires; the port's repeat
        // wait counts the unchanged updates instead, over once repeating and absent before the first unchanged update.
        int delay = W(PauseMemory.TimedHeldReset);
        VerificationAssertions.AssertEqual(PauseCadenceDefinitions.HeldInputRepeatDelay, delay, "native pause held-input reset");
        ControllerButton repeating = ControllerWordCodec.FromJoypadWord(W(PauseMemory.TimedHeldInput), "native timed held input");
        int countdown = Ensure.BetweenInclusive(0, delay, W(PauseMemory.TimedHeldTimer));
        int unchangedUpdates = repeating == ControllerButton.None ? delay - countdown : delay + 1;
        PrivateState.SetField(runtime.PauseHeldButtonRepeat, "_repeatWait",
            unchangedUpdates == 0 ? null : UpdateCountdownAccess.AtAge(delay + 1, unchangedUpdates));
        PrivateState.Set(runtime.PauseHeldButtonRepeat, nameof(HeldButtonRepeat.Repeating), repeating);
    }
}
