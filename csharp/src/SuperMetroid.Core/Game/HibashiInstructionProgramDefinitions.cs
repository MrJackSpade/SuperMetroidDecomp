namespace SuperMetroid.Core.Game;

/// <summary>Compiled mechanics words from Hibashi's paired graphics and hitbox programs.</summary>
/// <remarks>
/// Durations and instruction callbacks are immutable simulation data. The 24 interleaved
/// spritemap selections resolve compiled identities to installed artwork.
/// </remarks>
internal abstract class HibashiInstructionProgramDefinitions : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
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

    public static int MechanicsWordCount => 50;
    public static int PresentationWordCount => 24;

    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        int address = index switch
        {
            0 => GraphicsProgram,
            < 47 => GraphicsProgram + 2 + (index - 1) / 2 * 6 + (index - 1) % 2 * 4,
            47 => HitboxProgram - 2,
            48 => HitboxProgram,
            _ => HitboxProgram + 4,
        };
        return new((ushort)address, ReadMechanicsWord((ushort)address));
    }

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

    private static bool TryRead(int address, out ushort value)
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

    public static bool IsCompiledMechanicsByte(int address) =>
        (address & 0xff0000) == 0xa60000 &&
        (TryRead((ushort)address, out _) || TryRead((ushort)address - 1, out _));
}