namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>Verification access to <see cref="InstalledRoomLevelStreams"/> members production does not use.</summary>
internal static class InstalledRoomLevelStreamsAccess
{
    /// <summary>Exposes <see cref="InstalledRoomLevelStreams"/> state that only verification reads.</summary>
    extension(InstalledRoomLevelStreams self)
    {
        /// <summary>Number of distinct installed levels.</summary>
        public int Count => self.levels.Count;
    }
}
