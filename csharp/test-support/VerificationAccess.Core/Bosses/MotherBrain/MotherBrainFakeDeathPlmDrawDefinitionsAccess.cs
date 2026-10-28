namespace SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Rooms.Plms;

/// <summary>Verification access to <see cref="MotherBrainFakeDeathDraws"/> members production does not use.</summary>
internal static class MotherBrainFakeDeathPlmDrawDefinitionsAccess
{
    /// <summary>Resolves a Mother Brain fake-death draw list by native pointer.</summary>
    extension(MotherBrainFakeDeathDraws)
    {
        /// <summary>Searches background, regular, then boundary draw families for one native pointer.</summary>
        internal static bool TryGet(PlmDraw request, out RoomPlmShotBlockDrawDefinitions.DrawList draw)
        {
            // Native order: background, regular, then boundary draw families.
            if (MotherBrainFakeDeathDraws.TryGetBackground(request, out draw)
                || MotherBrainFakeDeathDraws.TryGetRegular(request, out draw)
                || MotherBrainFakeDeathDraws.TryGetBoundary(request, out draw))
                return true;
            draw = default;
            return false;
        }
    }
}
