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

    /// <summary>The fixed control-word addresses of the two programs.</summary>
    private enum ControlWord : ushort
    {
        /// <summary>The graphics program's opening sound instruction.</summary>
        PlaySound = GraphicsProgram,
        /// <summary>The sleep closing the graphics program, just before the hitbox program.</summary>
        GraphicsSleep = HitboxProgram - 2,
        /// <summary>The hitbox program's single two-frame duration.</summary>
        HitboxDuration = HitboxProgram,
        /// <summary>The sleep closing the hitbox program.</summary>
        HitboxSleep = HitboxProgram + 4,
    }

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
        value = 0;
        if ((uint)address <= ushort.MaxValue && Enum.IsDefined((ControlWord)address))
        {
            value = (ControlWord)address switch
            {
                ControlWord.PlaySound => (ushort)HibashiInstruction.PlaySFX,
                ControlWord.HitboxDuration => 2,
                ControlWord.GraphicsSleep or ControlWord.HitboxSleep => (ushort)CommonEnemyInstruction.Sleep,
                _ => throw new InvalidOperationException($"Undefined ControlWord {(ControlWord)address}."),
            };
            return true;
        }
        int offset = address - GraphicsProgram - 2;
        if (offset is < 0 or >= (23 * 6)) return false;
        int frame = offset / 6;
        if (offset % 6 == 0)
        {
            value = (ushort)(frame < 4 ? 2 : frame < 8 ? 1 : frame < 16 ? 2 : 4);
            return true;
        }
        if (offset % 6 == 4)
        {
            value = frame == 0
                ? (ushort)HibashiInstruction.ActivityFrame0
                : (ushort)((ushort)HibashiInstruction.ActivityFrame1 + (frame - 1) * 20);
            return true;
        }
        return false;
    }
}