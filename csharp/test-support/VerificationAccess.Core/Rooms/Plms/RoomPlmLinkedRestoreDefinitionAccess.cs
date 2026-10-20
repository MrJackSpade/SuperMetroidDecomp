namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Verification access to <see cref="RoomPlmLinkedRestoreDefinition"/> members production does not use.</summary>
internal static class RoomPlmLinkedRestoreDefinitionAccess
{
    /// <summary>Resolves a family's restore draw lists by their compiled pointer.</summary>
    extension(RoomPlmLinkedRestoreDefinition family)
    {
        /// <summary>Converts a recognized restore drawing of this family into the shared verifier draw-list form.</summary>
        internal bool TryGet(ushort pointer, out RoomPlmShotBlockDrawDefinitions.DrawList list)
        {
            if (family.TryDescribe(pointer, out RoomPlmLinkedRestoreDefinition.Draw draw))
            {
                list = RoomPlmLinkedRestoreDefinition.Export(draw);
                return true;
            }
            list = default;
            return false;
        }
    }
}
