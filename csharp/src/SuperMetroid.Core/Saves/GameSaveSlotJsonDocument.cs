
namespace SuperMetroid.Core.Saves;

/// <summary>One valid cartridge slot expressed as named gameplay domains.</summary>
/// <remarks>
/// The numeric fields are JSON numbers. The codec accepts each only inside the range the slot can hold
/// (<see cref="SaveFieldValue"/>) and decodes the reserve mode and HUD item into their domains.
/// </remarks>
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

    /// <summary>The reserve-tank mode's number: zero unset, one auto, or two manual.</summary>
    public required int ReserveMode { get; init; }

    /// <summary>The selected HUD item's number in the six-entry selection ring.</summary>
    public required int HudItem { get; init; }

    /// <summary>Whether backward walking remains enabled after the options screen closes.</summary>
    public required bool MoonwalkEnabled { get; init; }

    /// <summary>Whether door transitions automatically cancel the selected HUD item.</summary>
    public required bool IconCancelEnabled { get; init; }

    /// <summary>Whether the alternate Japanese text setting is active.</summary>
    public required bool JapaneseText { get; init; }

    /// <summary>
    /// The cumulative item-PLM setup count, not a percentage. Gameplay wraps it to zero after
    /// <see cref="SaveFieldValue.MaximumValue"/>; the save only carries the value it reached.
    /// </summary>
    public required int LoadedItemCount { get; init; }

    /// <summary>The checksummed cartridge debug value, carried unchanged.</summary>
    public required int CartridgeDebugFlag { get; init; }

    /// <summary>The checksummed new-file initialization marker, carried unchanged.</summary>
    public required int NewFileMarker { get; init; }

    /// <summary>
    /// The startup dispatcher route, carried unchanged including values the dispatcher does not name. Older JSON
    /// saves omitted this translated field and therefore retain the ordinary main-game default.
    /// </summary>
    public int LoadingGameState { get; init; } = SaveLoadingGameStates.MainGame;
}
