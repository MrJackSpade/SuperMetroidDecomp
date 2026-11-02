namespace SuperMetroid.Core.Frontend.FileSelect.Artwork;

/// <summary>
/// Where file select places its slot fields and actors and how long its looping actors hold each frame: the part of the
/// installed file-select presentation the game reads (#1502).
/// </summary>
public sealed record FileSelectPlacements
{
    /// <summary>Three dynamic save-field layouts in slot order for the main file-select page.</summary>
    public required FileSelectSlotFieldDocument[] MainSlots { get; init; }
    /// <summary>Three dynamic save-field layouts in slot order for copy and clear pages.</summary>
    public required FileSelectSlotFieldDocument[] DataSlots { get; init; }
    /// <summary>Each bordered heading's origin in screen pixels.</summary>
    public required IReadOnlyDictionary<FileSelectBorder, Position16<SpritePixels>> BorderAnchors { get; init; }
    /// <summary>Six missile-cursor origins in main-menu selection order, in screen pixels.</summary>
    public required Position16<SpritePixels>[] MainCursorAnchors { get; init; }
    /// <summary>Four missile-cursor origins in data-management selection order, in screen pixels.</summary>
    public required Position16<SpritePixels>[] DataCursorAnchors { get; init; }
    /// <summary>Two missile-cursor origins in confirmation-option order, in screen pixels.</summary>
    public required Position16<SpritePixels>[] ConfirmationCursorAnchors { get; init; }
    /// <summary>Three helmet origins in save-slot order, in screen pixels.</summary>
    public required Position16<SpritePixels>[] HelmetAnchors { get; init; }
    /// <summary>Each copy or clear page's slot-letter anchor, in tile cells inside the page.</summary>
    public required IReadOnlyDictionary<FileSelectAnchor, Position16<TilemapCells>> DynamicAnchors { get; init; }
    /// <summary>Menu updates per missile-cursor animation frame, in the range 1-65535.</summary>
    public required int CursorFrameDuration { get; init; }
    /// <summary>Menu updates per selected-slot helmet animation frame, in the range 1-65535.</summary>
    public required int HelmetFrameDuration { get; init; }
}
