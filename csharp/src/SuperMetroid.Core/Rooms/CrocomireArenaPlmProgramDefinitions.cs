namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Crocomire's five finite room-object instruction lists at $84:AFCA..AFE7.
/// Each list draws its physical layout once, then deletes its PLM.
/// </summary>
internal static class CrocomireArenaPlmProgramDefinitions
{
    /// <summary><c>$84:AFCA</c>: clear the ten-block bridge.</summary>
    internal const ushort ClearBridge = RoomPlmInstructionLists.ClearCrocomireBridge;
    /// <summary><c>$84:AFD0</c>: crumble one bridge block.</summary>
    internal const ushort CrumbleBridgeBlock = RoomPlmInstructionLists.CrumbleCrocomireBridgeBlock;
    /// <summary><c>$84:AFD6</c>: clear one bridge block.</summary>
    internal const ushort ClearBridgeBlock = RoomPlmInstructionLists.ClearCrocomireBridgeBlock;
    /// <summary><c>$84:AFDC</c>: clear the invisible wall.</summary>
    internal const ushort ClearInvisibleWall = RoomPlmInstructionLists.ClearCrocomireInvisibleWall;
    /// <summary><c>$84:AFE2</c>: create the invisible wall.</summary>
    internal const ushort CreateInvisibleWall = RoomPlmInstructionLists.CreateCrocomireInvisibleWall;
    /// <summary><c>$84:AFE8</c>: first byte of the following save-station program.</summary>
    internal const ushort EndExclusive = 0xafe8;

    /// <summary>
    /// Each six-byte list owns aligned duration, draw and delete words only.
    /// The five named draw operations are semantic selections, not stored operands.
    /// Odd words and the following save-station program remain unowned.
    /// </summary>
    internal static bool TryReadMechanicsWord(ushort address, out ushort value)
    {
        value = 0;
        int relative = address - ClearBridge;
        if (relative < 0 || address >= EndExclusive || (relative & 1) != 0)
            return false;
        value = (relative % 6) switch
        {
            0 => 1,
            4 => RoomPlmInstructionCodes.Delete,
            _ => (address - 2) switch
            {
                ClearBridge => CrocomireArenaPlmDrawDefinitions.ClearBridge,
                CrumbleBridgeBlock => CrocomireArenaPlmDrawDefinitions.CrumbleBridgeBlock,
                ClearBridgeBlock => CrocomireArenaPlmDrawDefinitions.ClearBridgeBlock,
                ClearInvisibleWall => CrocomireArenaPlmDrawDefinitions.ClearInvisibleWall,
                CreateInvisibleWall => CrocomireArenaPlmDrawDefinitions.CreateInvisibleWall,
                _ => throw new InvalidOperationException("Invalid bounded Crocomire program."),
            },
        };
        return true;
    }
}
