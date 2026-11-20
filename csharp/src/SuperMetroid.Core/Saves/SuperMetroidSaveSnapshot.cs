using SuperMetroid.Core.Input;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Saves;

/// <summary>What a save station, checkpoint or save editor hands a save slot to keep: Samus, options and progress.</summary>
public sealed record SuperMetroidSaveSnapshot
{
    /// <summary>The button bound to each of the seven configurable actions, no button bound to two.</summary>
    public required ControllerBindings ControllerBindings { get; init; }

    /// <summary>Whether the options menu's moonwalk behavior is enabled.</summary>
    public required bool MoonwalkEnabled { get; init; }

    /// <summary>Whether door transitions automatically clear the selected HUD item.</summary>
    public required bool IconCancelEnabled { get; init; }

    /// <summary>Whether the alternate Japanese text setting is enabled.</summary>
    public required TextLanguage Language { get; init; }

    /// <summary>The collected items Samus has equipped.</summary>
    public required SamusEquipmentFlags EquippedItems { get; init; }

    /// <summary>The items Samus has collected, equipped or not.</summary>
    public required SamusEquipmentFlags CollectedItems { get; init; }

    /// <summary>The collected beams Samus has equipped.</summary>
    public required SamusBeamFlags EquippedBeams { get; init; }

    /// <summary>The beams Samus has collected, equipped or not.</summary>
    public required SamusBeamFlags CollectedBeams { get; init; }

    /// <summary>The reserve tanks' mode: none, auto or manual.</summary>
    public required ReserveTankMode ReserveMode { get; init; }

    /// <summary>Samus's current energy in energy-point units.</summary>
    public required int Health { get; init; }

    /// <summary>Samus's energy capacity in energy-point units.</summary>
    public required int MaxHealth { get; init; }

    /// <summary>The current ordinary missile count.</summary>
    public required int Missiles { get; init; }

    /// <summary>The acquired ordinary missile capacity.</summary>
    public required int MaxMissiles { get; init; }

    /// <summary>The current super missile count.</summary>
    public required int SuperMissiles { get; init; }

    /// <summary>The acquired super missile capacity.</summary>
    public required int MaxSuperMissiles { get; init; }

    /// <summary>The current power bomb count.</summary>
    public required int PowerBombs { get; init; }

    /// <summary>The acquired power bomb capacity.</summary>
    public required int MaxPowerBombs { get; init; }

    /// <summary>The HUD selection: beam, missile, super missile, power bomb, grapple, or X-ray.</summary>
    public required HudItem HudItem { get; init; }

    /// <summary>The acquired reserve-tank capacity in energy-point units.</summary>
    public required int MaxReserveEnergy { get; init; }

    /// <summary>The energy-point total currently stored in reserve tanks.</summary>
    public required int ReserveEnergy { get; init; }

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
            EquippedItems = samus.EquippedItems,
            CollectedItems = samus.CollectedItems,
            EquippedBeams = samus.EquippedBeams,
            CollectedBeams = samus.CollectedBeams,
            ReserveMode = samus.ReserveTankMode,
            Health = samus.Health,
            MaxHealth = samus.MaxHealth,
            Missiles = samus.Missiles,
            MaxMissiles = samus.MaxMissiles,
            SuperMissiles = samus.SuperMissiles,
            MaxSuperMissiles = samus.MaxSuperMissiles,
            PowerBombs = samus.PowerBombs,
            MaxPowerBombs = samus.MaxPowerBombs,
            HudItem = samus.SelectedHudItem,
            MaxReserveEnergy = samus.MaxReserveEnergy,
            ReserveEnergy = samus.ReserveEnergy,
            Language = context.Language,
            GameTime = context.GameTime,
            StartupRoute = system.SavedStartupRoute,
            Checkpoint = Ensure.NotNull(context.Checkpoint),
            Progress = system.Progress.Copy(),
        };
    }
}
