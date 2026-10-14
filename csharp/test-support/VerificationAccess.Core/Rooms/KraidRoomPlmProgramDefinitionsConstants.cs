using SuperMetroid.Core.Rooms;

/// <summary>Cartridge values of <see cref="KraidRoomPlmProgramDefinitions"/> that only verification reads.</summary>
internal static class KraidRoomPlmProgramDefinitionsConstants
{
    /// <summary><c>$84:ABE3</c>: first byte of following Mother Brain PLM program.</summary>
    public const ushort EndExclusive = 0xabe3;

    /// <summary>Provides the exclusive program boundary for Kraid room PLM verification.</summary>
    extension(KraidRoomPlmProgramDefinitions)
    {
        /// <inheritdoc cref="KraidRoomPlmProgramDefinitionsConstants.EndExclusive"/>
        internal static ushort EndExclusive => KraidRoomPlmProgramDefinitionsConstants.EndExclusive;
    }
}
