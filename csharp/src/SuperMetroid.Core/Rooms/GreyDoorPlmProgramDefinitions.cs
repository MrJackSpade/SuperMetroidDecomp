namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Cartridge-authored bank-$84 instruction streams for all four ordinary grey
/// doors. Each orientation has a closing list and a condition-gated resident
/// list with flash and opening branches. The physical draws and editable tile
/// choices are separate resources; Bomb Torizo's special door is also separate.
/// </summary>
internal static class GreyDoorPlmProgramDefinitions
{
    /// <summary>First ordinary grey-door closing list at $84:BE59.</summary>
    internal const ushort FirstAddress = 0xbe59;
    /// <summary>Final byte of the facing-down opening list at $84:BFFC.</summary>
    internal const ushort LastAddress = 0xbffc;

    /// <summary>Each orientation occupies 105 bytes, including packed sound/hit arguments.</summary>
    private const int OrientationBytes = 105;
    /// <summary>$84:A677: first shared clear draw; orientations advance twelve bytes.</summary>
    private const ushort ClearLeft = 0xa677;
    /// <summary>$84:A6A7: first closed grey draw; orientations advance four twelve-byte frames.</summary>
    private const ushort GreyLeft = 0xa6a7;
    /// <summary>$84:A9B3: first closed blue flash draw; orientations advance five twelve-byte frames.</summary>
    private const ushort BlueLeft = 0xa9b3;
    /// <summary>$84:BD0F: follow the installed link when shot.</summary>
    private const ushort FollowLinkWhenShot = 0xbd0f;

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
    /// Decode all four structurally identical native orientation programs. Closing
    /// traverses frames 3..0; flashing repeats three blue/grey pairs; opening walks
    /// frames 1..3 then clears. Local links relocate by 105 bytes per orientation.
    /// Preserve all 420 bytes and 419 overlapping word views, including words
    /// crossing orientation boundaries. No stored program or generated cache remains.
    /// </summary>
    internal static bool TryReadMechanicsByte(ushort address, out byte value)
    {
        value = 0;
        if (address < FirstAddress || address > LastAddress) return false;
        int orientation = (address - FirstAddress) / OrientationBytes;
        int local = (address - FirstAddress) % OrientationBytes;
        int first = FirstAddress + orientation * OrientationBytes;
        int clear = ClearLeft + orientation * 12;
        int grey = GreyLeft + orientation * 48;
        if (local is 10 or 81 or 86)
        {
            value = local == 10 ? (byte)8 : local == 81 ? (byte)1 : (byte)7;
            return true;
        }
        int start;
        int word;
        if (local is >= 11 and < 23)
        {
            int offset = local - 11;
            int frame = 2 - offset / 4;
            word = offset % 4 < 2 ? (frame == 0 ? 1 : 2) : grey + frame * 12;
            start = 11 + offset / 2 * 2;
        }
        else if (local is >= 51 and < 75)
        {
            int offset = local - 51;
            bool blue = offset % 8 < 4;
            word = offset % 4 < 2 ? (blue ? 3 : 4) : blue ? BlueLeft + orientation * 60 : grey;
            start = 51 + offset / 2 * 2;
        }
        else if (local is >= 87 and < 103)
        {
            int offset = local - 87;
            int frame = offset / 4 + 1;
            word = offset % 4 < 2 ? (frame == 4 ? 1 : 4) : frame == 4 ? clear : grey + frame * 12;
            start = 87 + offset / 2 * 2;
        }
        else
        {
            start = local < 10 || local is >= 82 and < 86 ? local & ~1 : ((local - 1) & ~1) + 1;
            word = start switch
            {
                0 or 4 => 2,
                2 => clear,
                6 => grey + 3 * 12,
                8 or 84 => RoomPlmInstructionCodes.QueueSoundLibrary3Maximum6,
                23 => RoomPlmInstructionCodes.GotoIfDoorBitSet,
                25 => orientation switch
                {
                    0 => BlueDoorPlmProgramDefinitions.ClosedLeft,
                    1 => BlueDoorPlmProgramDefinitions.ClosedRight,
                    2 => BlueDoorPlmProgramDefinitions.ClosedUp,
                    _ => BlueDoorPlmProgramDefinitions.ClosedDown,
                },
                27 or 43 => RoomPlmInstructionCodes.LinkInstruction,
                29 => first + 43,
                31 => RoomPlmInstructionCodes.SetGreyDoorPreInstruction,
                33 => 1,
                35 => grey,
                37 => RoomPlmInstructionCodes.Sleep,
                39 or 75 => RoomPlmInstructionCodes.Goto,
                41 => first + 37,
                45 => first + 79,
                47 => RoomPlmInstructionCodes.InstallPreInstruction,
                49 => FollowLinkWhenShot,
                77 => first + 51,
                79 => RoomPlmInstructionCodes.IncrementDoorHitCounterAndGoto,
                82 => first + 84,
                _ => RoomPlmInstructionCodes.Delete, // Local 103 only.
            };
        }
        value = (byte)(word >> ((local - start) * 8));
        return true;
    }
}
