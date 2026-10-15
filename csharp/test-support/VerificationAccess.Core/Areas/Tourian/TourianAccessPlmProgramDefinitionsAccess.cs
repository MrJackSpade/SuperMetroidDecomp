namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>Verification access to <see cref="TourianAccessPlmProgramDefinitions"/> members production does not use.</summary>
internal static class TourianAccessPlmProgramDefinitionsAccess
{
    /// <summary>Enumerates Tourian access crumble frames and terminal clear commands.</summary>
    extension(TourianAccessPlmProgramDefinitions)
    {
        /// <summary>Visits the Tourian access crumble frames and terminal clear commands.</summary>
        internal static IEnumerable<ushort> NativeWordAddresses()
        {
            yield return TourianAccessPlmProgramDefinitions.Crumble;
            for (int frame = 0; frame < 4; frame++)
            {
                yield return checked((ushort)(TourianAccessPlmProgramDefinitions.Crumble + 3 + frame * 4));
                yield return checked((ushort)(TourianAccessPlmProgramDefinitions.Crumble + 5 + frame * 4));
            }
            yield return checked((ushort)(TourianAccessPlmProgramDefinitions.Crumble + 19));
            yield return checked((ushort)(TourianAccessPlmProgramDefinitions.Crumble + 21));
            yield return checked((ushort)(TourianAccessPlmProgramDefinitions.Crumble + 23));
            yield return checked((ushort)(TourianAccessPlmProgramDefinitions.Crumble + 25));
            yield return TourianAccessPlmProgramDefinitions.Clear;
            yield return checked((ushort)(TourianAccessPlmProgramDefinitions.Clear + 2));
            yield return checked((ushort)(TourianAccessPlmProgramDefinitions.Clear + 4));
        }
    }
}
