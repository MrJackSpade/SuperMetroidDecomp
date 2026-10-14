using SuperMetroid.Core.Rooms;

/// <summary>Cartridge values of <see cref="BotwoonWallPlmProgramDefinitions"/> that only verification reads.</summary>
internal static class BotwoonWallPlmProgramDefinitionsConstants
{
    /// <summary><c>$84:AB6D</c>: first byte of the following Kraid program.</summary>
    public const ushort ClearEndExclusive = 0xab6d;
    /// <summary><c>$84:AB51</c>: first byte of the following scroll callback.</summary>
    public const ushort CrumbleEndExclusive = (ushort)RoomPlmInstruction.SetBotwoonScrollsBlue;

    /// <summary>Provides the native clear and crumble program boundaries for Botwoon's wall PLM checks.</summary>
    extension(BotwoonWallPlmProgramDefinitions)
    {
        /// <inheritdoc cref="BotwoonWallPlmProgramDefinitionsConstants.ClearEndExclusive"/>
        internal static ushort ClearEndExclusive => BotwoonWallPlmProgramDefinitionsConstants.ClearEndExclusive;
        /// <inheritdoc cref="BotwoonWallPlmProgramDefinitionsConstants.CrumbleEndExclusive"/>
        internal static ushort CrumbleEndExclusive => BotwoonWallPlmProgramDefinitionsConstants.CrumbleEndExclusive;
    }
}
