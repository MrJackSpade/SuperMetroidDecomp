using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;

/// <summary>The installed reserve-strip layout document (#1638): the anchors the game draws the strip at.</summary>
public sealed record PauseReserveTankLayoutDocument
{
    /// <summary>Gets the layout revision.</summary>
    public required int Version { get; init; }

    /// <summary>Gets the six screen-pixel anchors for five tank positions and the trailing cap.</summary>
    public required Position16<SpritePixels>[] Anchors { get; init; }
}
