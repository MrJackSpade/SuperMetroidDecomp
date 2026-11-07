using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Cartridge values of <see cref="KraidRoomObjectDrawDefinitions"/> that only verification reads.</summary>
internal static class KraidRoomObjectDrawDefinitionsConstants
{
    /// <summary><c>$84:93EF</c>: start of the following Phantoon draw region.</summary>
    public const ushort EndExclusive = 0x93ef;

    /// <summary>Provides the exclusive draw-list boundary for Kraid room PLM verification.</summary>
    extension(KraidRoomObjectDrawDefinitions)
    {
        /// <inheritdoc cref="KraidRoomObjectDrawDefinitionsConstants.EndExclusive"/>
        internal static ushort EndExclusive => KraidRoomObjectDrawDefinitionsConstants.EndExclusive;
    }
}
