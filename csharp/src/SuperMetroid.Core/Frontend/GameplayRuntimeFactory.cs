using SuperMetroid.Core.Runtime;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// Creates the session's gameplay runtimes. Lifetime: a runtime lives from a selected-game, attract or saved-Ceres start
/// until title, game over or soft reset; the immutable artwork it binds is host-owned and shared, and the frame clock and
/// Mode 7 residue it counts on belong to the session.
/// </summary>
internal static class GameplayRuntimeFactory
{
    /// <summary>
    /// Constructs a runtime over the session's random-number word and performs the start handoff in the cartridge's
    /// order: the menu's layer-1 position, then the artwork, then a selected game's menu choices.
    /// </summary>
    /// <param name="start">The handoff; a selected game must carry its options and an attract demo must not.</param>
    /// <returns>The bound runtime, with no room selected.</returns>
    internal static SuperMetroidRuntime Create(GameplayRuntimeStart start)
    {
        Ensure.NotNull(start);
        if ((start.Purpose == GameplayRuntimePurpose.SelectedGame) != (start.Selected is not null))
            throw new ArgumentException($"A {start.Purpose} runtime start {(start.Selected is null ? "requires" : "takes no")} selected-game options.", nameof(start));

        GameplaySessionCarryover carryover = start.Carryover;
        var runtime = new SuperMetroidRuntime(start.Overrides, start.Artwork, carryover.Clock, carryover.UnauthoredBoostCadence,
            carryover.Random, carryover.EarthquakeSoundDelay, carryover.SaveChoiceInput);
        start.Layer1.PublishTo(runtime);
        runtime.BindArtwork(start.Artwork, previous: null);
        if (start.Selected is { } selected)
        {
            runtime.JapaneseText = selected.JapaneseText;
            runtime.Options.ControllerBindings = selected.ControllerBindings;
            runtime.Options.MoonwalkEnabled = selected.MoonwalkEnabled;
            runtime.Options.IconCancelEnabled = selected.IconCancelEnabled;
        }
        return runtime;
    }
}
