using SuperMetroid.Core.Input;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Saves;

/// <summary>
/// One saved game (#1475): the player's inventory, resources, clock and checkpoint, and the persistent progress the save
/// keeps (<see cref="Progression"/>, a copy the slot owns). Hosts persist it; no part of it is a memory image.
/// </summary>
/// <param name="Slot">The slot, A through C as 0 through 2.</param>
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
/// <param name="GameTimeFrames">The game clock's frames.</param>
/// <param name="GameTimeSeconds">The game clock's seconds.</param>
/// <param name="GameTimeMinutes">The game clock's minutes.</param>
/// <param name="GameTimeHours">The game clock's hours.</param>
/// <param name="Progression">The persistent progress the save keeps; the slot owns this copy.</param>
/// <param name="LoadingGameState">The saved startup dispatcher value, carried unchanged.</param>
/// <param name="SaveStation">The load station within <paramref name="Area"/>.</param>
/// <param name="Area">The checkpoint's area index.</param>
public sealed record SuperMetroidSaveSlot(
    int Slot,
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
    int GameTimeFrames,
    int GameTimeSeconds,
    int GameTimeMinutes,
    int GameTimeHours,
    PersistentProgress Progression,
    int LoadingGameState,
    int SaveStation,
    int Area)
{
    /// <summary>
    /// The startup route $82:EEB4 selects for <see cref="LoadingGameState"/>; a value the
    /// dispatcher does not name resumes the main game.
    /// </summary>
    public SaveLoadingGameState StartupRoute => SaveLoadingGameStateCodec.Route(LoadingGameState);

    /// <summary>The checksum-backed seven-action controller permutation.</summary>
    public ControllerBindings ControllerBindings { get; init; } = ControllerBindings.Default;

    /// <summary>Saved nonzero WRAM word <c>$09E4</c>.</summary>
    public bool MoonwalkEnabled { get; init; }

    /// <summary>The checksummed cartridge debug value, carried unchanged.</summary>
    public int DebugFlag { get; init; }

    /// <summary>The checksummed new-file marker, carried unchanged.</summary>
    public int NewFileMarker { get; init; }

    /// <summary>Saved nonzero WRAM word <c>$09EA</c>.</summary>
    public bool IconCancelEnabled { get; init; }

    /// <summary>The persistent excess-missile accumulator mirrored from WRAM <c>$09D8</c>.</summary>
    public int ReserveMissiles { get; init; }

    /// <summary>Whether the alternate Japanese text setting stored at WRAM <c>$09E2</c> is enabled.</summary>
    public bool JapaneseText { get; init; }

    /// <summary>
    /// The cumulative item-PLM setup count, not a completion percentage. Gameplay wraps it to zero after
    /// <see cref="SaveFieldValue.MaximumValue"/>; the slot only carries the value it reached.
    /// </summary>
    public int LoadedItemCount { get; init; }

    /// <summary>Restores the subset already represented by the translated Samus owner.</summary>
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
        samus.ReserveMissiles = ReserveMissiles;
    }

    /// <summary>Restores player inventory, the saved progress, the loading state and the item-load count.</summary>
    public void ApplyTo(SamusState samus, SharedGameState system)
    {
        ApplyTo(samus);
        Ensure.NotNull(system);
        system.Progress.CopyFrom(Progression);
        system.LoadSavedLoadingGameState(SaveFieldValue.Encode(LoadingGameState, nameof(LoadingGameState)));
        system.LoadedItemCount = SaveFieldValue.Encode(LoadedItemCount, nameof(LoadedItemCount));
    }
}
