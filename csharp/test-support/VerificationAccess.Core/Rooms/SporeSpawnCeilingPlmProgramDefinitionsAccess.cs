using SuperMetroid.Core.Rooms;

/// <summary>Verification access to <see cref="SporeSpawnCeilingPlmProgramDefinitions"/> members production does not use.</summary>
internal static class SporeSpawnCeilingPlmProgramDefinitionsAccess
{
    /// <summary>Enumerates ceiling-crumble frame words and its final clear-state list.</summary>
    extension(SporeSpawnCeilingPlmProgramDefinitions)
    {
        /// <summary>Visits ceiling-crumble frame words followed by the clear-state list.</summary>
        internal static IEnumerable<ushort> NativeWordAddresses()
        {
            yield return SporeSpawnCeilingPlmProgramDefinitions.Crumble;
            for (int frame = 0; frame < 3; frame++)
            {
                yield return checked((ushort)(SporeSpawnCeilingPlmProgramDefinitions.Crumble + 3 + frame * 4));
                yield return checked((ushort)(SporeSpawnCeilingPlmProgramDefinitions.Crumble + 5 + frame * 4));
            }
            yield return SporeSpawnCeilingPlmProgramDefinitions.Clear;
            yield return checked((ushort)(SporeSpawnCeilingPlmProgramDefinitions.Clear + 2));
            yield return checked((ushort)(SporeSpawnCeilingPlmProgramDefinitions.Clear + 4));
        }
    }
}
