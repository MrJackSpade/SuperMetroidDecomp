namespace SuperMetroid.Core.Game;

/// <summary>
/// Botwoon's eight mouth-closed head movement instruction lists in bank <c>$B3</c>,
/// in native direction order. The unused horizontal list at <c>$B3:9359</c> is not a member.
/// </summary>
internal enum BotwoonMovementProgram : ushort
{
    /// <summary><c>InstList_Botwoon_MouthClosed_AimingUpLeft</c> at $B3:9341.</summary>
    UpLeft = 0x9341,
    /// <summary><c>InstList_Botwoon_MouthClosed_AimingLeft</c> at $B3:9349.</summary>
    Left = 0x9349,
    /// <summary><c>InstList_Botwoon_MouthClosed_AimingDownLeft</c> at $B3:9351.</summary>
    DownLeft = 0x9351,
    /// <summary><c>InstList_Botwoon_MouthClosed_AimingDown_FacingRight</c> at $B3:9361.</summary>
    Down = 0x9361,
    /// <summary><c>InstList_Botwoon_MouthClosed_AimingDownRight</c> at $B3:9369.</summary>
    DownRight = 0x9369,
    /// <summary><c>InstList_Botwoon_MouthClosed_AimingRight</c> at $B3:9371.</summary>
    Right = 0x9371,
    /// <summary><c>InstList_Botwoon_MouthClosed_AimingUpRight</c> at $B3:9379.</summary>
    UpRight = 0x9379,
    /// <summary><c>InstList_Botwoon_MouthClosed_AimingUp_FacingRight</c> at $B3:9381.</summary>
    Up = 0x9381,
}

/// <summary>
/// Compiled engine-control words for Botwoon's selector-reachable head movement,
/// hiding, and spit programs. Interleaved spritemap operands select installed presentation art.
/// </summary>
internal abstract class BotwoonInstructionProgramDefinitions
{
    /// <summary><c>InstList_Botwoon_Hide</c> at $B3:9389.</summary>
    internal const ushort Hidden = 0x9389;

    /// <summary><c>InstList_Botwoon_Spit_AimingUpLeft</c> at $B3:939F.</summary>
    internal const ushort SpittingUpLeft = 0x939f;
    public static int PresentationWordCount => 25;
    internal static int PhysicalDirection(int index) => index < 3 ? index : index + 1;
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        if (index < 8) return (ushort)((ushort)BotwoonMovementProgram.UpLeft + 8 * PhysicalDirection(index) + 4);
        if (index == 8) return Hidden + 2;
        int frame = index - 9;
        return (ushort)(SpittingUpLeft + 16 * PhysicalDirection(frame / 2) + (frame % 2 == 0 ? 2 : 12));
    }
    internal static bool IsPresentationWord(ushort address) => address == Hidden + 2 ||
        (TryDecodeDirectional(address, out bool spitting, out _, out int offset) &&
            (spitting ? offset is 2 or 12 : offset == 4));

    internal static ushort ReadMechanicsWord(ushort address)
    {
        if (address == Hidden) return 1;
        if (address == Hidden + 4) return CommonEnemyInstructionCodes.Sleep;
        if (TryDecodeDirectional(address, out bool spitting, out BotwoonMovementProgram movement, out int offset))
        {
            if (!spitting)
            {
                if (offset == 0) return RadiusInstruction(movement);
                if (offset == 2) return 1;
                if (offset == 6) return CommonEnemyInstructionCodes.Sleep;
            }
            else
            {
                switch (offset)
                {
                    case 0: return 32;
                    case 4: return RadiusInstruction(movement);
                    case 6: return (ushort)BotwoonInstruction.QueueSpitSFX;
                    case 8: return (ushort)BotwoonInstruction.SetSpittingFlag;
                    case 10: return (ushort)(movement == BotwoonMovementProgram.Left ? 25 : 16);
                    case 14: return CommonEnemyInstructionCodes.Sleep;
                }
            }
        }
        throw new InvalidDataException($"Botwoon instruction mechanics pointer $B3:{address:X4} is not compiled.");
    }
    private static ushort RadiusInstruction(BotwoonMovementProgram movement) => movement switch
    {
        BotwoonMovementProgram.UpLeft => (ushort)BotwoonInstruction.EnemyRadius_CxC,
        BotwoonMovementProgram.Left => (ushort)BotwoonInstruction.EnemyRadius_10x8,
        BotwoonMovementProgram.DownLeft => (ushort)BotwoonInstruction.EnemyRadius_CxC_duplicate,
        BotwoonMovementProgram.Down => (ushort)BotwoonInstruction.EnemyRadius_8x10_duplicate_again,
        BotwoonMovementProgram.DownRight => (ushort)BotwoonInstruction.EnemyRadius_CxC_duplicate_again,
        BotwoonMovementProgram.Right => (ushort)BotwoonInstruction.EnemyRadius_10x8_duplicate,
        BotwoonMovementProgram.UpRight => (ushort)BotwoonInstruction.EnemyRadius_CxC_duplicate_again2,
        BotwoonMovementProgram.Up => (ushort)BotwoonInstruction.EnemyRadius_8x10_duplicate_again2,
        _ => throw new InvalidOperationException($"Undefined {nameof(BotwoonMovementProgram)} {movement}."),
    };
    internal static bool TryDecodeDirectional(ushort address, out bool spitting, out BotwoonMovementProgram movement, out int offset)
    {
        spitting = address >= SpittingUpLeft;
        int relative = address - (spitting ? SpittingUpLeft : (ushort)BotwoonMovementProgram.UpLeft);
        int stride = spitting ? 16 : 8;
        int direction = relative / stride;
        if ((uint)relative >= 9 * stride || direction == 3)
        {
            movement = default; offset = 0; return false;
        }
        movement = ClosedNativeWords.Decode<BotwoonMovementProgram>(
            (ushort)((ushort)BotwoonMovementProgram.UpLeft + 8 * direction), "Botwoon movement program");
        offset = relative % stride;
        return true;
    }
}
