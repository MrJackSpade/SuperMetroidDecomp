using SuperMetroid.Core.Input;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Saves;

/// <summary>
/// One saved game (#1475): the player's inventory, resources, clock and checkpoint, and the persistent progress the save
/// keeps (<see cref="Progression"/>, a copy the slot owns). Hosts persist it; no part of it is a memory image.
/// </summary>
/// <param name="Slot">The slot the save is kept in.</param>
/// <param name="EquippedItems">The collected items currently enabled on Samus.</param>
/// <param name="CollectedItems">The item upgrades Samus has collected.</param>
/// <param name="EquippedBeams">The collected beams currently enabled on Samus.</param>
/// <param name="CollectedBeams">The beam upgrades Samus has collected.</param>
/// <param name="ReserveMode">The reserve tanks' mode.</param>
/// <param name="Health">Samus's energy.</param>
/// <param name="MaxHealth">Samus's energy capacity.</param>
/// <param name="Missiles">The missile count.</param>
/// <param name="MaxMissiles">The missile capacity.</param>
/// <param name="SuperMissiles">The super missile count.</param>
/// <param name="MaxSuperMissiles">The super missile capacity.</param>
/// <param name="PowerBombs">The power bomb count.</param>
/// <param name="MaxPowerBombs">The power bomb capacity.</param>
/// <param name="HudItem">The selected HUD item.</param>
/// <param name="MaxReserveEnergy">The reserve tanks' capacity.</param>
/// <param name="ReserveEnergy">The energy stored in reserve tanks.</param>
/// <param name="GameTime">The game clock's reading.</param>
/// <param name="Progression">The persistent progress the save keeps; the slot owns this copy.</param>
/// <param name="StartupRoute">The startup route the save resumes by.</param>
/// <param name="Checkpoint">The load station the save resumes at.</param>
public sealed record SuperMetroidSaveSlot(
    SaveSlot Slot,
    SamusEquipmentFlags EquippedItems,
    SamusEquipmentFlags CollectedItems,
    SamusBeamFlags EquippedBeams,
    SamusBeamFlags CollectedBeams,
    ReserveTankMode ReserveMode,
    int Health,
    int MaxHealth,
    int Missiles,
    int MaxMissiles,
    int SuperMissiles,
    int MaxSuperMissiles,
    int PowerBombs,
    int MaxPowerBombs,
    HudItem HudItem,
    int MaxReserveEnergy,
    int ReserveEnergy,
    GameClockReading GameTime,
    PersistentProgress Progression,
    SaveStartupRoute StartupRoute,
    LoadStation Checkpoint)
{
    /// <summary>The buttons of the seven configurable actions.</summary>
    public required ControllerBindings ControllerBindings { get; init; }

    /// <summary>Whether the saved moonwalk setting is enabled.</summary>
    public required bool MoonwalkEnabled { get; init; }

    /// <summary>Whether the saved icon-cancel setting is enabled.</summary>
    public required bool IconCancelEnabled { get; init; }

    /// <summary>Whether the alternate Japanese text setting is enabled.</summary>
    public required TextLanguage Language { get; init; }

    /// <summary>Restores the subset the Samus state represents.</summary>
    public void ApplyTo(SamusState samus)
    {
        Ensure.NotNull(samus);
        samus.EquippedItems = EquippedItems;
        samus.CollectedItems = CollectedItems;
        samus.EquippedBeams = EquippedBeams;
        samus.CollectedBeams = CollectedBeams;
        samus.ReserveTankMode = ReserveMode;
        samus.Health = Health;
        samus.PreviousHealthForHurtCheck = Health;
        samus.MaxHealth = MaxHealth;
        samus.Missiles = Missiles;
        samus.MaxMissiles = MaxMissiles;
        samus.SuperMissiles = SuperMissiles;
        samus.MaxSuperMissiles = MaxSuperMissiles;
        samus.PowerBombs = PowerBombs;
        samus.MaxPowerBombs = MaxPowerBombs;
        samus.SelectedHudItem = HudItem;
        samus.MaxReserveEnergy = MaxReserveEnergy;
        samus.ReserveEnergy = ReserveEnergy;
    }

    /// <summary>Restores player inventory, the saved progress and the loading state.</summary>
    public void ApplyTo(SamusState samus, SharedGameState system)
    {
        ApplyTo(samus);
        Ensure.NotNull(system);
        system.Progress.CopyFrom(Progression);
        system.LoadSavedStartupRoute(StartupRoute);
    }
}
