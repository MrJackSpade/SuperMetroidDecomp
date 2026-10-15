using SuperMetroid.Core.Input;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Saves;

/// <summary>Domain snapshot supplied to the native SRAM encoder.</summary>
public sealed record SuperMetroidSaveSnapshot
{
    /// <summary>The seven configurable actions as a validated permutation of SNES button masks.</summary>
    public ControllerBindings ControllerBindings { get; init; } = ControllerBindings.Default;

    /// <summary>Whether the options menu's moonwalk behavior is enabled.</summary>
    public bool MoonwalkEnabled { get; init; }
    /// <summary>Checksummed cartridge word at WRAM <c>$09E6</c>.</summary>
    public ushort DebugFlag { get; init; } = 1;
    /// <summary>Checksummed new-file marker at WRAM <c>$09E8</c>.</summary>
    public ushort NewFileMarker { get; init; } = 1;

    /// <summary>Whether door transitions automatically clear the selected HUD item.</summary>
    public bool IconCancelEnabled { get; init; }

    /// <summary>The persistent excess-missile accumulator written to WRAM <c>$09D8</c>.</summary>
    public ushort ReserveMissiles { get; init; }

    /// <summary>Whether the alternate Japanese text setting written to WRAM <c>$09E2</c> is enabled.</summary>
    public bool JapaneseText { get; init; }

    /// <summary>The cumulative item-PLM setup count persisted at WRAM <c>$7E:D91A</c>.</summary>
    public ushort LoadedItemCount { get; init; }

    /// <summary>Bit mask of collected items currently enabled on Samus.</summary>
    public ushort EquippedItems { get; init; }

    /// <summary>Bit mask of item upgrades Samus has collected, including unequipped upgrades.</summary>
    public ushort CollectedItems { get; init; }

    /// <summary>Bit mask of collected beam upgrades currently enabled on Samus.</summary>
    public ushort EquippedBeams { get; init; }

    /// <summary>Bit mask of all beam upgrades Samus has collected.</summary>
    public ushort CollectedBeams { get; init; }

    /// <summary>The native reserve-tank mode word; zero is unset, one is auto, and two is manual.</summary>
    public ushort ReserveMode { get; init; }

    /// <summary>Samus's current energy in energy-point units.</summary>
    public ushort Health { get; init; } = 99;

    /// <summary>Samus's energy capacity in energy-point units.</summary>
    public ushort MaxHealth { get; init; } = 99;

    /// <summary>The current ordinary missile count.</summary>
    public ushort Missiles { get; init; }

    /// <summary>The acquired ordinary missile capacity.</summary>
    public ushort MaxMissiles { get; init; }

    /// <summary>The current super missile count.</summary>
    public ushort SuperMissiles { get; init; }

    /// <summary>The acquired super missile capacity.</summary>
    public ushort MaxSuperMissiles { get; init; }

    /// <summary>The current power bomb count.</summary>
    public ushort PowerBombs { get; init; }

    /// <summary>The acquired power bomb capacity.</summary>
    public ushort MaxPowerBombs { get; init; }

    /// <summary>The native HUD selection index: beam, missile, super missile, power bomb, grapple, or X-ray.</summary>
    public ushort HudItem { get; init; }

    /// <summary>The acquired reserve-tank capacity in energy-point units.</summary>
    public ushort MaxReserveEnergy { get; init; }

    /// <summary>The energy-point total currently stored in reserve tanks.</summary>
    public ushort ReserveEnergy { get; init; }

    /// <summary>The sub-second gameplay counter, normally in the inclusive range 0 through 59.</summary>
    public ushort GameTimeFrames { get; init; }

    /// <summary>The gameplay clock's seconds component, normally in the inclusive range 0 through 59.</summary>
    public ushort GameTimeSeconds { get; init; }

    /// <summary>The gameplay clock's minutes component, normally in the inclusive range 0 through 59.</summary>
    public ushort GameTimeMinutes { get; init; }

    /// <summary>The gameplay clock's hours component, saturated by gameplay at 99.</summary>
    public ushort GameTimeHours { get; init; }
    /// <summary>Saved dispatcher word at WRAM <c>$7E:D914</c>.</summary>
    public ushort LoadingGameState { get; init; } = SaveLoadingGameStates.MainGame;

    /// <summary>The zero-based load-station index within <see cref="Area"/>.</summary>
    public ushort SaveStation { get; init; }

    /// <summary>The retail area index that owns <see cref="SaveStation"/>.</summary>
    public ushort Area { get; init; }

    /// <summary>Eight persistent event bytes copied without reinterpretation from bank-$80 state.</summary>
    public byte[] EventBytes { get; init; } = new byte[Bank80SystemState.EventByteCount];

    /// <summary>One boss-state byte per native area slot, copied without reinterpretation.</summary>
    public byte[] BossBytes { get; init; } = new byte[Bank80SystemState.AreaCount];

    /// <summary>The 64-byte room-indexed bitset for opened Chozo item orbs.</summary>
    public byte[] RoomChozoBytes { get; init; } =
        new byte[Bank80SystemState.RoomChozoBitByteCount];

    /// <summary>The 64-byte room-indexed bitset for collected item PLMs.</summary>
    public byte[] CollectedItemBytes { get; init; } =
        new byte[Bank80SystemState.ItemBitByteCount];

    /// <summary>The 64-byte room-indexed bitset for doors permanently opened by the player.</summary>
    public byte[] OpenedDoorBytes { get; init; } =
        new byte[Bank80SystemState.DoorBitByteCount];

    /// <summary>The 16-byte per-area bitset for visited save stations and elevators.</summary>
    public byte[] UsedSaveStationBytes { get; init; } =
        new byte[Bank80SystemState.UsedSaveStationByteCount];

    /// <summary>The 12-byte per-area bitset for activated map stations.</summary>
    public byte[] MapStationBytes { get; init; } =
        new byte[Bank80SystemState.MapStationByteCount];

    /// <summary>
    /// Unpacked explored-map bytes in area-major order; SRAM encoding interleaves the six
    /// persisted Zebes areas and omits the Ceres prologue.
    /// </summary>
    public byte[] ExploredMapBytes { get; init; } = new byte[
        Bank80SystemState.ExploredMapAreaCount * Bank80SystemState.ExploredMapBytesPerArea];

    /// <summary>Captures the translated gameplay owners into a detached SRAM-encoding snapshot.</summary>
    /// <param name="samus">Source of player inventory, ammunition, energy, and reserve state.</param>
    /// <param name="system">Source of persistent events, bosses, room bits, maps, and load state.</param>
    /// <param name="context">The save location, timer, bindings and options a save snapshot records with Samus and system state (#633).</param>
    /// <returns>A detached copy safe to pass to <see cref="SuperMetroidSaveRam.SaveSlot"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="samus"/> or <paramref name="system"/> is <see langword="null"/>.</exception>
    public static SuperMetroidSaveSnapshot Capture(SamusState samus, Bank80SystemState system, SaveSnapshotContext context)
    {
        Ensure.NotNull(samus);
        Ensure.NotNull(system);
        var events = new byte[Bank80SystemState.EventByteCount];
        var bosses = new byte[Bank80SystemState.AreaCount];
        var roomChozo = new byte[Bank80SystemState.RoomChozoBitByteCount];
        var collectedItems = new byte[Bank80SystemState.ItemBitByteCount];
        var openedDoors = new byte[Bank80SystemState.DoorBitByteCount];
        var usedSaveStations = new byte[Bank80SystemState.UsedSaveStationByteCount];
        var mapStations = new byte[Bank80SystemState.MapStationByteCount];
        var exploredMap = new byte[
            Bank80SystemState.ExploredMapAreaCount * Bank80SystemState.ExploredMapBytesPerArea];
        for (int index = 0; index < events.Length; index++)
            events[index] = system.GetEventByteRaw(index);
        for (int index = 0; index < bosses.Length; index++)
            bosses[index] = system.GetBossBitsRaw(index);
        for (int index = 0; index < roomChozo.Length; index++)
            roomChozo[index] = system.GetRoomChozoByteRaw(index);
        for (int index = 0; index < collectedItems.Length; index++)
            collectedItems[index] = system.GetCollectedItemByteRaw(index);
        for (int index = 0; index < openedDoors.Length; index++)
            openedDoors[index] = system.GetOpenedDoorByteRaw(index);
        for (int index = 0; index < usedSaveStations.Length; index++)
            usedSaveStations[index] = system.GetUsedSaveStationByteRaw(index);
        for (int index = 0; index < mapStations.Length; index++)
            mapStations[index] = system.GetMapStationByteRaw(index);
        for (int areaIndex = 0; areaIndex < Bank80SystemState.ExploredMapAreaCount; areaIndex++)
        {
            for (int byteIndex = 0;
                byteIndex < Bank80SystemState.ExploredMapBytesPerArea;
                byteIndex++)
            {
                exploredMap[areaIndex * Bank80SystemState.ExploredMapBytesPerArea + byteIndex] =
                    system.GetExploredMapByteRaw(areaIndex, byteIndex);
            }
        }

        return new SuperMetroidSaveSnapshot
        {
            ControllerBindings = (context.ControllerBindings ?? ControllerBindings.Default)
                .RequireRetailPermutation(),
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
            ReserveMissiles = samus.ReserveMissiles,
            JapaneseText = context.JapaneseText,
            LoadedItemCount = system.LoadedItemCount,
            GameTimeFrames = context.GameTime?.Frames ?? 0,
            GameTimeSeconds = context.GameTime?.Seconds ?? 0,
            GameTimeMinutes = context.GameTime?.Minutes ?? 0,
            GameTimeHours = context.GameTime?.Hours ?? 0,
            LoadingGameState = system.SavedLoadingGameState,
            SaveStation = context.SaveStation,
            Area = context.Area,
            EventBytes = events,
            BossBytes = bosses,
            RoomChozoBytes = roomChozo,
            CollectedItemBytes = collectedItems,
            OpenedDoorBytes = openedDoors,
            UsedSaveStationBytes = usedSaveStations,
            MapStationBytes = mapStations,
            ExploredMapBytes = exploredMap,
        };
    }
}
