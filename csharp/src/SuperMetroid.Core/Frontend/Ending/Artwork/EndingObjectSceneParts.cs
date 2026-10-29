using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>The ending's tilemaps the game binds (#633, #1463); the character atlases the ending's sheets are composed from belong to the import pipeline.</summary>
public record EndingObjectSceneParts
{
    /// <summary>Four 32-by-32-cell explosion maps, ordered by <see cref="EndingObjectFragmentId"/>.</summary>
    public required IReadOnlyList<RoomBackgroundTilemapAtlas> Fragments { get; init; }

    /// <summary>$0800-byte, 32-by-32-cell BG2 waiting-scene map.</summary>
    public required RoomBackgroundTilemapAtlas WaitingTilemap { get; init; }

    /// <summary>32-by-32-cell BG3 transformation map.</summary>
    public required RoomBackgroundTilemapAtlas PostCreditsFragmentB { get; init; }

    /// <summary>$0800-byte, 32-by-32-cell final-logo map.</summary>
    public required RoomBackgroundTilemapAtlas PostShotLogoMap { get; init; }
}
