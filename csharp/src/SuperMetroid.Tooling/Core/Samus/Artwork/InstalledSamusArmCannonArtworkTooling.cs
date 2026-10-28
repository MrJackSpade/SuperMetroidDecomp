using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.AssetExtraction.Rooms;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Development-tool members of <see cref="InstalledSamusArmCannonArtwork"/>; never linked by player hosts.</summary>
internal static class InstalledSamusArmCannonArtworkTooling
{
    /// <summary>Combines validated placement JSON with the exact number of indexed cannon tile uploads.</summary>
    public static InstalledSamusArmCannonArtwork Load(Stream json, Stream tilePng)
    {
        Ensure.NotNull(json);
        Ensure.NotNull(tilePng);
        InstalledSamusArmCannonArtwork.Placement placement = InstalledSamusArmCannonArtwork.LoadPlacement(json);
        RoomCharacterAtlas tiles = RoomCharacterAtlasPng.Read(tilePng,
            SamusArmCannonArtworkFormat.CoverTileCount *
                SamusDrawingDefinitions.ArmCannon.TileByteCount);
        return InstalledSamusArmCannonArtwork.FromPlacement(placement, tiles);
    }
}
