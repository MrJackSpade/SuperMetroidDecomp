using SuperMetroid.Core.Rooms;

/// <summary>Verification access to <see cref="MotherBrainFakeDeathPlmDrawDefinitions"/> members production does not use.</summary>
internal static class MotherBrainFakeDeathPlmDrawDefinitionsAccess
{
    /// <summary>Resolves a Mother Brain fake-death draw list by native pointer.</summary>
    extension(MotherBrainFakeDeathPlmDrawDefinitions)
    {
        /// <summary>Searches background, regular, then boundary draw families for one native pointer.</summary>
        internal static bool TryGet(ushort pointer, out RoomPlmShotBlockDrawDefinitions.DrawList draw)
        {
            // Native order: background, regular, then boundary draw families.
            if (MotherBrainFakeDeathPlmDrawDefinitions.TryGetBackground(pointer, out draw)
                || MotherBrainFakeDeathPlmDrawDefinitions.TryGetRegular(pointer, out draw)
                || MotherBrainFakeDeathPlmDrawDefinitions.TryGetBoundary(pointer, out draw))
                return true;
            draw = default;
            return false;
        }
    }
}
