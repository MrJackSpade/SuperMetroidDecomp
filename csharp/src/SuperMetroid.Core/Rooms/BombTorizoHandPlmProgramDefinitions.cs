namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Fixed bank-$84 control and draw-selector bytes for Bomb Torizo's crumbling
/// Chozo hand. The physical draw lists at $9877/$989D remain separate.
/// </summary>
internal static class BombTorizoHandPlmProgramDefinitions
{
    /// <summary>First byte of the hand instruction list at $84:D368.</summary>
    internal const ushort FirstAddress = 0xd368;
    /// <summary>Last byte of the hand instruction list at $84:D3C6.</summary>
    internal const ushort LastAddress = 0xd3c6;
    /// <summary>Native fragment-sheet destination, VRAM word $6E00, from the $84:D376 record.</summary>
    internal const ushort DebrisDestinationWord = 0x6e00;

    /// <summary>$84:D33B: wake this sleeping PLM once Samus has Bombs.</summary>
    internal const ushort WakeIfSamusHasBombsPreInstruction = 0xd33b;
    /// <summary>$84:D37F: first of eight eight-byte timed-draw/spawn/argument records.</summary>
    private const ushort DebrisRecords = 0xd37f;
    /// <summary>$84:D3BF: cleared-hand draw, music command and deletion after the eighth fragment.</summary>
    private const ushort Completion = 0xd3bf;

    internal static bool TryReadMechanicsWord(ushort address, out ushort value)
    {
        value = 0;
        if (address is < FirstAddress or >= LastAddress) return false;
        TryReadMechanicsByte(address, out byte low);
        TryReadMechanicsByte((ushort)(address + 1), out byte high);
        value = (ushort)(low | high << 8);
        return true;
    }

    /// <summary>
    /// Decode all ninety-five original bytes, including the packed seven-byte DMA
    /// payload that changes word alignment. The eight debris selectors advance by
    /// two; holds are the initial 96/48 ticks followed by a countdown 15..10. Those
    /// are the supported NTSC values. No persistent program blob/cache remains;
    /// the word API preserves all ninety-four overlapping little-endian views.
    /// </summary>
    internal static bool TryReadMechanicsByte(ushort address, out byte value)
    {
        value = 0;
        if (address is < FirstAddress or > LastAddress) return false;
        if (address is >= 0xd378 and <= 0xd37e)
        {
            int packed = address <= 0xd379 ? SuperMetroid.Core.Assets.TorizoInstructionTileRomData.ChozoDebrisByteCount :
                address <= 0xd37c ? SuperMetroid.Core.Assets.TorizoInstructionTileRomData.ChozoDebrisSource : DebrisDestinationWord;
            int start = address <= 0xd379 ? 0xd378 : address <= 0xd37c ? 0xd37a : 0xd37d;
            value = (byte)(packed >> ((address - start) * 8));
            return true;
        }
        ushort word;
        int byteInWord;
        if (address is >= DebrisRecords and < Completion)
        {
            int offset = address - DebrisRecords;
            int fragment = offset / 8;
            word = (offset % 8 / 2) switch
            {
                0 => (ushort)(fragment switch { 0 => 96, 1 => 48, _ => 17 - fragment }),
                1 => BombTorizoHandPlmDrawDefinitions.Intact,
                2 => (ushort)RoomPlmInstruction.SpawnTorizoStatueBreaking,
                _ => (ushort)(fragment * 2),
            };
            byteInWord = offset & 1;
        }
        else if (address >= Completion)
        {
            int offset = address - Completion;
            word = (offset / 2) switch
            {
                0 => 1,
                1 => BombTorizoHandPlmDrawDefinitions.Cleared,
                2 => (ushort)RoomPlmInstruction.QueueSongOneMusicTrack,
                _ => (ushort)RoomPlmInstruction.Delete,
            };
            byteInWord = offset & 1;
        }
        else
        {
            word = (address & ~1) switch
            {
                0xd368 => 1,
                0xd36a or 0xd374 => BombTorizoHandPlmDrawDefinitions.Intact,
                0xd36c => (ushort)RoomPlmInstruction.InstallPreInstruction,
                0xd36e => WakeIfSamusHasBombsPreInstruction,
                0xd370 => (ushort)RoomPlmInstruction.Sleep,
                0xd372 => 120,
                _ => (ushort)RoomPlmInstruction.CopyFromRamToVram, // D376 only.
            };
            byteInWord = address & 1;
        }
        value = (byte)(word >> (byteInWord * 8));
        return true;
    }
}
