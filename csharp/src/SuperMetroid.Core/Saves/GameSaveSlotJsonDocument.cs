
namespace SuperMetroid.Core.Saves;

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
