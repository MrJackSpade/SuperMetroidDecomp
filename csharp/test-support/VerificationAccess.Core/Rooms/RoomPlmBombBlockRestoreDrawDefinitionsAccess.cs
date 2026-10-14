using SuperMetroid.Core.Rooms;

/// <summary>Verification access to <see cref="RoomPlmBombBlockRestoreDrawDefinitions"/> members production does not use.</summary>
internal static class RoomPlmBombBlockRestoreDrawDefinitionsAccess
{
    /// <summary>Resolves bomb-block restore draw lists by their compiled pointer.</summary>
    extension(RoomPlmBombBlockRestoreDrawDefinitions)
    {
        /// <summary>Converts a recognized bomb-block restore drawing into the shared verifier draw-list form.</summary>
        internal static bool TryGet(ushort pointer, out RoomPlmShotBlockDrawDefinitions.DrawList list)
        {
            if (RoomPlmBombBlockRestoreDrawDefinitions.TryDescribe(pointer, out RoomPlmBombBlockRestoreDrawDefinitions.Draw draw))
            {
                list = ((RoomPlmShotBlockDrawDefinitions.DrawList)(RoomPlmBombBlockRestoreDrawDefinitions.Export((RoomPlmBombBlockRestoreDrawDefinitions.Draw)(draw))));
                return true;
            }
            list = default;
            return false;
        }
    }
}
