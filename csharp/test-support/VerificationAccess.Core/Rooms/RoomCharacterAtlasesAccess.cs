using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Verification access to <see cref="RoomCharacterAtlases"/> members production does not use.</summary>
internal static class RoomCharacterAtlasesAccess
{
    /// <summary>Exposes <see cref="RoomCharacterAtlases"/> state that only verification reads.</summary>
    extension(RoomCharacterAtlases self)
    {
        /// <summary>Every installed sheet besides the common room elements.</summary>
        public IReadOnlyDictionary<RoomCharacterSet, RoomCharacterAtlas> Sheets => self.byRoomCharacterSet;
    }
}
