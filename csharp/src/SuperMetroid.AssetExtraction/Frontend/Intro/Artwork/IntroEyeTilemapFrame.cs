using SuperMetroid.AssetExtraction.Rooms;

namespace SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

/// <summary>Defines one stable 3-by-2 portrait eye tilemap rectangle.</summary>
public sealed record IntroEyeTilemapFrame
{
    /// <summary>Gets the stable <c>eye-frame-N</c> identity for this array position.</summary>
    public required string Id { get; init; }
    /// <summary>Gets six row-major packed-tile components.</summary>
    public required RoomBackgroundTilemapCell[] Cells { get; init; }
}
