using SuperMetroid.Core.Input;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Saves;

/// <summary>Decoded debugger view of the player fields and checkpoint in one valid slot.</summary>
public sealed record SuperMetroidSaveSlot(
    int Slot,
    ushort EquippedItems,
    ushort CollectedItems,
    ushort EquippedBeams,
    ushort CollectedBeams,
    ushort ReserveMode,
    ushort Health,
    ushort MaxHealth,
    ushort Missiles,
    ushort MaxMissiles,
    ushort SuperMissiles,
    ushort MaxSuperMissiles,
    ushort PowerBombs,
    ushort MaxPowerBombs,
    ushort HudItem,
    ushort MaxReserveEnergy,
    ushort ReserveEnergy,
    ushort GameTimeFrames,
    ushort GameTimeSeconds,
    ushort GameTimeMinutes,
    ushort GameTimeHours,
    byte[] EventBytes,
    byte[] BossBytes,
    byte[] RoomChozoBytes,
    byte[] CollectedItemBytes,
    byte[] OpenedDoorBytes,
    byte[] UsedSaveStationBytes,
    byte[] MapStationBytes,
    byte[] ExploredMapBytes,
    ushort LoadingGameState,
    ushort SaveStation,
    ushort Area)
{
    /// <summary>
    /// The startup route $82:EEB4 selects for <see cref="LoadingGameState"/>; a word the
    /// dispatcher does not name resumes the main game.
    /// </summary>
    public SaveLoadingGameState StartupRoute => Enum.IsDefined((SaveLoadingGameState)LoadingGameState)
        ? (SaveLoadingGameState)LoadingGameState
        : SaveLoadingGameState.MainGame;

    /// <summary>The checksum-backed seven-action controller permutation.</summary>
    public ControllerBindings ControllerBindings { get; init; } = ControllerBindings.Default;

    /// <summary>Saved nonzero WRAM word <c>$09E4</c>.</summary>
    public bool MoonwalkEnabled { get; init; }

    /// <summary>Checksummed cartridge word at WRAM <c>$09E6</c>.</summary>
    public ushort DebugFlag { get; init; }

    /// <summary>Checksummed new-file marker at WRAM <c>$09E8</c>.</summary>
    public ushort NewFileMarker { get; init; }

    /// <summary>Saved nonzero WRAM word <c>$09EA</c>.</summary>
    public bool IconCancelEnabled { get; init; }

    /// <summary>The persistent excess-missile accumulator mirrored from WRAM <c>$09D8</c>.</summary>
    public ushort ReserveMissiles { get; init; }

    /// <summary>Whether the alternate Japanese text setting stored at WRAM <c>$09E2</c> is enabled.</summary>
    public bool JapaneseText { get; init; }

    /// <summary>The cumulative item-PLM setup count from WRAM <c>$7E:D91A</c>, not a completion percentage.</summary>
    public ushort LoadedItemCount { get; init; }

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
    public void ApplyTo(SamusState samus, Bank80SystemState system)
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
        system.LoadSavedLoadingGameState(LoadingGameState);
        system.LoadedItemCount = LoadedItemCount;
    }
}
