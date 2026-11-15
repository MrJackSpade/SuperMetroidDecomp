using SuperMetroid.AssetExtraction.Rooms;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Verification access to <see cref="LinkedRestoreObjectFamily"/> members production does not use.</summary>
internal static class LinkedRestoreObjectFamilyAccess
{
    /// <summary>Resolves a family's restore draw lists by their compiled pointer.</summary>
    extension(LinkedRestoreObjectFamily family)
    {
        /// <summary>Converts a recognized restore drawing of this family into the shared verifier draw-list form.</summary>
        internal bool TryGet(RoomObjectDraw request, out RoomObjectDrawLists.DrawList list)
        {
            foreach (KeyValuePair<RoomObjectRestoreShape, RoomObjectDraw> shape in family.Draws)
            {
                if (shape.Value != request)
                    continue;
                list = RoomObjectLinkedRestoreVisualFiles.Export(request, family.Describe(shape.Key));
                return true;
            }
            list = default;
            return false;
        }
    }
}
