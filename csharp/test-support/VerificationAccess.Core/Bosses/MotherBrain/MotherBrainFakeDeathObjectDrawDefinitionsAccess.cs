namespace SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Verification access to <see cref="MotherBrainFakeDeathDraws"/> members production does not use.</summary>
internal static class MotherBrainFakeDeathObjectDrawDefinitionsAccess
{
    /// <summary>Resolves a Mother Brain fake-death draw list by native pointer.</summary>
    extension(MotherBrainFakeDeathDraws)
    {
        /// <summary>Searches background, regular, then boundary draw families for one native pointer.</summary>
        internal static bool TryGet(RoomObjectDraw request, out RoomObjectShotBlockDrawDefinitions.DrawList draw)
        {
            // Native order: background, regular, then boundary draw families.
            if (RoomObjectMotherBrainFakeDeathVisualFiles.TryGetBackground(request, out draw)
                || RoomObjectMotherBrainFakeDeathVisualFiles.TryGetRegular(request, out draw)
                || RoomObjectMotherBrainFakeDeathVisualFiles.TryGetBoundary(request, out draw))
                return true;
            draw = default;
            return false;
        }
    }
}
