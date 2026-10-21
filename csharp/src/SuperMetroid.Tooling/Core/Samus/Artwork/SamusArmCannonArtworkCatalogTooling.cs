using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Samus.Drawing;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Development-tool members of <see cref="SamusArmCannonArtworkCatalog"/>; never linked by player hosts.</summary>
internal static class SamusArmCannonArtworkCatalogTooling
{
    /// <summary>Combines validated placement JSON with the exact number of indexed cannon tile uploads.</summary>
    public static SamusArmCannonArtworkCatalog Load(Stream json, Stream tilePng)
    {
        Ensure.NotNull(json);
        Ensure.NotNull(tilePng);
        SamusArmCannonArtworkCatalog.Placement placement = SamusArmCannonArtworkCatalog.LoadPlacement(json);
        RoomCharacterAtlas tiles = RoomCharacterAtlas.Load(tilePng,
            SamusArmCannonArtworkFormat.CoverTileCount *
                SamusRenderingRomData.ArmCannon.TileUploadByteCount);
        return SamusArmCannonArtworkCatalog.FromPlacement(placement, tiles);
    }
}
