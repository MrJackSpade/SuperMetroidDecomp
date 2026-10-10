namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Cartridge-authored bank-$84 control lists for all four ordinary blue-door
/// orientations: opening, closing, and closed-cap handoff. The sixteen physical
/// draw layouts and their editable visual words are separate resources.
/// </summary>
internal static class BlueDoorPlmProgramDefinitions
{
    /// <summary>First blue-door opening list, facing left, at $84:C489.</summary>
    internal const ushort FirstAddress = 0xc489;
    /// <summary>Last byte of the closed facing-down list at $84:C54C.</summary>
    internal const ushort LastAddress = 0xc54c;
    /// <summary>Facing-left closed-cap conversion list at $84:C4B1.</summary>
    internal const ushort ClosedLeft = 0xc4b1;
    /// <summary>Facing-right closed-cap conversion list at $84:C4E2.</summary>
    internal const ushort ClosedRight = 0xc4e2;
    /// <summary>Facing-up closed-cap conversion list at $84:C513.</summary>
    internal const ushort ClosedUp = 0xc513;
    /// <summary>Facing-down closed-cap conversion list at $84:C544.</summary>
    internal const ushort ClosedDown = 0xc544;

    /// <summary>Each orientation's opening/closing/closed group occupies 49 bytes.</summary>
    private const int OrientationBytes = 49;
    /// <summary>$84:A677: first shared clear draw; orientations advance twelve bytes.</summary>
    private const ushort ClearLeft = 0xa677;
    /// <summary>$84:A9B3: first closed blue cap; orientation groups span five twelve-byte draws.</summary>
    private const ushort ClosedDrawLeft = 0xa9b3;

    /// <summary>Reads a little-endian word view from the compiled blue-door PLM bytes, including overlapping instruction operands.</summary>
    /// <param name="address">Bank-$84 address of the word's first byte.</param>
    /// <param name="value">The combined word when the address belongs to the compiled program; otherwise zero.</param>
    /// <returns><see langword="true"/> when the address is in the compiled word-start range.</returns>
    internal static bool TryReadMechanicsWord(ushort address, out ushort value)
    {
        value = 0;
        if (address < FirstAddress || address >= LastAddress) return false;
        TryReadMechanicsByte(address, out byte low);
        TryReadMechanicsByte((ushort)(address + 1), out byte high);
        value = (ushort)(low | high << 8);
        return true;
    }

    /// <summary>
    /// Decode four NTSC orientation groups, preserving 196 bytes and all 195
    /// overlapping word views. Opening walks three six-tick frames, then clears
    /// for 94 ticks; closing reverses those frames at two ticks each and falls
    /// into the one-tick closed-cap handoff. Packed sounds/BTS change alignment.
    /// No persistent instruction table or generated cache remains.
    /// </summary>
    internal static bool TryReadMechanicsByte(ushort address, out byte value)
    {
        value = 0;
        if (address < FirstAddress || address > LastAddress) return false;
        int orientation = (address - FirstAddress) / OrientationBytes;
        int local = (address - FirstAddress) % OrientationBytes;
        int clear = ClearLeft + orientation * 12;
        int closed = ClosedDrawLeft + orientation * 60;
        if (local is 2 or 31 or 42)
        {
            value = local == 2 ? (byte)7 : local == 31 ? (byte)8 :
                (byte)(RoomBlockBehaviorValues.BlueDoorFacingLeft.Value + orientation);
            return true;
        }
        int start;
        int word;
        if (local is >= 3 and < 19)
        {
            int offset = local - 3;
            int frame = offset / 4 + 1;
            word = offset % 4 < 2 ? (frame == 4 ? 94 : 6) : frame == 4 ? clear : closed + frame * 12;
            start = 3 + offset / 2 * 2;
        }
        else if (local is >= 32 and < 40)
        {
            int offset = local - 32;
            int frame = 2 - offset / 4;
            word = offset % 4 < 2 ? 2 : closed + frame * 12;
            start = 32 + offset / 2 * 2;
        }
        else
        {
            start = local == 0 || local == 1 || local is 40 or 41 ? local & ~1 : ((local - 1) & ~1) + 1;
            word = start switch
            {
                0 or 29 => RoomPlmInstructionCodes.QueueSoundLibrary3Maximum6,
                19 or 47 => RoomPlmInstructionCodes.Delete,
                21 or 25 => 2,
                23 => clear,
                27 => closed + 3 * 12,
                40 => RoomPlmInstructionCodes.SetPlmBtsFromByte,
                43 => 1,
                _ => closed, // Local 45: final closed-cap draw.
            };
        }
        value = (byte)(word >> ((local - start) * 8));
        return true;
    }
}
