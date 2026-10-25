using SuperMetroid.Core.Input;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Saves;

/// <summary>Decoded debugger view of the player fields and checkpoint in one valid slot.</summary>
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
    byte[] EventBytes,
    byte[] BossBytes,
    byte[] RoomChozoBytes,
    byte[] CollectedItemBytes,
    byte[] OpenedDoorBytes,
    byte[] UsedSaveStationBytes,
    byte[] MapStationBytes,
    byte[] ExploredMapBytes,
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

    /// <summary>Restores player inventory and all progression bytes represented in SRAM.</summary>
    public void ApplyTo(SamusState samus, SharedGameState system)
    {
        ApplyTo(samus);
        Ensure.NotNull(system);
        system.LoadEventBytes(EventBytes);
        system.LoadBossBytes(BossBytes);
        system.LoadRoomChozoBytes(RoomChozoBytes);
        system.LoadCollectedItemBytes(CollectedItemBytes);
        system.LoadOpenedDoorBytes(OpenedDoorBytes);
        system.LoadUsedSaveStationBytes(UsedSaveStationBytes);
        system.LoadMapStationBytes(MapStationBytes);
        system.LoadExploredMapBytes(ExploredMapBytes);
        system.LoadSavedLoadingGameState(SaveFieldValue.Encode(LoadingGameState, nameof(LoadingGameState)));
        system.LoadedItemCount = SaveFieldValue.Encode(LoadedItemCount, nameof(LoadedItemCount));
    }
}
