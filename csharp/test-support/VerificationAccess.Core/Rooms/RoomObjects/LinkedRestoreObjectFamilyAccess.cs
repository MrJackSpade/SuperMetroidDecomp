using SuperMetroid.AssetExtraction.Rooms;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Verification access to <see cref="LinkedRestoreObjectFamily"/> members production does not use.</summary>
internal static class LinkedRestoreObjectFamilyAccess
{
    /// <summary>Resolves a family's restore draw lists by their compiled pointer.</summary>
    extension(LinkedRestoreObjectFamily family)
    {
        /// <summary>Converts a recognized restore drawing of this family into the shared verifier draw-list form.</summary>
        internal bool TryGet(RoomObjectDraw request, out RoomObjectShotBlockDrawDefinitions.DrawList list)
        {
            if (family.TryDescribe(request, out LinkedRestoreObjectFamily.Draw draw))
            {
                list = RoomObjectLinkedRestoreVisualFiles.Export(request, draw);
                return true;
            }
            list = default;
            return false;
        }
    }
}
