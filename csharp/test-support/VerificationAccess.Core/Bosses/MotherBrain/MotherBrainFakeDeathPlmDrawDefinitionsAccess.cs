namespace SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Rooms.Plms;

/// <summary>Verification access to <see cref="MotherBrainFakeDeathPlmDrawDefinitions"/> members production does not use.</summary>
internal static class MotherBrainFakeDeathPlmDrawDefinitionsAccess
{
    /// <summary>Resolves a Mother Brain fake-death draw list by native pointer.</summary>
    extension(MotherBrainFakeDeathPlmDrawDefinitions)
    {
        /// <summary>Searches background, regular, then boundary draw families for one native pointer.</summary>
        internal static bool TryGet(PlmDraw request, out RoomPlmShotBlockDrawDefinitions.DrawList draw)
        {
            // Native order: background, regular, then boundary draw families.
            if (MotherBrainFakeDeathPlmDrawDefinitions.TryGetBackground(request, out draw)
                || MotherBrainFakeDeathPlmDrawDefinitions.TryGetRegular(request, out draw)
                || MotherBrainFakeDeathPlmDrawDefinitions.TryGetBoundary(request, out draw))
                return true;
            draw = default;
            return false;
        }
    }
}
