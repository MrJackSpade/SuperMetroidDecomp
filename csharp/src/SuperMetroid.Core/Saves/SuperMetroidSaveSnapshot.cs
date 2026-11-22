using SuperMetroid.Core.Input;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Saves;

/// <summary>What a save station, checkpoint or save editor hands a save slot to keep: Samus, options and progress.</summary>
public sealed partial record SuperMetroidSaveSnapshot
{
    /// <summary>The button bound to each of the seven configurable actions, no button bound to two.</summary>
    public required ControllerBindings ControllerBindings { get; init; }

    /// <summary>Whether the options menu's moonwalk behavior is enabled.</summary>
    public required bool MoonwalkEnabled { get; init; }

    /// <summary>Whether door transitions automatically clear the selected HUD item.</summary>
    public required bool IconCancelEnabled { get; init; }

    /// <summary>Whether the alternate Japanese text setting is enabled.</summary>
    public required TextLanguage Language { get; init; }

    /// <summary>What Samus carries: her items and beams, reserve mode, energy, ammunition and reserve, with their capacities.</summary>
    public required SamusInventory Inventory { get; init; }

    /// <summary>The HUD selection: beam, missile, super missile, power bomb, grapple, or X-ray.</summary>
    public required HudItem HudItem { get; init; }

    /// <summary>The gameplay clock's reading.</summary>
    public required GameClockReading GameTime { get; init; }

    /// <summary>The startup route the save resumes by.</summary>
    public required SaveStartupRoute StartupRoute { get; init; }

    /// <summary>The load station the save resumes at.</summary>
    public required LoadStation Checkpoint { get; init; }

    /// <summary>The persistent progress to save; the snapshot owns this copy.</summary>
    public required PersistentProgress Progress { get; init; }

    /// <summary>Captures the gameplay owners into a detached snapshot.</summary>
    /// <param name="samus">Source of player inventory, ammunition, energy, and reserve state.</param>
    /// <param name="system">Source of persistent events, defeated bosses, collected items, opened doors, maps, and load state.</param>
    /// <param name="context">The save location, timer, bindings and options a save snapshot records with Samus and system state (#633).</param>
    /// <returns>A detached copy safe to pass to <see cref="SaveSlotStore.SaveSlot"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="samus"/> or <paramref name="system"/> is <see langword="null"/>.</exception>
    public static SuperMetroidSaveSnapshot Capture(SamusState samus, SharedGameState system, SaveSnapshotContext context)
    {
        Ensure.NotNull(samus);
        Ensure.NotNull(system);
        return new SuperMetroidSaveSnapshot
        {
            ControllerBindings = context.ControllerBindings.RequireUniquePermutation(),
            MoonwalkEnabled = context.MoonwalkEnabled,
            IconCancelEnabled = context.IconCancelEnabled,
            Inventory = samus.Inventory,
            HudItem = samus.SelectedHudItem,
            Language = context.Language,
            GameTime = context.GameTime,
            StartupRoute = system.SavedStartupRoute,
            Checkpoint = Ensure.NotNull(context.Checkpoint),
            Progress = system.Progress.Copy(),
        };
    }
}
