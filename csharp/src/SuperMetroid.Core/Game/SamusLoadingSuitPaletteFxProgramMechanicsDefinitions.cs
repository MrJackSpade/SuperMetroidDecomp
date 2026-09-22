namespace SuperMetroid.Core.Game;

/// <summary>The mutually exclusive suit palette loaded around Samus.</summary>
public enum SamusLoadingSuitPaletteFxProgramOwner
{
    /// <summary>Power-suit colors.</summary>
    PowerSuit,
    /// <summary>Varia-suit colors.</summary>
    VariaSuit,
    /// <summary>Gravity-suit colors.</summary>
    GravitySuit,
}

/// <summary>Immutable mechanics shared by the three Samus-loading palette programs.</summary>
/// <remarks>
/// Definitions <c>$E1F4</c>, <c>$E1F8</c>, and <c>$E1FC</c> share four counted
/// two-record groups and one terminal record. Their 432 BGR555 words remain live
/// presentation data; this catalog owns palette placement, timing, replay, and deletion.
/// </remarks>
public static class SamusLoadingSuitPaletteFxProgramMechanicsDefinitions
{
    /// <summary>All programs write CGRAM from byte index <c>$0180</c>.</summary>
    public const ushort ColorByteIndex = 0x0180;

    /// <summary>Each program has four independently counted two-record groups.</summary>
    public const int GroupCount = 4;

    /// <summary>Each counted group contains two palette records.</summary>
    public const int FramesPerGroup = 2;

    /// <summary>Each complete program contains eight counted records and one final record.</summary>
    public const int FrameCount = GroupCount * FramesPerGroup + 1;

    /// <summary>Each record writes sixteen live BGR555 colors.</summary>
    public const int ColorsPerFrame = 16;

    /// <summary>Bytes from one duration through its terminal wait command.</summary>
    public const int FrameByteCount = 36;

    /// <summary>Each record in a counted group lasts three frames.</summary>
    public const ushort GroupFrameDuration = 3;

    /// <summary>The terminal record lasts one frame.</summary>
    public const ushort FinalFrameDuration = 1;

    /// <summary>Every suit-loading program lasts 265 frames.</summary>
    public const int CycleFrames = 265;

    private static readonly ushort[] GroupStartOffsets = [0x0007, 0x0056, 0x00a5, 0x00f4];
    private static readonly byte[] GroupTimerValues = [0x24, 0x03, 0x03, 0x02];
    private static readonly SamusLoadingSuitPaletteFxProgramDefinition[] Definitions =
    [
        new(SamusLoadingSuitPaletteFxProgramOwner.PowerSuit, 0xe1f4, 0xdb62),
        new(SamusLoadingSuitPaletteFxProgramOwner.VariaSuit, 0xe1f8, 0xdcc8),
        new(SamusLoadingSuitPaletteFxProgramOwner.GravitySuit, 0xe1fc, 0xde2e),
    ];
    private static readonly IReadOnlyList<SamusLoadingSuitPaletteFxProgramDefinition>
        ReadOnlyDefinitions = Array.AsReadOnly(Definitions);

    /// <summary>The power, Varia, and gravity programs in definition order.</summary>
    public static IReadOnlyList<SamusLoadingSuitPaletteFxProgramDefinition> All =>
        ReadOnlyDefinitions;

    /// <summary>Returns the bank-local offset of one counted group.</summary>
    public static ushort GroupStartOffset(int group)
    {
        if ((uint)group >= GroupCount)
            throw new ArgumentOutOfRangeException(nameof(group));
        return GroupStartOffsets[group];
    }

    /// <summary>Returns the cartridge-authored replay count of one group.</summary>
    public static byte GroupTimerValue(int group)
    {
        if ((uint)group >= GroupCount)
            throw new ArgumentOutOfRangeException(nameof(group));
        return GroupTimerValues[group];
    }

    /// <summary>Resolves one compiled mechanics word across all three programs.</summary>
    public static bool TryReadMechanicsWord(ushort pointer, out ushort value)
    {
        foreach (SamusLoadingSuitPaletteFxProgramDefinition definition in Definitions)
        {
            if (definition.TryReadMechanicsWord(pointer, out value))
                return true;
        }

        value = 0;
        return false;
    }

    /// <summary>Resolves one compiled byte-sized group timer across all three programs.</summary>
    public static bool TryReadMechanicsByte(ushort pointer, out byte value)
    {
        foreach (SamusLoadingSuitPaletteFxProgramDefinition definition in Definitions)
        {
            if (definition.TryReadMechanicsByte(pointer, out value))
                return true;
        }

        value = 0;
        return false;
    }
}

/// <summary>One complete Samus-loading palette control program.</summary>
public sealed class SamusLoadingSuitPaletteFxProgramDefinition
{
    private const ushort FinalFrameOffset = 0x0140;
    private const ushort DeleteInstructionOffset = 0x0164;

    internal SamusLoadingSuitPaletteFxProgramDefinition(
        SamusLoadingSuitPaletteFxProgramOwner owner,
        ushort definitionPointer,
        ushort programStart)
    {
        Owner = owner;
        DefinitionPointer = definitionPointer;
        ProgramStart = programStart;
    }

    /// <summary>The mutually exclusive suit palette.</summary>
    public SamusLoadingSuitPaletteFxProgramOwner Owner { get; }

    /// <summary>The palette-FX definition identity that installs this program.</summary>
    /// <remarks><c>$8D:E1F4</c>, <c>$8D:E1F8</c>, or <c>$8D:E1FC</c>.</remarks>
    public ushort DefinitionPointer { get; }

    /// <summary>The native instruction-list entry.</summary>
    /// <remarks><c>$8D:DB62</c>, <c>$8D:DCC8</c>, or <c>$8D:DE2E</c>.</remarks>
    public ushort ProgramStart { get; }

    /// <summary>The terminal one-frame record after all counted groups.</summary>
    public ushort FinalFramePointer => unchecked((ushort)(ProgramStart + FinalFrameOffset));

    /// <summary>The terminal <c>delete</c> command after the final record.</summary>
    public ushort DeleteInstructionPointer =>
        unchecked((ushort)(ProgramStart + DeleteInstructionOffset));

    /// <summary>Returns the first record of one counted group.</summary>
    public ushort GroupStartPointer(int group) => unchecked((ushort)(
        ProgramStart + SamusLoadingSuitPaletteFxProgramMechanicsDefinitions
            .GroupStartOffset(group)));

    /// <summary>Returns the byte operand that initializes one group's replay timer.</summary>
    public ushort GroupTimerBytePointer(int group) =>
        unchecked((ushort)(GroupStartPointer(group) - 1));

    /// <summary>Returns one of the nine timed-record pointers.</summary>
    public ushort FramePointer(int frame)
    {
        if ((uint)frame >= SamusLoadingSuitPaletteFxProgramMechanicsDefinitions.FrameCount)
            throw new ArgumentOutOfRangeException(nameof(frame));
        if (frame == SamusLoadingSuitPaletteFxProgramMechanicsDefinitions.FrameCount - 1)
            return FinalFramePointer;

        int group = frame / SamusLoadingSuitPaletteFxProgramMechanicsDefinitions.FramesPerGroup;
        int frameInGroup = frame %
            SamusLoadingSuitPaletteFxProgramMechanicsDefinitions.FramesPerGroup;
        return unchecked((ushort)(GroupStartPointer(group) + frameInGroup *
            SamusLoadingSuitPaletteFxProgramMechanicsDefinitions.FrameByteCount));
    }

    /// <summary>Resolves one compiled mechanics word while excluding live colors.</summary>
    public bool TryReadMechanicsWord(ushort pointer, out ushort value)
    {
        value = pointer switch
        {
            var item when item == ProgramStart => PaletteFxInstructionCodes.SetColorIndex,
            var item when item == ProgramStart + 2 =>
                SamusLoadingSuitPaletteFxProgramMechanicsDefinitions.ColorByteIndex,
            var item when item == DeleteInstructionPointer => PaletteFxInstructionCodes.Delete,
            _ => 0,
        };
        if (value != 0)
            return true;

        for (int group = 0;
             group < SamusLoadingSuitPaletteFxProgramMechanicsDefinitions.GroupCount;
             group++)
        {
            ushort groupStart = GroupStartPointer(group);
            ushort setTimer = unchecked((ushort)(groupStart - 3));
            ushort decrement = unchecked((ushort)(groupStart +
                SamusLoadingSuitPaletteFxProgramMechanicsDefinitions.FramesPerGroup *
                SamusLoadingSuitPaletteFxProgramMechanicsDefinitions.FrameByteCount));
            value = pointer switch
            {
                var item when item == setTimer => PaletteFxInstructionCodes.SetTimer,
                var item when item == decrement =>
                    PaletteFxInstructionCodes.DecrementTimerAndGoto,
                var item when item == decrement + 2 => groupStart,
                _ => 0,
            };
            if (value != 0)
                return true;
        }

        for (int frame = 0;
             frame < SamusLoadingSuitPaletteFxProgramMechanicsDefinitions.FrameCount;
             frame++)
        {
            int offset = pointer - FramePointer(frame);
            value = offset switch
            {
                0 => frame == SamusLoadingSuitPaletteFxProgramMechanicsDefinitions.FrameCount - 1
                    ? SamusLoadingSuitPaletteFxProgramMechanicsDefinitions.FinalFrameDuration
                    : SamusLoadingSuitPaletteFxProgramMechanicsDefinitions.GroupFrameDuration,
                SamusLoadingSuitPaletteFxProgramMechanicsDefinitions.FrameByteCount -
                    sizeof(ushort) => PaletteFxInstructionCodes.Wait,
                _ => 0,
            };
            if (value != 0)
                return true;
        }

        return false;
    }

    /// <summary>Resolves one compiled byte-sized group timer.</summary>
    public bool TryReadMechanicsByte(ushort pointer, out byte value)
    {
        for (int group = 0;
             group < SamusLoadingSuitPaletteFxProgramMechanicsDefinitions.GroupCount;
             group++)
        {
            if (pointer != GroupTimerBytePointer(group))
                continue;
            value = SamusLoadingSuitPaletteFxProgramMechanicsDefinitions.GroupTimerValue(group);
            return true;
        }

        value = 0;
        return false;
    }
}
