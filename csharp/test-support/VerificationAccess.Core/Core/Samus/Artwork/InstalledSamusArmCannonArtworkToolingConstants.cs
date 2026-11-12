using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.AssetExtraction.Rooms.Artwork;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Cartridge values and helpers of <see cref="InstalledSamusArmCannonArtworkTooling"/> that only verification reads.</summary>
internal static class InstalledSamusArmCannonArtworkToolingConstants
{
    /// <summary>Combines validated placement JSON with the exact number of indexed cannon cover characters (#1636).</summary>
    public static SamusArmCannonCoverCharacters LoadWithCover(Stream json, Stream tilePng)
    {
        Ensure.NotNull(tilePng);
        RoomCharacterAtlas tiles = RoomCharacterAtlasPng.Read(tilePng,
            SamusArmCannonArtworkFormat.CoverTileCount *
                SamusArmCannonArtworkFormat.CoverTileByteCount);
        return new SamusArmCannonCoverCharacters(InstalledSamusArmCannonArtworkTooling.Load(json), tiles);
    }
}
