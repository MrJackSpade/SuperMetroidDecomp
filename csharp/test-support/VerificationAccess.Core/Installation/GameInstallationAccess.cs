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
        /// Loads validated room character atlases, selecting compatible user tile artwork for each supplied replacement: the
        /// import pipeline's input to the rooms' pictures, which no host binds (#1466).
        /// </summary>
        public RoomCharacterAtlases LoadRoomCharacters() =>
            RoomCharacterArtworkFiles.Load(self.RoomCharacterDirectory, self.RoomCharacterOverrideDirectory);

        /// <summary>
        /// Loads validated room background tilemaps and compatible replacements from the persistent override tree: the
        /// import pipeline's input to the room backgrounds' images, which no host binds (#1466).
        /// </summary>
        public RoomBackgroundTilemaps LoadRoomBackgroundTilemaps() =>
            RoomBackgroundTilemapArtworkFiles.Load(self.RoomBackgroundTilemapDirectory, self.RoomBackgroundTilemapOverrideDirectory);

        /// <summary>
        /// Loads validated sky tilemaps from the shared room-background directory and its compatible user replacements: the
        /// import pipeline's input to the sky's images, which no host binds (#1466).
        /// </summary>
        public RoomSkyTilemaps LoadRoomSkyTilemaps() =>
            RoomSkyTilemapArtworkFiles.Load(self.RoomBackgroundTilemapDirectory, self.RoomBackgroundTilemapOverrideDirectory);
        /// <summary>Stock room character-tile atlases and their source manifests.</summary>
        public string RoomCharacterDirectory => Path.Combine(self.ContentDirectory, GameInstallationLayout.RoomCharacterDirectoryName);
        /// <summary>Stock background tilemaps, including sky tilemaps, under the replaceable content tree.</summary>
        public string RoomBackgroundTilemapDirectory => Path.Combine(self.ContentDirectory, GameInstallationLayout.RoomBackgroundTilemapDirectoryName);
    }
}
