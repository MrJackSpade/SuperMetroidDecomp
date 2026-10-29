using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;

/// <summary>Defines editable pause equipment-selector placement and animation.</summary>
public sealed record PauseSelectorDocument
{
    /// <summary>Gets the document schema revision.</summary>
    public required int Version { get; init; }
    /// <summary>Gets the initial selector dwell time in update ticks.</summary>
    public required int InitialDurationTicks { get; init; }
    /// <summary>Gets the OBJ palette index from zero through seven.</summary>
    public required int Palette { get; init; }
    /// <summary>Gets all sixteen named selector anchors in screen pixels.</summary>
    public required Dictionary<string, Position16<SpritePixels>> Anchors { get; init; }
    /// <summary>Gets named selector frames as sprite-part compositions.</summary>
    public required Dictionary<string, SpriteVisualPart[]> Frames { get; init; }
    /// <summary>Gets the ordered cyclic animation phases.</summary>
    public required PauseSelectorPhase[] Animation { get; init; }
}
