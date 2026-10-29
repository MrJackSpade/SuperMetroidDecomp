namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>Which artwork a room's BG1 and BG2 blocks show (#1357, #1466).</summary>
public enum RoomBlockArtwork
{
    /// <summary>The room tileset's artwork, as every room but the Ceres Ridley arena shows it.</summary>
    Tileset,

    /// <summary>
    /// The sprites' artwork: the Ceres Ridley arena's setup points its backgrounds there, because its Mode 7 tileset fills
    /// the tileset's.
    /// </summary>
    Sprites,
}
