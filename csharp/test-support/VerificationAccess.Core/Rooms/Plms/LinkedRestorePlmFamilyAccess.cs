namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Verification access to <see cref="LinkedRestorePlmFamily"/> members production does not use.</summary>
internal static class LinkedRestorePlmFamilyAccess
{
    /// <summary>Resolves a family's restore draw lists by their compiled pointer.</summary>
    extension(LinkedRestorePlmFamily family)
    {
        /// <summary>Converts a recognized restore drawing of this family into the shared verifier draw-list form.</summary>
        internal bool TryGet(PlmDraw request, out RoomPlmShotBlockDrawDefinitions.DrawList list)
        {
            if (family.TryDescribe(request, out LinkedRestorePlmFamily.Draw draw))
            {
                list = LinkedRestorePlmFamily.Export(draw);
                return true;
            }
            list = default;
            return false;
        }
    }
}
