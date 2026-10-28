using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.Core.World.RoomStates;

/// <summary>
/// Verification access to the characters a loaded room binds (#1422): the room names its common room elements and its
/// tileset and counts their characters, and the import pipeline's resolver reads them from the catalog a test installed.
/// </summary>
internal static class LoadedRoomAssetsCharacterAccess
{
    /// <summary>The bytes of the characters the room binds, as a character catalog installs them.</summary>
    extension(LoadedRoomAssets self)
    {
        /// <summary>The common room elements' characters the room binds, as <paramref name="characters"/> installs them.</summary>
        internal ReadOnlyMemory<byte> CreCharactersIn(RoomCharacterAtlases characters) =>
            Bound(characters, RoomBgArtwork.CommonRoomElements, self.CreCharacterCount);

        /// <summary>The tileset's characters the room binds, as <paramref name="characters"/> installs them.</summary>
        internal ReadOnlyMemory<byte> TilesetCharactersIn(RoomCharacterAtlases characters) =>
            Bound(characters, RoomBgArtwork.Tileset(self.Tileset.Characters), self.TilesetCharacterCount);
    }

    /// <summary>The first <paramref name="characterCount"/> tile characters <paramref name="artwork"/> names in the catalog.</summary>
    private static ReadOnlyMemory<byte> Bound(RoomCharacterAtlases characters, BgArtworkIdentity artwork, int characterCount) =>
        new BgArtworkBytes().WithRoomCharacters(characters).Of(new BgArtworkBinding(artwork, BgCharacterSet.Tiles, 0, characterCount));
}
