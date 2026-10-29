using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Development-tool loading of <see cref="SamusArmCannonSourceArtwork"/>; never linked by player hosts.</summary>
internal static class InstalledSamusArmCannonArtworkTooling
{
    /// <summary>Combines validated placement JSON with the exact number of indexed cannon tile uploads.</summary>
    public static SamusArmCannonSourceArtwork Load(Stream json, Stream tilePng)
    {
        Ensure.NotNull(json);
        Ensure.NotNull(tilePng);
        SamusArmCannonSourceArtwork.DescriptorTables placement = SamusArmCannonSourceArtwork.LoadPlacement(json);
        RoomCharacterAtlas tiles = RoomCharacterAtlasPng.Read(tilePng,
            SamusArmCannonArtworkFormat.CoverTileCount *
                SamusArmCannonArtworkFormat.CoverTileByteCount);
        return SamusArmCannonSourceArtwork.FromPlacement(placement, tiles);
    }
}
