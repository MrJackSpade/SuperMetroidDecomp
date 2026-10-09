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

    /// <summary>Number of interleaved graphics and hitbox spritemap operands in the compiled Hibashi programs.</summary>
    public static int PresentationWordCount => 24;

    /// <summary>Gets the bank-local address of one visual operand in the graphics or hitbox program.</summary>
    /// <param name="index">Zero-based operand index from zero through <see cref="PresentationWordCount"/> minus one.</param>
    /// <returns>The native instruction address containing the selected spritemap pointer.</returns>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)(index < 23 ? GraphicsProgram + 4 + index * 6 : HitboxProgram + 2);
    }

    /// <summary>Reads a compiled duration or callback word and rejects addresses outside the known mechanics positions.</summary>
    /// <param name="address">Bank-$A6 instruction address whose mechanics value is requested.</param>
    /// <returns>The compiled instruction value at that address.</returns>
    /// <exception cref="InvalidDataException">The address is not a compiled mechanics word.</exception>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        if (TryRead(address, out ushort value)) return value;
        throw new InvalidDataException(
            $"Hibashi instruction mechanics pointer $A6:{address:X4} is not compiled.");
    }

    /// <summary>Attempts to resolve one bank-local Hibashi program address to its compiled mechanics word.</summary>
    /// <param name="address">Bank-$A6 address to classify.</param>
    /// <param name="value">Receives the compiled value when found, or zero when the address is not mechanics data.</param>
    /// <returns><see langword="true"/> when the address contains a compiled command, duration, or activity callback.</returns>
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
