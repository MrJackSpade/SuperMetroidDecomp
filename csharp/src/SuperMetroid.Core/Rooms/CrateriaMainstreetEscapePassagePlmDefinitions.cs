namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Cartridge definitions for PLM $84:BB30, which clears the Crateria mainstreet escape
/// passage when the critters escaped. Room setup $8F:9194 spawns it during the escape.
/// </summary>
internal static class CrateriaMainstreetEscapePassagePlmDefinitions
{
    /// <summary>$8F:9198: the hardcoded spawn's block X.</summary>
    internal const int BlockX = 0x3d;
    /// <summary>$8F:9199: the hardcoded spawn's block Y.</summary>
    internal const int BlockY = 0x0b;

    /// <summary>$84:BB19, <c>InstList_PLM_ClearCrateriaMainstreetEscPassageIfCrittersEsc</c>.</summary>
    internal const ushort InstructionList = 0xbb19;

    /// <summary>$84:9253, <c>DrawInst_CrateriaMainStreetEscape</c>: two $00FF air blocks in a row.</summary>
    internal const ushort ClearPairDraw = 0x9253;

    /// <summary>
    /// $84:BB25, <c>Instruction_PLM_MovePLMRight4Blocks</c>: adds eight to the PLM's byte
    /// block index, four level words.
    /// </summary>
    internal const ushort MoveRightFourBlocks = 0xbb25;

    internal static bool TryReadMechanicsWord(ushort address, out ushort value)
    {
        value = address switch
        {
            0xbb19 => 0x0001,
            0xbb1b => ClearPairDraw,
            0xbb1d => MoveRightFourBlocks,
            0xbb1f => 0x0001,
            0xbb21 => ClearPairDraw,
            0xbb23 => RoomPlmInstructionCodes.Delete,
            _ => 0,
        };
        return value != 0;
    }

    internal static bool TryGetDraw(ushort pointer, out RoomPlmShotBlockDrawDefinitions.DrawList draw)
    {
        if (pointer != ClearPairDraw)
        {
            draw = default;
            return false;
        }
        draw = new(pointer, new RoomPlmShotBlockDrawDefinitions.Run[]
        {
            new(0x0002, new ushort[] { 0x00ff, 0x00ff }, 0, 0),
        });
        return true;
    }
}
