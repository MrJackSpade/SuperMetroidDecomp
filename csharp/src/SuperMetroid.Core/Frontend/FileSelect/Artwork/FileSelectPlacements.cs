using SuperMetroid.Core.Saves;

namespace SuperMetroid.Core.Frontend.FileSelect.Artwork;

/// <summary>
/// Where file select places its slot fields and actors and how long its looping actors hold each frame: the part of the
/// installed file-select presentation the game reads (#1502).
/// </summary>
public sealed record FileSelectPlacements
{
    /// <summary>Each save slot's field layout on the main file-select page.</summary>
    public required IReadOnlyDictionary<SaveSlot, FileSelectSlotFieldDocument> MainSlots { get; init; }
    /// <summary>Each save slot's field layout on the copy and clear pages.</summary>
    public required IReadOnlyDictionary<SaveSlot, FileSelectSlotFieldDocument> DataSlots { get; init; }
    /// <summary>Each bordered heading's origin in screen pixels.</summary>
    public required IReadOnlyDictionary<FileSelectBorder, Offset16<ScreenPixels>> BorderAnchors { get; init; }
    /// <summary>Six missile-cursor origins in main-menu selection order, in screen pixels.</summary>
    public required Offset16<ScreenPixels>[] MainCursorAnchors { get; init; }
    /// <summary>Four missile-cursor origins in data-management selection order, in screen pixels.</summary>
    public required Offset16<ScreenPixels>[] DataCursorAnchors { get; init; }
    /// <summary>Two missile-cursor origins in confirmation-option order, in screen pixels.</summary>
    public required Offset16<ScreenPixels>[] ConfirmationCursorAnchors { get; init; }
    /// <summary>Each save slot's helmet origin, in screen pixels.</summary>
    public required IReadOnlyDictionary<SaveSlot, Offset16<ScreenPixels>> HelmetAnchors { get; init; }
    /// <summary>The page pixel each copy or clear page's slot letter is shown at.</summary>
    public required IReadOnlyDictionary<FileSelectAnchor, Position16<PlanePixels>> DynamicAnchors { get; init; }
    /// <summary>Menu updates per missile-cursor animation frame, at least one.</summary>
    public required int CursorFrameDuration { get; init; }
    /// <summary>Menu updates per selected-slot helmet animation frame, at least one.</summary>
    public required int HelmetFrameDuration { get; init; }
}
