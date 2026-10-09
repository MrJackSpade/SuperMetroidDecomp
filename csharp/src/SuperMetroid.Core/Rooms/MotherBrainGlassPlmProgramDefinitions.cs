using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Rooms;

/// <summary>
/// NTSC bank-$84 glass control program. Nine stages wait for successive even hit
/// counts; five transitions emit two four-shard bursts around a four-tick draw.
/// Named draw and branch cases express glass damage states, not stored ROM bytes.
/// All 247 bytes and 246 overlapping word starts at D202..D2F8 remain supported.
/// </summary>
internal static class MotherBrainGlassPlmProgramDefinitions
{
    /// <summary>$84:D202: main glass instruction list.</summary>
    internal const ushort FirstAddress = 0xd202;
    /// <summary>$84:D2F8: last byte of the no-glass branch, before callback code.</summary>
    internal const ushort LastAddress = 0xd2f8;
    /// <summary>$84:D1E6: increment the glass PLM room argument on a missile hit.</summary>
    internal const ushort HitPreInstruction = 0xd1e6;
    /// <summary>$84:D2ED: boss-dead branch selects cleared glass.</summary>
    private const ushort BossDead = 0xd2ed;
    /// <summary>$84:D2F3: persisted glass-destroyed branch selects final shatter.</summary>
    private const ushort NoGlass = 0xd2f3;
    /// <summary>$84:D211: first one-tick glass damage stage.</summary>
    private const ushort Stages = 0xd211;
    /// <summary>$86:CE61 shard placement: argument zero selects X offset +8.</summary>
    private const ushort RightShard = 0;
    /// <summary>$86:CE63 shard placement: argument two selects X offset -40.</summary>
    private const ushort LeftShard = 2;
    /// <summary>$86:CE65 shard placement: argument four selects X offset -16.</summary>
    private const ushort CentreShard = 4;

    /// <summary>Reads a little-endian mechanics word whose low byte starts within the compiled glass program's word-address range.</summary>
    /// <param name="address">Bank-local address of the word's low byte.</param>
    /// <param name="value">Receives the compiled word when the address is supported, or zero otherwise.</param>
    /// <returns><see langword="true"/> when a complete two-byte read is available from this program.</returns>
    internal static bool TryReadMechanicsWord(ushort address, out ushort value)
    {
        value = 0;
        if (address < FirstAddress || address >= LastAddress) return false;
        value = (ushort)(ByteAt(address) | ByteAt(address + 1) << 8);
        return true;
    }

    /// <summary>Reads a byte from any address occupied by the compiled glass program, including its final byte.</summary>
    /// <param name="address">Bank-local byte address to read.</param>
    /// <param name="value">Receives the compiled byte when supported, or zero otherwise.</param>
    /// <returns><see langword="true"/> when the address lies within the program's inclusive byte range.</returns>
    internal static bool TryReadMechanicsByte(ushort address, out byte value)
    {
        value = 0;
        if (address < FirstAddress || address > LastAddress) return false;
        value = ByteAt(address);
        return true;
    }

    /// <summary>Extracts one byte from the overlapping little-endian words that encode the glass control program.</summary>
    /// <param name="address">Address of the byte within the encoded program.</param>
    /// <returns>The byte selected from its containing compiled word.</returns>
    private static byte ByteAt(int address)
    {
        if (address == FirstAddress + 2) return 1; // Area's primary boss bit.
        int start = address < FirstAddress + 2 ? FirstAddress + ((address - FirstAddress) & ~1) :
            FirstAddress + 3 + ((address - FirstAddress - 3) & ~1);
        return (byte)(WordAt(start) >> ((address - start) * 8));
    }

    /// <summary>Reconstructs the mechanics operand at a word start across the glass stages, shard bursts, and terminal branches.</summary>
    /// <param name="address">Bank-local address of the instruction word to reproduce.</param>
    /// <returns>The callback, branch operand, timing, draw pointer, or shard argument encoded at that position.</returns>
    private static ushort WordAt(int address)
    {
        if (address < Stages)
            return address switch
            {
                FirstAddress => RoomPlmInstructionCodes.GotoIfAreaBossBitSet,
                0xd205 => BossDead,
                0xd207 => RoomPlmInstructionCodes.GotoIfEventSet,
                0xd209 => (ushort)EventNumber.MotherBrainGlassDestroyed,
                0xd20b => NoGlass,
                0xd20d => RoomPlmInstructionCodes.InstallPreInstruction,
                _ => HitPreInstruction,
            };
        if (address >= BossDead)
            return ((address - BossDead) % 6) switch
            {
                0 => 1,
                2 => address < NoGlass ? MotherBrainGlassPlmDrawDefinitions.Cleared : MotherBrainGlassPlmDrawDefinitions.Shatter3,
                _ => RoomPlmInstructionCodes.Delete,
            };
        int start = Stages;
        for (int stage = 0; stage < 9; stage++)
        {
            bool burst = stage == 2 || stage >= 5;
            int length = stage == 8 ? 44 : burst ? 34 : 10;
            if (address >= start + length) { start += length; continue; }
            int offset = address - start;
            if (offset is >= 12 and <= 18 or >= 26 and <= 32)
            {
                // The first group starts at 12; the second starts at 26.
                int shard = (offset - (offset < 20 ? 12 : 26)) / 2;
                return stage switch
                {
                    2 => RightShard,
                    5 => LeftShard,
                    6 => offset >= 26 ? CentreShard : shard < 2 ? RightShard : LeftShard,
                    _ => shard < 2 ? LeftShard : CentreShard,
                };
            }
            return offset switch
            {
                0 => 1,
                2 => DrawAt(stage),
                4 => RoomPlmInstructionCodes.GotoIfRoomArgumentLess,
                6 => (ushort)(2 * (stage + 1)),
                8 => (ushort)start,
                10 or 24 => RoomPlmInstructionCodes.SpawnFourMotherBrainGlassShards,
                20 => 4,
                22 or 36 => DrawAt(stage + 1),
                34 => 48,
                38 => RoomPlmInstructionCodes.SetEvent,
                40 => (ushort)EventNumber.MotherBrainGlassDestroyed,
                _ => RoomPlmInstructionCodes.Delete,
            };
        }
        throw new InvalidOperationException("Glass program stage domain is inconsistent.");
    }

    /// <summary>Selects the draw-definition pointer for the requested pane-damage or shatter stage.</summary>
    /// <param name="stage">Zero-based stage in the glass damage sequence.</param>
    /// <returns>The compiled draw-list pointer for that stage, with later stages resolving to the final shatter.</returns>
    private static ushort DrawAt(int stage) => stage switch
    {
        0 => MotherBrainGlassPlmDrawDefinitions.Initial,
        1 => MotherBrainGlassPlmDrawDefinitions.PaneDamage1,
        2 => MotherBrainGlassPlmDrawDefinitions.PaneDamage2,
        3 => MotherBrainGlassPlmDrawDefinitions.PaneTransition,
        4 => MotherBrainGlassPlmDrawDefinitions.ShiftedPane1,
        5 => MotherBrainGlassPlmDrawDefinitions.ShiftedPane2,
        6 => MotherBrainGlassPlmDrawDefinitions.ShiftedPane3,
        7 => MotherBrainGlassPlmDrawDefinitions.Shatter1,
        8 => MotherBrainGlassPlmDrawDefinitions.Shatter2,
        _ => MotherBrainGlassPlmDrawDefinitions.Shatter3,
    };
}
