namespace SuperMetroid.Core.Game;

/// <summary>Compiled mechanics words from Hibashi's paired graphics and hitbox programs.</summary>
/// <remarks>
/// Durations and instruction callbacks are immutable simulation data. The 24 interleaved
/// spritemap selections resolve compiled identities to installed artwork.
/// </remarks>
internal abstract class HibashiInstructionProgramDefinitions
{
    /// <summary><c>$A6:8D1B</c>, the visible eruption graphics program.</summary>
    internal const ushort GraphicsProgram = 0x8d1b;

    /// <summary><c>$A6:8DA9</c>, the invisible collision-part program.</summary>
    internal const ushort HitboxProgram = 0x8da9;

    /// <summary><c>Instruction_Hibashi_PlaySFX</c> at $A6:8DAF queues the pillar sound before animation.</summary>
    private const ushort PlaySound = 0x8daf;
    /// <summary><c>Instruction_Hibashi_ActivityFrame0</c> at $A6:8E13 also sets the initial X radius.</summary>
    private const ushort FirstActivityFrame = 0x8e13;
    /// <summary><c>Instruction_Hibashi_ActivityFrame1</c> at $A6:8E2D starts the twenty-byte activity callback stride.</summary>
    private const ushort FollowingActivityFrames = 0x8e2d;
    public static int PresentationWordCount => 24;

    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)(index < 23 ? GraphicsProgram + 4 + index * 6 : HitboxProgram + 2);
    }

    internal static ushort ReadMechanicsWord(ushort address)
    {
        if (TryRead(address, out ushort value)) return value;
        throw new InvalidDataException(
            $"Hibashi instruction mechanics pointer $A6:{address:X4} is not compiled.");
    }

    internal static bool TryRead(int address, out ushort value)
    {
        value = address switch
        {
            GraphicsProgram => PlaySound,
            HitboxProgram => 2,
            HitboxProgram - 2 or HitboxProgram + 4 => CommonEnemyInstructionCodes.Sleep,
            _ => 0,
        };
        if (value != 0) return true;
        int offset = address - GraphicsProgram - 2;
        if (offset < 0 || offset >= 23 * 6) return false;
        int frame = offset / 6;
        if (offset % 6 == 0)
        {
            value = (ushort)(frame < 4 ? 2 : frame < 8 ? 1 : frame < 16 ? 2 : 4);
            return true;
        }
        if (offset % 6 == 4)
        {
            value = frame == 0 ? FirstActivityFrame : (ushort)(FollowingActivityFrames + (frame - 1) * 20);
            return true;
        }
        return false;
    }
}