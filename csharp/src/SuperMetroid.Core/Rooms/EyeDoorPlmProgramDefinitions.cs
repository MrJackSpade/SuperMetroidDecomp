namespace SuperMetroid.Core.Rooms;

/// <summary>Shared eye-door control sequences with calculated mirrored operands; no encoded program cache.</summary>
internal static class EyeDoorPlmProgramDefinitions
{
    /// <summary>$84:D81E, InstList_PLM_EyeDoorEyeFacingLeft_0.</summary>
    internal const ushort FirstAddress = 0xd81e;
    /// <summary>$84:DA8B, final byte of the right bottom-component delete instruction.</summary>
    internal const ushort LastAddress = 0xda8b;
    /// <summary>$84:D955 follows the left eye and two passive lists; each orientation occupies 311 bytes.</summary>
    private const int OrientationBytes = 311;
    /// <summary>$84:D81E-D8E8, active eye list before the 54-byte middle and bottom lists.</summary>
    private const int EyeBytes = 203;
    /// <summary>$84:9C03, DrawInst_EyeDoorEyeFacingLeft_1, first two-cell eye pose.</summary>
    private const ushort EyeDrawFirst = 0x9c03;
    /// <summary>$84:9C2B, DrawInst_EyeDoorFacingLeft_0, first passive middle pose.</summary>
    private const ushort ComponentDrawFirst = 0x9c2b;
    /// <summary>$84:9C5B minus $84:9C03: mirrored draw groups differ by 88 bytes.</summary>
    private const int DrawMirrorBytes = 88;
    /// <summary>$84:A9A7, DrawInst_EyeDoorEyeFacingLeft, replacement blue-cap draw.</summary>
    private const ushort LeftBlueCapDraw = 0xa9a7;
    /// <summary>$84:A9E3, DrawInst_EyeDoorEyeFacingRight, replacement blue-cap draw.</summary>
    private const ushort RightBlueCapDraw = 0xa9e3;

    internal static bool TryReadMechanicsWord(ushort address, out ushort value)
    {
        value = 0;
        if (address is < FirstAddress or >= LastAddress) return false;
        value = (ushort)(ByteAt(address) | ByteAt(address + 1) << 8);
        return true;
    }

    internal static bool TryReadMechanicsByte(ushort address, out byte value)
    {
        value = 0;
        if (address is < FirstAddress or > LastAddress) return false;
        value = ByteAt(address);
        return true;
    }

    private static byte ByteAt(int address)
    {
        int orientation = (address - FirstAddress) / OrientationBytes;
        int local = (address - FirstAddress) % OrientationBytes;
        int start = FirstAddress + orientation * OrientationBytes;
        if (local >= EyeBytes)
        {
            int component = (local - EyeBytes) / 54;
            int offset = (local - EyeBytes) % 54;
            return (byte)(ComponentWord(start + EyeBytes + component * 54, offset & ~1,
                orientation != 0, component) >> ((offset & 1) * 8));
        }
        // Sound, hit threshold and loop count each occupy one byte, changing word parity.
        if (local == 0x64) return 9;
        if (local == 0x6b) return 3;
        if (local == 0xb4) return 10;
        int wordOffset = local < 0x64 ? local & ~1 :
            local < 0x6b ? 0x65 + ((local - 0x65) & ~1) :
            local < 0xb4 ? 0x6c + ((local - 0x6c) & ~1) :
            0xb5 + ((local - 0xb5) & ~1);
        return (byte)(EyeWord(start, wordOffset, orientation != 0) >> ((local - wordOffset) * 8));
    }

    private static ushort ComponentWord(int start, int offset, bool right, int component)
    {
        int draw = ComponentDrawFirst + (right ? DrawMirrorBytes : 0) + component * 18;
        // Each passive component animates forward through three poses, then back through the middle.
        if (offset is >= 0x1a and < 0x2a)
        {
            int phase = (offset - 0x1a) / 4;
            return (offset & 2) != 0 ? (ushort)(right ? 6 : 8) :
                (ushort)(draw + (2 - Math.Abs(phase - 2)) * 6);
        }
        return offset switch
        {
            0x00 => (ushort)RoomPlmInstruction.GotoIfDoorBitSet,
            0x02 or 0x14 => (ushort)(start + 0x34), // Delete after the door bit is set.
            0x04 or 0x2a => (ushort)RoomPlmInstruction.GotoIfSamusNear,
            0x06 or 0x2c => 0x1006, // Six columns, sixteen rows.
            0x08 => (ushort)(start + 0x12), // Install the door-bit callback.
            0x0a => 8,
            0x0c => (ushort)draw,
            0x0e or 0x30 => (ushort)RoomPlmInstruction.Goto,
            0x10 or 0x32 => (ushort)(start + 4), // Resume proximity wait.
            0x12 => (ushort)RoomPlmInstruction.LinkInstruction,
            0x16 => (ushort)RoomPlmInstruction.InstallPreInstruction,
            0x18 => EyeDoorPlmRomData.WakeWhenDoorBitSetPreInstruction,
            0x2e => (ushort)(start + 0x1a), // Continue the four-pose cycle.
            0x34 => (ushort)RoomPlmInstruction.Delete,
            _ => throw new InvalidOperationException("Unclassified eye-door component word."),
        };
    }

    private static ushort EyeWord(int start, int offset, bool right)
    {
        int eye = EyeDrawFirst + (right ? DrawMirrorBytes : 0);
        ushort makeBlue = right ? (ushort)RoomPlmInstruction.MoveUpAndMakeBlueDoorFacingRight :
            (ushort)RoomPlmInstruction.MoveUpAndMakeBlueDoorFacingLeft;
        return offset switch
        {
            // Proximity wait, missile callback, three shots, hit reaction and terminal conversion.
            0x00 => (ushort)RoomPlmInstruction.GotoIfDoorBitSet,
            0x08 or 0x1e or 0x50 => (ushort)RoomPlmInstruction.GotoIfSamusNear,
            0x0e or 0x56 or 0x5e or 0xa2 or 0xc1 or 0xc7 => (ushort)RoomPlmInstruction.Goto,
            0x12 => (ushort)RoomPlmInstruction.LinkInstruction,
            0x16 => (ushort)RoomPlmInstruction.InstallPreInstruction,
            0x28 or 0x30 or 0x38 => (ushort)RoomPlmInstruction.ShootEyeDoorProjectile,
            0x62 => (ushort)RoomPlmInstruction.QueueSoundLibrary2Maximum6,
            0x65 or 0x67 or 0x76 or 0x84 or 0xac or 0xae => (ushort)RoomPlmInstruction.SpawnTwoEyeDoorSmoke,
            0x69 => (ushort)RoomPlmInstruction.IncrementDoorHitCounterAndGoto,
            0x92 => (ushort)RoomPlmInstruction.SpawnEyeDoorSweat,
            0xa6 => (ushort)RoomPlmInstruction.ClearPreInstruction,
            0xa8 or 0xaa => (ushort)RoomPlmInstruction.SpawnEyeDoorSmoke,
            0xb0 or 0xc5 => makeBlue,
            0xb2 => (ushort)RoomPlmInstruction.SetEightBitTimer,
            0xbd => (ushort)RoomPlmInstruction.DecrementTimerAndGoto,

            0x02 => (ushort)(start + 0xc5),
            0x0c => (ushort)(start + 0x12),
            0x10 or 0x58 => (ushort)(start + 4),
            0x14 => (ushort)(start + 0x62),
            0x22 => (ushort)(start + 0x5a),
            0x54 or 0x60 or 0xa4 => (ushort)(start + 0x1e),
            0x6c => (ushort)(start + 0xa6),
            0xbf => (ushort)(start + 0xb5),
            0xc3 or 0xc9 => right ? BlueDoorPlmProgramDefinitions.ClosedRight : BlueDoorPlmProgramDefinitions.ClosedLeft,

            0x18 => EyeDoorPlmRomData.MissileHitPreInstruction,
            0x0a or 0x52 => 0x0406,
            0x20 => 0x0401,
            0x2a or 0x32 or 0x3a => (ushort)(right ? 20 : 0),
            0x94 => (ushort)(right ? 4 : 0),

            0x04 or 0x5a or 0x8a or 0x9a or 0x9e or 0xb9 => 4,
            0x1a or 0x8e => 8,
            0x24 or 0x3c => 64,
            0x2c or 0x34 => 32,
            0x40 or 0x4c => 6,
            0x44 or 0x48 => 48,
            0x6e or 0x72 or 0x78 or 0x7c or 0x80 or 0x86 => 2,
            0x96 => 56,
            0xb5 => 3,

            0x06 or 0x46 or 0x4a or 0x5c or 0x90 or 0x98 => (ushort)eye,
            0x1c or 0x42 or 0x4e or 0x8c or 0x9c => (ushort)(eye + 8),
            0x26 or 0x2e or 0x36 or 0x3e => (ushort)(eye + 16),
            0x70 or 0x7a or 0x82 => (ushort)(eye + 24),
            0x74 or 0x7e or 0x88 or 0xa0 => (ushort)(eye + 32),
            0xb7 => (ushort)(EyeDoorPlmDrawDefinitions.MirroredOpeningClear + (right ? DrawMirrorBytes : 0)),
            0xbb => right ? RightBlueCapDraw : LeftBlueCapDraw,
            _ => throw new InvalidOperationException("Unclassified eye-door active word."),
        };
    }
}