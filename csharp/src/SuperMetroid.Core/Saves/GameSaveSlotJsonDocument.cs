using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Saves;

/// <summary>One valid save slot expressed as named gameplay domains.</summary>
/// <remarks>
/// The numeric fields are JSON numbers, each accepted only inside the range its quantity can take; the reserve mode and
/// HUD item are written by name.
/// </remarks>
public sealed record GameSaveSlotJsonDocument
{
    /// <summary>The zero-based save slot whose state this object represents.</summary>
    public required int Slot { get; init; }

    /// <summary>The load station the save resumes at, by its <see cref="LoadStation.Name"/> (schema nine, #1723).</summary>
    public required string Checkpoint { get; init; }

    /// <summary>Current and acquired energy, ammunition, and reserve quantities.</summary>
    public required SaveResourcesJsonDocument Resources { get; init; }

    /// <summary>Collected and equipped item/beam bit sets.</summary>
    public required SaveInventoryJsonDocument Inventory { get; init; }

    /// <summary>The four-component gameplay clock.</summary>
    public required SaveGameTimeJsonDocument GameTime { get; init; }

    /// <summary>The seven configurable gameplay actions as controller button masks.</summary>
    public required SaveControllerJsonDocument Controller { get; init; }

    /// <summary>Persistent world events, room bits, station bits, and explored map tiles.</summary>
    public required SaveProgressionJsonDocument Progression { get; init; }

    /// <summary>The reserve tanks' mode: none, auto or manual.</summary>
    public required ReserveTankMode ReserveMode { get; init; }

    /// <summary>The selected HUD item.</summary>
    public required HudItem HudItem { get; init; }

    /// <summary>Whether backward walking remains enabled after the options screen closes.</summary>
    public required bool MoonwalkEnabled { get; init; }

    /// <summary>Whether door transitions automatically cancel the selected HUD item.</summary>
    public required bool IconCancelEnabled { get; init; }

    /// <summary>Whether the alternate Japanese text setting is active.</summary>
    public required bool JapaneseText { get; init; }

    /// <summary>The startup route the save resumes by; a save without one resumes the main game.</summary>
    public SaveLoadingGameState StartupRoute { get; init; } = SaveLoadingGameState.MainGame;
}
