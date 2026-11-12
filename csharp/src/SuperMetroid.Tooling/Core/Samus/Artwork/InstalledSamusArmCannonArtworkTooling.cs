using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.AssetExtraction.Rooms.Artwork;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Development-tool loading of <see cref="SamusArmCannonSourceArtwork"/>; never linked by player hosts.</summary>
internal static class InstalledSamusArmCannonArtworkTooling
{
    /// <summary>The cover placement of validated placement JSON.</summary>
    public static SamusArmCannonSourceArtwork Load(Stream json)
    {
        Ensure.NotNull(json);
        return SamusArmCannonSourceArtwork.FromPlacement(SamusArmCannonSourceArtwork.LoadPlacement(json));
    }

    /// <summary>Combines validated placement JSON with the exact number of indexed cannon cover characters (#1636).</summary>
    public static SamusArmCannonCoverCharacters LoadWithCover(Stream json, Stream tilePng)
    {
        Ensure.NotNull(tilePng);
        RoomCharacterAtlas tiles = RoomCharacterAtlasPng.Read(tilePng,
            SamusArmCannonArtworkFormat.CoverTileCount *
                SamusArmCannonArtworkFormat.CoverTileByteCount);
        return new SamusArmCannonCoverCharacters(Load(json), tiles);
    }
}
