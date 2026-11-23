using System.Text.Json.Serialization;
using SuperMetroid.Core.Frontend.FileSelect.Artwork;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Frontend.FileSelect.Artwork;

/// <summary>
/// The installed file-select JSON (#1463, #1638): the slot fields, anchors and durations the game places its actors by, in
/// pixel space. The pages, patches, glyphs and sprite compositions the file-select pictures are composed from are import
/// data (<see cref="FileSelectArtworkDocument"/>), as their cells are the original's packed tile placements.
/// </summary>
/// <remarks>Init-only properties hold caller-owned mutable arrays and dictionaries.</remarks>
public sealed partial record FileSelectLayoutDocument
{
    /// <summary>Schema version; must equal <see cref="FileSelectPresentationFormat.Version"/>.</summary>
    [JsonPropertyOrder(1)]
    public required int Version { get; init; }
    /// <summary>Three dynamic save-field layouts in slot order for the main file-select page.</summary>
    [JsonPropertyOrder(6)]
    public required Dictionary<string, FileSelectSlotFieldDocument> MainSlots { get; init; }
    /// <summary>Three dynamic save-field layouts in slot order for copy and clear pages.</summary>
    [JsonPropertyOrder(7)]
    public required Dictionary<string, FileSelectSlotFieldDocument> DataSlots { get; init; }
    /// <summary>Main, copy, and clear border origins in screen pixels, with X=0-511 and Y=0-255.</summary>
    [JsonPropertyOrder(9)]
    public required Dictionary<string, Offset16<ScreenPixels>> BorderAnchors { get; init; }
    /// <summary>The six main-menu choices' missile-cursor origins, keyed by the choice, in screen pixels with X=0-511 and Y=0-255.</summary>
    [JsonPropertyOrder(10)]
    public required Dictionary<string, Offset16<ScreenPixels>> MainCursorAnchors { get; init; }
    /// <summary>The four data-management choices' missile-cursor origins, keyed by the choice, in screen pixels with X=0-511 and Y=0-255.</summary>
    [JsonPropertyOrder(11)]
    public required Dictionary<string, Offset16<ScreenPixels>> DataCursorAnchors { get; init; }
    /// <summary>The Yes and No options' missile-cursor origins, keyed by the option, in screen pixels with X=0-511 and Y=0-255.</summary>
    [JsonPropertyOrder(12)]
    public required Dictionary<string, Offset16<ScreenPixels>> ConfirmationCursorAnchors { get; init; }
    /// <summary>Three helmet origins in save-slot order, in screen pixels with X=0-511 and Y=0-255.</summary>
    [JsonPropertyOrder(13)]
    public required Dictionary<string, Offset16<ScreenPixels>> HelmetAnchors { get; init; }
    /// <summary>Exactly the four named copy/clear slot-letter anchors, each the page pixel its letter is shown at.</summary>
    [JsonPropertyOrder(14)]
    public required Dictionary<string, Position16<PlanePixels>> DynamicAnchors { get; init; }
    /// <summary>Menu-update duration 1-65535 for each of the four looping missile-cursor frames.</summary>
    [JsonPropertyOrder(16)]
    public required int CursorFrameDuration { get; init; }
    /// <summary>Menu-update duration 1-65535 for each selected-slot helmet frame; the engine stops the animation on its last frame.</summary>
    [JsonPropertyOrder(17)]
    public required int HelmetFrameDuration { get; init; }
}
