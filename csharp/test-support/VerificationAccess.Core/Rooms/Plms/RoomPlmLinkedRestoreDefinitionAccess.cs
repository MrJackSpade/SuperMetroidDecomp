namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Verification access to <see cref="RoomPlmLinkedRestoreDefinition"/> members production does not use.</summary>
internal static class RoomPlmLinkedRestoreDefinitionAccess
{
    /// <summary>Resolves a family's restore draw lists by their compiled pointer.</summary>
    extension(RoomPlmLinkedRestoreDefinition family)
    {
        /// <summary>Converts a recognized restore drawing of this family into the shared verifier draw-list form.</summary>
        internal bool TryGet(PlmDraw request, out RoomPlmShotBlockDrawDefinitions.DrawList list)
        {
            if (family.TryDescribe(request, out RoomPlmLinkedRestoreDefinition.Draw draw))
            {
                list = RoomPlmLinkedRestoreDefinition.Export(draw);
                return true;
            }
            list = default;
            return false;
        }
    }
}
