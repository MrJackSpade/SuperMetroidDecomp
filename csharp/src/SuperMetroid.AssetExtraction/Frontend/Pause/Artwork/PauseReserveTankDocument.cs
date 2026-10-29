using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;

/// <summary>Editable JSON schema for the pause screen's reserve-energy strip artwork and placement.</summary>
public sealed record PauseReserveTankDocument
{
    /// <summary>Gets the reserve-tank presentation schema version.</summary>
    public required int Version { get; init; }

    /// <summary>Gets the OBJ palette index, from zero through seven, applied to every reserve-strip part.</summary>
    public required int Palette { get; init; }

    /// <summary>Gets the six screen-pixel anchors for five tank positions and the trailing cap.</summary>
    public required Position16<SpritePixels>[] Anchors { get; init; }

    /// <summary>Gets the ten named full, end-cap, empty, and partial-fill sprite compositions.</summary>
    public required Dictionary<string, SpriteVisualPart[]> Frames { get; init; }
}
