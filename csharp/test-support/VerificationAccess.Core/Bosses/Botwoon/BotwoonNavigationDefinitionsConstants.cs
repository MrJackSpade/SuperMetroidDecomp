namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>Cartridge values and helpers of <see cref="BotwoonNavigationDefinitions"/> that only verification reads.</summary>
internal static class BotwoonNavigationDefinitionsConstants
{
    /// <summary>
    /// First movement-component pair at native address <c>$B3:A058</c>. Only the ROM oracle reads it, to map
    /// cartridge path pointers to sample indices: the simulation indexes samples, never addresses.
    /// </summary>
    internal const ushort MovementDataStart = 0xa058;
    /// <summary>Exclusive end of the movement corpus at native address <c>$B3:E150</c>; read only by the ROM oracle.</summary>
    internal const ushort MovementDataEndExclusive = 0xe150;
}
