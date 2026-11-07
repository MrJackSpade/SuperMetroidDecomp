using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>Cartridge values of <see cref="BotwoonWallObjectDrawDefinitions"/> that only verification reads.</summary>
internal static class BotwoonWallObjectDrawDefinitionsConstants
{
    /// <summary><c>$84:9325</c>: first byte of the following unused draw list.</summary>
    public const ushort EndExclusive = 0x9325;

    /// <summary>Provides the exclusive native draw-list boundary used to verify Botwoon wall PLM extraction.</summary>
    extension(BotwoonWallObjectDrawDefinitions)
    {
        /// <inheritdoc cref="BotwoonWallObjectDrawDefinitionsConstants.EndExclusive"/>
        internal static ushort EndExclusive => BotwoonWallObjectDrawDefinitionsConstants.EndExclusive;
    }
}
