namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Cartridge definitions for PLM $84:B964, the old Tourian escape shaft fake wall that room
/// setup $8F:91A9 spawns in the Climb during the escape.
/// </summary>
internal static class OldTourianEscapeShaftWallPlmDefinitions
{
    /// <summary>$8F:91AD: the hardcoded spawn's block X.</summary>
    internal const int BlockX = 0x10;
    /// <summary>$8F:91AE: the hardcoded spawn's block Y.</summary>
    internal const int BlockY = 0x87;

    /// <summary>$84:B919, <c>InstList_PLM_MakeOldTourianEscapeShaftFakeWallExplode</c>.</summary>
    internal const ushort InstructionList = 0xb919;

    /// <summary>$84:B927, the pre-instruction that waits for Samus and spawns the explosion.</summary>
    internal const ushort WaitForSamusPreInstruction = 0xb927;

    /// <summary>$84:9283, <c>DrawInst_OldTourianEscapeShaftBlocks</c>: two blank columns.</summary>
    internal const ushort OpenWallDraw = 0x9283;

    /// <summary>$84:B927-$B92F: Samus must be strictly right of X $F0 and below Y $820.</summary>
    internal const ushort WakeTargetX = 0x00f0;
    internal const ushort WakeTargetY = 0x0820;

    internal static bool TryReadMechanicsWord(ushort address, out ushort value)
    {
        value = address switch
        {
            0xb919 => (ushort)RoomPlmInstruction.InstallPreInstruction,
            0xb91b => WaitForSamusPreInstruction,
            0xb91d => (ushort)RoomPlmInstruction.Sleep,
            0xb91f => (ushort)RoomPlmInstruction.ClearPreInstruction,
            0xb921 => 0x0001,
            0xb923 => OpenWallDraw,
            0xb925 => (ushort)RoomPlmInstruction.Delete,
            _ => 0,
        };
        return value != 0;
    }

    /// <summary>
    /// $84:9283: a vertical run of three $00FF air words at the PLM, then another one block to
    /// its right.
    /// </summary>
    internal static bool TryGetDraw(ushort pointer, out RoomPlmShotBlockDrawDefinitions.DrawList draw)
    {
        if (pointer != OpenWallDraw)
        {
            draw = default;
            return false;
        }
        ushort[] column = [0x00ff, 0x00ff, 0x00ff];
        draw = new(pointer, new RoomPlmShotBlockDrawDefinitions.Run[]
        {
            new(0x8003, column, 1, 0),
            new(0x8003, column, 0, 0),
        });
        return true;
    }
}
