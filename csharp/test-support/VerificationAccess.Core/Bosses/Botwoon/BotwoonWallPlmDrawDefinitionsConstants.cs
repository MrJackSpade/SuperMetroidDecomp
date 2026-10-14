namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>Cartridge values of <see cref="BotwoonWallPlmDrawDefinitions"/> that only verification reads.</summary>
internal static class BotwoonWallPlmDrawDefinitionsConstants
{
    /// <summary><c>$84:9325</c>: first byte of the following unused draw list.</summary>
    public const ushort EndExclusive = 0x9325;

    /// <summary>Provides the exclusive native draw-list boundary used to verify Botwoon wall PLM extraction.</summary>
    extension(BotwoonWallPlmDrawDefinitions)
    {
        /// <inheritdoc cref="BotwoonWallPlmDrawDefinitionsConstants.EndExclusive"/>
        internal static ushort EndExclusive => BotwoonWallPlmDrawDefinitionsConstants.EndExclusive;
    }
}
