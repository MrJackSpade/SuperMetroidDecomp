using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.Core.World.RoomStates;

/// <summary>
/// Verification access to the characters a loaded room's load shows (#1422, #1466): the room names its tileset, and the
/// import pipeline's catalog a test installed holds the common room elements' and the tileset's characters.
/// </summary>
internal static class LoadedRoomAssetsCharacterAccess
{
    /// <summary>The bytes of the characters the room's load shows, as a character catalog installs them.</summary>
    extension(LoadedRoomAssets self)
    {
        /// <summary>The common room elements' characters the room's load shows, as <paramref name="characters"/> installs them.</summary>
        internal ReadOnlyMemory<byte> CreCharactersIn(RoomCharacterAtlases characters) => Ensure.NotNull(characters).Cre.Characters;

        /// <summary>The tileset's characters the room's load shows, as <paramref name="characters"/> installs them.</summary>
        internal ReadOnlyMemory<byte> TilesetCharactersIn(RoomCharacterAtlases characters) =>
            Ensure.NotNull(characters).Get(self.Tileset.Characters).Characters;

        /// <summary>
        /// Loads the room into a test's VRAM fixture: gives the background sets the tileset's colours, as gameplay does,
        /// and records that the fixture's image shows the room's load under <paramref name="appearance"/> (#1466).
        /// </summary>
        internal void LoadGraphics(RoomAppearance appearance, BackgroundColourSets colours)
        {
            self.LoadGraphics(colours);
            BgArtworkFixtures.RecordLoad(appearance, self.Tileset);
        }
    }
}
