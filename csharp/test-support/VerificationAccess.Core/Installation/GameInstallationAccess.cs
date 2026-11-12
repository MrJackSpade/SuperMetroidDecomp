using SuperMetroid.AssetExtraction.Rooms;

namespace SuperMetroid.AssetExtraction.Installation;

/// <summary>Verification access to <see cref="GameInstallation"/> members production does not use.</summary>
internal static class GameInstallationAccess
{
    /// <summary>Exposes <see cref="GameInstallation"/> state that only verification reads.</summary>
    extension(GameInstallation self)
    {
        /// <summary>
        /// Stock room block compositions under the replaceable content tree, which extraction composes the block sheets from
        /// (#1431); a room's blocks are changed only through their pictures in the block sheet.
        /// </summary>
        public string RoomMetatileDirectory => Path.Combine(self.ContentDirectory, GameInstallationLayout.RoomMetatileDirectoryName);

        /// <summary>
        /// Loads the installed room background tilemaps: the import pipeline's input to the room backgrounds' images, which
        /// no host binds (#1466).
        /// </summary>
        public RoomBackgroundTilemaps LoadRoomBackgroundTilemaps() =>
            RoomBackgroundTilemapArtworkFiles.Load(self.RoomBackgroundTilemapDirectory);

        /// <summary>
        /// Loads the installed sky tilemaps from the shared room-background directory: the import pipeline's input to the
        /// sky's images, which no host binds (#1466).
        /// </summary>
        public RoomSkyTilemaps LoadRoomSkyTilemaps() =>
            RoomSkyTilemapArtworkFiles.Load(self.RoomBackgroundTilemapDirectory);
        /// <summary>The room characters' provenance manifest; the characters are import data no installation holds (#1636).</summary>
        public string RoomCharacterDirectory => Path.Combine(self.ContentDirectory, GameInstallationLayout.RoomCharacterDirectoryName);
        /// <summary>Stock background tilemaps, including sky tilemaps, under the replaceable content tree.</summary>
        public string RoomBackgroundTilemapDirectory => Path.Combine(self.ContentDirectory, GameInstallationLayout.RoomBackgroundTilemapDirectoryName);
        /// <summary>
        /// The installed picture store (#1635): every picture the sheets show, each written once, on pages the player edits in
        /// place, an edit changing the picture for every sheet that shows it.
        /// </summary>
        public string PictureStoreDirectory => Path.Combine(self.ContentDirectory, GameInstallationLayoutConstants.PictureStoreDirectoryName);
    }
}
