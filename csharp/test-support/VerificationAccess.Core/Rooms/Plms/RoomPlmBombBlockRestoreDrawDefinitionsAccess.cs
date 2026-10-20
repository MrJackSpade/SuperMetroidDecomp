namespace SuperMetroid.Core.Rooms.Plms;

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
                list = RoomPlmBombBlockRestoreDrawDefinitions.Export(draw);
                return true;
            }
            list = default;
            return false;
        }
    }
}
