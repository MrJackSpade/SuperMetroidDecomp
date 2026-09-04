using SuperMetroid.Core.Game;
using SuperMetroid.Core.Input;

namespace SuperMetroid.Core.Frontend;

/// <summary>Versioned, human-readable representation of all three cartridge save slots.</summary>
public sealed record GameSaveJsonDocument
{
    public int SchemaVersion { get; init; } = GameSaveJsonFormat.SchemaVersion;
    public required int SelectedSlot { get; init; }
    public required GameSaveSlotJsonDocument?[] Slots { get; init; }

    /// <summary>
    /// Offset-labelled copy of the complete SRAM image used solely to preserve bytes not yet
    /// represented by named properties. On load, every named property below is authoritative
    /// and is overlaid onto this image before cartridge checksums are rebuilt.
    /// </summary>
    public required NativeSramPageJsonDocument[] PreservedUntranslatedSram { get; init; }
}

/// <summary>One valid cartridge slot expressed as named gameplay domains.</summary>
public sealed record GameSaveSlotJsonDocument
{
    public required int Slot { get; init; }
    public required SaveCheckpointJsonDocument Checkpoint { get; init; }
    public required SaveResourcesJsonDocument Resources { get; init; }
    public required SaveInventoryJsonDocument Inventory { get; init; }
    public required SaveGameTimeJsonDocument GameTime { get; init; }
    public required SaveControllerJsonDocument Controller { get; init; }
    public required SaveProgressionJsonDocument Progression { get; init; }
    public required ushort ReserveMode { get; init; }
    public required ushort HudItem { get; init; }
    public required bool MoonwalkEnabled { get; init; }
    public required bool IconCancelEnabled { get; init; }
    public required ushort CartridgeDebugFlag { get; init; }
    public required ushort NewFileMarker { get; init; }
}

public sealed record SaveCheckpointJsonDocument(
    AreaId Area,
    ushort SaveStation);

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
    ushort MaxReserveEnergy);

public sealed record SaveInventoryJsonDocument(
    SamusEquipmentFlags CollectedItems,
    SamusEquipmentFlags EquippedItems,
    SamusBeamFlags CollectedBeams,
    SamusBeamFlags EquippedBeams);

public sealed record SaveGameTimeJsonDocument(
    ushort Hours,
    ushort Minutes,
    ushort Seconds,
    ushort Frames);

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
    public required string[] Events { get; init; }
    public required BossBits[] BossFlagsByAreaIndex { get; init; }
    public required int[] RoomChozoBits { get; init; }
    public required int[] CollectedItemBits { get; init; }
    public required int[] OpenedDoorBits { get; init; }
    public required int[] UsedSaveStationBits { get; init; }
    public required int[] AcquiredMapStationBits { get; init; }
    public required ExploredAreaJsonDocument[] ExploredAreas { get; init; }
}

public sealed record ExploredAreaJsonDocument(
    AreaId Area,
    MapCoordinateJsonDocument[] Tiles);

public sealed record MapCoordinateJsonDocument(int X, int Y);

public sealed record NativeSramPageJsonDocument(
    string Offset,
    string Bytes);
