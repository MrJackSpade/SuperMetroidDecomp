namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>Verification access to <see cref="BotwoonWallPlmProgramDefinitions"/> members production does not use.</summary>
internal static class BotwoonWallPlmProgramDefinitionsAccess
{
    /// <summary>Enumerates native Botwoon wall program words for exact ROM ownership checks.</summary>
    extension(BotwoonWallPlmProgramDefinitions)
    {
        /// <summary>Visits crumble timing, draw, and terminal words, then the already-cleared wall list.</summary>
        internal static IEnumerable<ushort> NativeWordAddresses()
        {
            yield return BotwoonWallPlmProgramDefinitions.Crumble;
            yield return checked((ushort)(BotwoonWallPlmProgramDefinitions.Crumble + 3));
            yield return checked((ushort)(BotwoonWallPlmProgramDefinitions.Crumble + 5));
            for (int frame = 0; frame < 4; frame++)
            {
                yield return checked((ushort)(BotwoonWallPlmProgramDefinitions.Crumble + 8 + frame * 4));
                yield return checked((ushort)(BotwoonWallPlmProgramDefinitions.Crumble + 10 + frame * 4));
            }
            yield return checked((ushort)(BotwoonWallPlmProgramDefinitions.Crumble + 24));
            yield return checked((ushort)(BotwoonWallPlmProgramDefinitions.Crumble + 26));
            yield return checked((ushort)(BotwoonWallPlmProgramDefinitions.Crumble + 28));
            yield return checked((ushort)(BotwoonWallPlmProgramDefinitions.Crumble + 30));
            yield return BotwoonWallPlmProgramDefinitions.Clear;
            yield return checked((ushort)(BotwoonWallPlmProgramDefinitions.Clear + 2));
            yield return checked((ushort)(BotwoonWallPlmProgramDefinitions.Clear + 4));
        }
    }
}
