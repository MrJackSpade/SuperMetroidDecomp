namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>Verification access to <see cref="RoomLevelStreamCatalog"/> members production does not use.</summary>
internal static class RoomLevelStreamCatalogAccess
{
    /// <summary>Exposes <see cref="RoomLevelStreamCatalog"/> state that only verification reads.</summary>
    extension(RoomLevelStreamCatalog self)
    {
        /// <summary>Number of distinct installed level sources.</summary>
        public int Count => self.streams.Count;
    }
}
