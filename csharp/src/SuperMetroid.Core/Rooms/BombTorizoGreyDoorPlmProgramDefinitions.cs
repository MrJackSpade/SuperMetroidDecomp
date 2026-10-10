namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Bomb Torizo's bank-$84 grey-door instruction lists. The closing list is
/// interrupted by the native $BA6F Bombs-condition callback, whose machine
/// code is deliberately not part of this compiled instruction data.
/// </summary>
internal static class BombTorizoGreyDoorPlmProgramDefinitions
{
    /// <summary>Bomb-gated closing instruction list at $84:BA4C.</summary>
    internal const ushort ClosingStart = 0xba4c;
    /// <summary>Last closing-list byte before callback code at $84:BA6E.</summary>
    internal const ushort ClosingEnd = 0xba6e;
    /// <summary>Condition-gated resident instruction list at $84:BA7F.</summary>
    internal const ushort ResidentStart = 0xba7f;
    /// <summary>Last opening-list byte before unused setup code at $84:BAD0.</summary>
    internal const ushort ResidentEnd = 0xbad0;

    /// <summary>$84:BA8D: sleeping locked-door loop.</summary>
    private const ushort Locked = 0xba8d;
    /// <summary>$84:BA93: install the shot link after the room condition succeeds.</summary>
    private const ushort Activate = 0xba93;
    /// <summary>$84:BA9B: three pairs of three-tick blue/four-tick grey flashes.</summary>
    private const ushort Flash = 0xba9b;
    /// <summary>$84:BAB7: one hit sets the persistent door bit.</summary>
    private const ushort Hit = 0xbab7;
    /// <summary>$84:BABC: sound and opening frames.</summary>
    private const ushort Open = 0xbabc;
    /// <summary>$84:BD0F: follow the installed link when shot.</summary>
    private const ushort FollowLinkWhenShot = 0xbd0f;
    /// <summary>$84:A683: cleared right-facing door cap.</summary>
    private const ushort ClearDraw = 0xa683;
    /// <summary>$84:A6D7: closed right-facing grey cap; subsequent frames are twelve bytes apart.</summary>
    private const ushort ClosedDraw = 0xa6d7;
    /// <summary>$84:A9EF: closed right-facing blue cap used during flashing.</summary>
    private const ushort BlueDraw = 0xa9ef;

    internal static bool TryReadMechanicsWord(ushort address, out ushort value)
    {
        value = 0;
        if (!Owns(address) || address == ClosingEnd || address == ResidentEnd) return false;
        TryReadMechanicsByte(address, out byte low);
        TryReadMechanicsByte((ushort)(address + 1), out byte high);
        value = (ushort)(low | high << 8);
        return true;
    }

    /// <summary>
    /// Decode the NTSC closing/resident lists, preserving all 117 bytes and 115
    /// overlapping word views. Closing walks grey frames 3..0, opening 1..3 then
    /// clears the cap, and flashing repeats three identical blue/grey pairs.
    /// One-byte sound/hit operands change alignment. Callback machine code in
    /// BA6F..BA7E is excluded. No stored program or generated cache remains.
    /// </summary>
    internal static bool TryReadMechanicsByte(ushort address, out byte value)
    {
        value = 0;
        if (!Owns(address)) return false;
        if (address is 0xba5a or 0xbab9 or 0xbabe)
        {
            value = address == 0xba5a ? (byte)8 : address == 0xbab9 ? (byte)1 : (byte)7;
            return true;
        }
        int start;
        ushort word;
        if (address is >= 0xba5b and <= 0xba6a)
        {
            int offset = address - 0xba5b;
            int frame = 3 - offset / 4;
            word = offset % 4 < 2 ? (ushort)(frame == 0 ? 1 : 2) : (ushort)(ClosedDraw + frame * 12);
            start = 0xba5b + (offset / 2) * 2;
        }
        else if (address is >= Flash and < 0xbab3)
        {
            int offset = address - Flash;
            bool blue = offset % 8 < 4;
            word = offset % 4 < 2 ? (ushort)(blue ? 3 : 4) : blue ? BlueDraw : ClosedDraw;
            start = Flash + (offset / 2) * 2;
        }
        else if (address is >= 0xbabf and <= 0xbace)
        {
            int offset = address - 0xbabf;
            int frame = offset / 4 + 1;
            word = offset % 4 < 2 ? (ushort)(frame == 4 ? 1 : 4) :
                frame == 4 ? ClearDraw : (ushort)(ClosedDraw + frame * 12);
            start = 0xbabf + (offset / 2) * 2;
        }
        else
        {
            start = address < 0xba5a ? address & ~1 :
                address is >= 0xbaba and <= 0xbabd ? address & ~1 :
                ((address - 1) & ~1) + 1;
            word = start switch
            {
                0xba4c => 2,
                0xba4e or 0xba56 => ClearDraw,
                0xba50 => (ushort)RoomPlmInstruction.GotoIfSamusHasNoBombs,
                0xba52 => ClosingStart,
                0xba54 => 40,
                0xba58 or Open => (ushort)RoomPlmInstruction.QueueSoundLibrary3Maximum6,
                0xba6b or 0xba8f or 0xbab3 => (ushort)RoomPlmInstruction.Goto,
                0xba6d => ResidentStart,
                ResidentStart => (ushort)RoomPlmInstruction.GotoIfDoorBitSet,
                0xba81 => BlueDoorPlmProgramDefinitions.ClosedRight,
                0xba83 or Activate => (ushort)RoomPlmInstruction.LinkInstruction,
                0xba85 => Activate,
                0xba87 => (ushort)RoomPlmInstruction.SetGreyDoorPreInstruction,
                0xba89 => 1,
                0xba8b => ClosedDraw,
                Locked => (ushort)RoomPlmInstruction.Sleep,
                0xba91 => Locked,
                0xba95 => Hit,
                0xba97 => (ushort)RoomPlmInstruction.InstallPreInstruction,
                0xba99 => FollowLinkWhenShot,
                0xbab5 => Flash,
                Hit => (ushort)RoomPlmInstruction.IncrementDoorHitCounterAndGoto,
                0xbaba => Open,
                _ => (ushort)RoomPlmInstruction.Delete, // BACF only.
            };
        }
        value = (byte)(word >> ((address - start) * 8));
        return true;
    }

    private static bool Owns(ushort address) =>
        address is >= ClosingStart and <= ClosingEnd or >= ResidentStart and <= ResidentEnd;
}
