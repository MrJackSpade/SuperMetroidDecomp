namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Verification access to <see cref="RoomPlmContactCrumbleRestoreDrawDefinitions"/> members production does not use.</summary>
internal static class RoomPlmContactCrumbleRestoreDrawDefinitionsAccess
{
    /// <summary>Resolves contact-crumble restoration draw lists by native pointer.</summary>
    extension(RoomPlmContactCrumbleRestoreDrawDefinitions)
    {
        /// <summary>Converts a recognized contact-crumble restore drawing to a verifier draw list.</summary>
        internal static bool TryGet(ushort pointer, out RoomPlmShotBlockDrawDefinitions.DrawList list)
        {
            if (RoomPlmContactCrumbleRestoreDrawDefinitions.TryDescribe(pointer, out RoomPlmContactCrumbleRestoreDrawDefinitions.Draw draw))
            {
                list = RoomPlmContactCrumbleRestoreDrawDefinitions.Export(draw);
                return true;
            }
            list = default;
            return false;
        }
    }
}
