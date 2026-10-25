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
    /// <summary>The checksummed cartridge debug value, carried unchanged; a new file holds one.</summary>
    public int DebugFlag { get; init; } = 1;
    /// <summary>The checksummed new-file marker, carried unchanged; a new file holds one.</summary>
    public int NewFileMarker { get; init; } = 1;

    /// <summary>Whether door transitions automatically clear the selected HUD item.</summary>
    public bool IconCancelEnabled { get; init; }

    /// <summary>The persistent excess-missile accumulator written to WRAM <c>$09D8</c>.</summary>
    public int ReserveMissiles { get; init; }

    /// <summary>Whether the alternate Japanese text setting written to WRAM <c>$09E2</c> is enabled.</summary>
    public bool JapaneseText { get; init; }

    /// <summary>
    /// The cumulative item-PLM setup count. Gameplay wraps it to zero after <see cref="SaveFieldValue.MaximumValue"/>;
    /// the snapshot only carries the value it reached.
    /// </summary>
    public int LoadedItemCount { get; init; }

    /// <summary>Bit mask of collected items currently enabled on Samus.</summary>
    public SamusEquipmentFlags EquippedItems { get; init; }

    /// <summary>Bit mask of item upgrades Samus has collected, including unequipped upgrades.</summary>
    public SamusEquipmentFlags CollectedItems { get; init; }

    /// <summary>Bit mask of collected beam upgrades currently enabled on Samus.</summary>
    public SamusBeamFlags EquippedBeams { get; init; }

    /// <summary>Bit mask of all beam upgrades Samus has collected.</summary>
    public SamusBeamFlags CollectedBeams { get; init; }

    /// <summary>The native reserve-tank mode word; zero is unset, one is auto, and two is manual.</summary>
    public ReserveTankMode ReserveMode { get; init; }

    /// <summary>Samus's current energy in energy-point units.</summary>
    public int Health { get; init; } = 99;

    /// <summary>Samus's energy capacity in energy-point units.</summary>
    public int MaxHealth { get; init; } = 99;

    /// <summary>The current ordinary missile count.</summary>
    public int Missiles { get; init; }

    /// <summary>The acquired ordinary missile capacity.</summary>
    public int MaxMissiles { get; init; }

    /// <summary>The current super missile count.</summary>
    public int SuperMissiles { get; init; }

    /// <summary>The acquired super missile capacity.</summary>
    public int MaxSuperMissiles { get; init; }

    /// <summary>The current power bomb count.</summary>
    public int PowerBombs { get; init; }

    /// <summary>The acquired power bomb capacity.</summary>
    public int MaxPowerBombs { get; init; }

    /// <summary>The native HUD selection index: beam, missile, super missile, power bomb, grapple, or X-ray.</summary>
    public HudItem HudItem { get; init; }

    /// <summary>The acquired reserve-tank capacity in energy-point units.</summary>
    public int MaxReserveEnergy { get; init; }

    /// <summary>The energy-point total currently stored in reserve tanks.</summary>
    public int ReserveEnergy { get; init; }

    /// <summary>The sub-second gameplay counter, normally in the inclusive range 0 through 59.</summary>
    public int GameTimeFrames { get; init; }

    /// <summary>The gameplay clock's seconds component, normally in the inclusive range 0 through 59.</summary>
    public int GameTimeSeconds { get; init; }

    /// <summary>The gameplay clock's minutes component, normally in the inclusive range 0 through 59.</summary>
    public int GameTimeMinutes { get; init; }

    /// <summary>The gameplay clock's hours component, saturated by gameplay at 99.</summary>
    public int GameTimeHours { get; init; }

    /// <summary>The startup dispatcher route, carried unchanged including values the dispatcher does not name.</summary>
    public int LoadingGameState { get; init; } = SaveLoadingGameStateCodec.Word(SaveLoadingGameState.MainGame);

    /// <summary>The zero-based load-station index within <see cref="Area"/>.</summary>
    public int SaveStation { get; init; }

    /// <summary>The retail area index that owns <see cref="SaveStation"/>.</summary>
    public int Area { get; init; }

    /// <summary>Eight persistent event bytes copied without reinterpretation from the shared game state.</summary>
    public byte[] EventBytes { get; init; } = new byte[SharedGameState.EventByteCount];

    /// <summary>
    /// One boss-state flag set per native area slot, including any unproven bits a corrupting write left, which the
    /// SRAM encoding carries losslessly.
    /// </summary>
    public BossBits[] BossFlags { get; init; } = new BossBits[SharedGameState.AreaCount];

    /// <summary>The 64-byte room-indexed bitset for opened Chozo item orbs.</summary>
    public byte[] RoomChozoBytes { get; init; } =
        new byte[SharedGameState.RoomChozoBitByteCount];

    /// <summary>The 64-byte room-indexed bitset for collected item PLMs.</summary>
    public byte[] CollectedItemBytes { get; init; } =
        new byte[SharedGameState.ItemBitByteCount];

    /// <summary>The 64-byte room-indexed bitset for doors permanently opened by the player.</summary>
    public byte[] OpenedDoorBytes { get; init; } =
        new byte[SharedGameState.DoorBitByteCount];

    /// <summary>The 16-byte per-area bitset for visited save stations and elevators.</summary>
    public byte[] UsedSaveStationBytes { get; init; } =
        new byte[SharedGameState.UsedSaveStationByteCount];

    /// <summary>The 12-byte per-area bitset for activated map stations.</summary>
    public byte[] MapStationBytes { get; init; } =
        new byte[SharedGameState.MapStationByteCount];

    /// <summary>
    /// Unpacked explored-map bytes in area-major order; SRAM encoding interleaves the six
    /// persisted Zebes areas and omits the Ceres prologue.
    /// </summary>
    public byte[] ExploredMapBytes { get; init; } = new byte[
        SharedGameState.ExploredMapAreaCount * SharedGameState.ExploredMapBytesPerArea];

    /// <summary>Captures the translated gameplay owners into a detached SRAM-encoding snapshot.</summary>
    /// <param name="samus">Source of player inventory, ammunition, energy, and reserve state.</param>
    /// <param name="system">Source of persistent events, bosses, room bits, maps, and load state.</param>
    /// <param name="context">The save location, timer, bindings and options a save snapshot records with Samus and system state (#633).</param>
    /// <returns>A detached copy safe to pass to <see cref="SaveSlotStore.SaveSlot"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="samus"/> or <paramref name="system"/> is <see langword="null"/>.</exception>
    public static SuperMetroidSaveSnapshot Capture(SamusState samus, SharedGameState system, SaveSnapshotContext context)
    {
        Ensure.NotNull(samus);
        Ensure.NotNull(system);
        var events = new byte[SharedGameState.EventByteCount];
        var bosses = new BossBits[SharedGameState.AreaCount];
        var roomChozo = new byte[SharedGameState.RoomChozoBitByteCount];
        var collectedItems = new byte[SharedGameState.ItemBitByteCount];
        var openedDoors = new byte[SharedGameState.DoorBitByteCount];
        var usedSaveStations = new byte[SharedGameState.UsedSaveStationByteCount];
        var mapStations = new byte[SharedGameState.MapStationByteCount];
        var exploredMap = new byte[
            SharedGameState.ExploredMapAreaCount * SharedGameState.ExploredMapBytesPerArea];
        for (int index = 0; index < events.Length; index++)
            events[index] = system.GetEventByteRaw(index);
        for (int index = 0; index < bosses.Length; index++)
            bosses[index] = (BossBits)system.GetBossBitsRaw(index);
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
        for (int areaIndex = 0; areaIndex < SharedGameState.ExploredMapAreaCount; areaIndex++)
        {
            for (int byteIndex = 0;
                byteIndex < SharedGameState.ExploredMapBytesPerArea;
                byteIndex++)
            {
                exploredMap[areaIndex * SharedGameState.ExploredMapBytesPerArea + byteIndex] =
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
            Area = AreaIds.ToIndex(context.Area),
            EventBytes = events,
            BossFlags = bosses,
            RoomChozoBytes = roomChozo,
            CollectedItemBytes = collectedItems,
            OpenedDoorBytes = openedDoors,
            UsedSaveStationBytes = usedSaveStations,
            MapStationBytes = mapStations,
            ExploredMapBytes = exploredMap,
        };
    }
}
