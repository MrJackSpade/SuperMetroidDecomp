using SuperMetroid.Core.Input;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Saves;

/// <summary>
/// One saved game (#1475): the player's inventory, resources, clock and checkpoint, and the persistent progress the save
/// keeps (<see cref="Progression"/>, a copy the slot owns). Hosts persist it; no part of it is a memory image.
/// </summary>
/// <param name="Slot">The slot the save is kept in.</param>
/// <param name="Inventory">What Samus carries: her items and beams, reserve mode, energy, ammunition and reserve.</param>
/// <param name="HudItem">The selected HUD item.</param>
/// <param name="GameTime">The game clock's reading.</param>
/// <param name="Progression">The persistent progress the save keeps; the slot owns this copy.</param>
/// <param name="StartupRoute">The startup route the save resumes by.</param>
/// <param name="Checkpoint">The load station the save resumes at.</param>
public sealed record SuperMetroidSaveSlot(
    SaveSlot Slot,
    SamusInventory Inventory,
    HudItem HudItem,
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
    public required bool JapaneseText { get; init; }

    /// <summary>Restores the subset the Samus state represents.</summary>
    public void ApplyTo(SamusState samus)
    {
        Ensure.NotNull(samus);
        samus.Inventory = Inventory;
        samus.PreviousHealthForHurtCheck = Inventory.Health;
        samus.SelectedHudItem = HudItem;
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
