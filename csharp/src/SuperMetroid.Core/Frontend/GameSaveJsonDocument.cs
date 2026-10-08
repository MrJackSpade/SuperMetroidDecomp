using SuperMetroid.Core.Game;
using SuperMetroid.Core.Input;

namespace SuperMetroid.Core.Frontend;

/// <summary>Versioned, human-readable representation of all three cartridge save slots.</summary>
public sealed record GameSaveJsonDocument
{
    /// <summary>The file-format revision used to select compatible decoding and migration rules.</summary>
    public int SchemaVersion { get; init; } = GameSaveJsonFormat.SchemaVersion;
    internal int SourceSchemaVersion { get; init; } = GameSaveJsonFormat.SchemaVersion;

    /// <summary>The zero-based file-select slot persisted globally; valid values are 0 through 2.</summary>
    public required int SelectedSlot { get; init; }

    /// <summary>The three fixed cartridge slots in index order; <see langword="null"/> entries are empty or invalid.</summary>
    public required GameSaveSlotJsonDocument?[] Slots { get; init; }

    /// <summary>Global completion marker that unlocks the fourth attract-demo set.</summary>
    public required bool GameCompleted { get; init; }
}

/// <summary>One valid cartridge slot expressed as named gameplay domains.</summary>
public sealed record GameSaveSlotJsonDocument
{
    /// <summary>The zero-based cartridge slot whose state this object represents.</summary>
    public required int Slot { get; init; }

    /// <summary>The area and load-station pair used to resume gameplay.</summary>
    public required SaveCheckpointJsonDocument Checkpoint { get; init; }

    /// <summary>Current and acquired energy, ammunition, and reserve quantities.</summary>
    public required SaveResourcesJsonDocument Resources { get; init; }

    /// <summary>Collected and equipped item/beam bit sets.</summary>
    public required SaveInventoryJsonDocument Inventory { get; init; }

    /// <summary>The cartridge-compatible four-component gameplay clock.</summary>
    public required SaveGameTimeJsonDocument GameTime { get; init; }

    /// <summary>The seven configurable gameplay actions as SNES button masks.</summary>
    public required SaveControllerJsonDocument Controller { get; init; }

    /// <summary>Persistent world events, room bits, station bits, and explored map tiles.</summary>
    public required SaveProgressionJsonDocument Progression { get; init; }

    /// <summary>The native reserve-tank mode word: zero unset, one auto, or two manual.</summary>
    public required ushort ReserveMode { get; init; }

    /// <summary>The native HUD item-selection index saved with the slot.</summary>
    public required ushort HudItem { get; init; }

    /// <summary>Whether backward walking remains enabled after the options screen closes.</summary>
    public required bool MoonwalkEnabled { get; init; }

    /// <summary>Whether door transitions automatically cancel the selected HUD item.</summary>
    public required bool IconCancelEnabled { get; init; }

    /// <summary>Whether the alternate Japanese text setting is active.</summary>
    public required bool JapaneseText { get; init; }

    /// <summary>The cumulative item-PLM setup count, retained as a native word rather than a percentage.</summary>
    public required ushort LoadedItemCount { get; init; }

    /// <summary>The checksummed cartridge debug word mirrored from WRAM <c>$09E6</c>.</summary>
    public required ushort CartridgeDebugFlag { get; init; }

    /// <summary>The checksummed new-file initialization marker mirrored from WRAM <c>$09E8</c>.</summary>
    public required ushort NewFileMarker { get; init; }
    /// <summary>
    /// Native $7E:D914 dispatcher word. Older JSON saves omitted this translated field and
    /// therefore retain the ordinary state-five default.
    /// </summary>
    public ushort LoadingGameState { get; init; } = SaveLoadingGameStates.MainGame;
}

/// <summary>A cartridge load point expressed with typed area identity.</summary>
/// <param name="Area">Retail world area that owns the load-station table.</param>
/// <param name="SaveStation">Zero-based load-station index within <paramref name="Area"/>.</param>
public sealed record SaveCheckpointJsonDocument(
    AreaId Area,
    ushort SaveStation);

/// <summary>Current and acquired player-resource quantities persisted in a save slot.</summary>
/// <param name="Health">Current energy in energy-point units.</param>
/// <param name="MaxHealth">Acquired energy capacity in energy-point units.</param>
/// <param name="Missiles">Current ordinary missile count.</param>
/// <param name="MaxMissiles">Acquired ordinary missile capacity.</param>
/// <param name="SuperMissiles">Current super missile count.</param>
/// <param name="MaxSuperMissiles">Acquired super missile capacity.</param>
/// <param name="PowerBombs">Current power bomb count.</param>
/// <param name="MaxPowerBombs">Acquired power bomb capacity.</param>
/// <param name="ReserveEnergy">Energy points currently stored in reserve tanks.</param>
/// <param name="MaxReserveEnergy">Acquired reserve-tank capacity in energy points.</param>
/// <param name="ReserveMissiles">Persistent excess-missile accumulator from WRAM <c>$09D8</c>.</param>
public sealed record SaveResourcesJsonDocument(
    ushort Health,
    ushort MaxHealth,
    ushort Missiles,
    ushort MaxMissiles,
    ushort SuperMissiles,
    ushort MaxSuperMissiles,
    ushort PowerBombs,
    ushort MaxPowerBombs,
    ushort ReserveEnergy,
    ushort MaxReserveEnergy,
    ushort ReserveMissiles);

/// <summary>Collected and currently enabled equipment bit sets.</summary>
/// <param name="CollectedItems">All acquired suit, mobility, explosive, and exploration item flags.</param>
/// <param name="EquippedItems">The acquired item flags currently enabled for gameplay.</param>
/// <param name="CollectedBeams">All acquired beam-component flags.</param>
/// <param name="EquippedBeams">The acquired beam components currently enabled; retail UI excludes Spazer with Plasma.</param>
public sealed record SaveInventoryJsonDocument(
    SamusEquipmentFlags CollectedItems,
    SamusEquipmentFlags EquippedItems,
    SamusBeamFlags CollectedBeams,
    SamusBeamFlags EquippedBeams);

/// <summary>The gameplay-update-based clock persisted by the cartridge.</summary>
/// <param name="Hours">Hours component, normally saturated at 99.</param>
/// <param name="Minutes">Minutes component, normally from 0 through 59.</param>
/// <param name="Seconds">Seconds component, normally from 0 through 59.</param>
/// <param name="Frames">Accepted-update component, normally from 0 through 59.</param>
public sealed record SaveGameTimeJsonDocument(
    ushort Hours,
    ushort Minutes,
    ushort Seconds,
    ushort Frames);

/// <summary>The seven configurable actions, each represented by one SNES controller bit.</summary>
/// <param name="Shoot">Button assigned to firing beams and weapons.</param>
/// <param name="Jump">Button assigned to jumping.</param>
/// <param name="Dash">Button assigned to running and speed-boost input.</param>
/// <param name="ItemSelect">Button assigned to cycling the HUD item selection.</param>
/// <param name="ItemCancel">Button assigned to returning the HUD selection to the beam.</param>
/// <param name="AimUp">Button assigned to upward diagonal aiming.</param>
/// <param name="AimDown">Button assigned to downward diagonal aiming.</param>
public sealed record SaveControllerJsonDocument(
    SnesButton Shoot,
    SnesButton Jump,
    SnesButton Dash,
    SnesButton ItemSelect,
    SnesButton ItemCancel,
    SnesButton AimUp,
    SnesButton AimDown);

/// <summary>Named bit-index lists retain every persistent progression plane without Base64.</summary>
public sealed record SaveProgressionJsonDocument
{
    /// <summary>Canonical names of set global event bits; unknown numeric bits are rejected by the JSON codec.</summary>
    public required string[] Events { get; init; }

    /// <summary>One composable boss-state byte for each native area index.</summary>
    public required BossBits[] BossFlagsByAreaIndex { get; init; }

    /// <summary>Set bit indices in the 64-byte room Chozo-orb persistence plane.</summary>
    public required int[] RoomChozoBits { get; init; }

    /// <summary>Set bit indices in the 64-byte collected-item persistence plane.</summary>
    public required int[] CollectedItemBits { get; init; }

    /// <summary>Set bit indices in the 64-byte opened-door persistence plane.</summary>
    public required int[] OpenedDoorBits { get; init; }

    /// <summary>Set bit indices in the 16-byte save-station and elevator persistence plane.</summary>
    public required int[] UsedSaveStationBits { get; init; }

    /// <summary>Set bit indices in the 12-byte acquired-map-station persistence plane.</summary>
    public required int[] AcquiredMapStationBits { get; init; }

    /// <summary>Explored map cells grouped by the six persisted Zebes areas.</summary>
    public required ExploredAreaJsonDocument[] ExploredAreas { get; init; }
}

/// <summary>Explored map cells belonging to one persisted Zebes area.</summary>
/// <param name="Area">Area whose map coordinate space owns the cells.</param>
/// <param name="Tiles">Distinct explored 8-by-8 map-tile coordinates in that area.</param>
public sealed record ExploredAreaJsonDocument(
    AreaId Area,
    MapCoordinateJsonDocument[] Tiles);

/// <summary>A zero-based cell in an area's 64-by-32 map grid.</summary>
/// <param name="X">Horizontal map-tile coordinate, from 0 through 63.</param>
/// <param name="Y">Vertical map-tile coordinate, from 0 through 31.</param>
public sealed record MapCoordinateJsonDocument(int X, int Y);
