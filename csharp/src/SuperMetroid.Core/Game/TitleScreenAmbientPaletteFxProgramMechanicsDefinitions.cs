namespace SuperMetroid.Core.Game;

/// <summary>The mutually exclusive title-screen ambient palette owner.</summary>
public enum TitleScreenAmbientPaletteFxProgramOwner
{
    /// <summary>The baby-Metroid tube light.</summary>
    BabyMetroidTubeLight,
    /// <summary>The flickering title-screen displays.</summary>
    FlickeringDisplays,
}

/// <summary>Immutable mechanics for the looping title-screen ambient palettes.</summary>
/// <remarks>
/// Definitions <c>$E1A0</c> and <c>$E1A4</c> use distinct record counts and cadences.
/// Their 36 BGR555 words are installed presentation data; this catalog owns palette
/// placement, timing, waits, and loop branches. Diagnostic sessions without an installed
/// presentation retain the cartridge-backed color path.
/// </remarks>
public static class TitleScreenAmbientPaletteFxProgramMechanicsDefinitions
{
    private static readonly TitleScreenAmbientPaletteFxProgramDefinition[] Definitions =
    [
        new(
            TitleScreenAmbientPaletteFxProgramOwner.BabyMetroidTubeLight,
            definitionPointer: 0xe1a0,
            programStart: 0xc7fa,
            colorByteIndex: 0x0054,
            frameCount: 8,
            colorsPerFrame: 4,
            frameDuration: 10),
        new(
            TitleScreenAmbientPaletteFxProgramOwner.FlickeringDisplays,
            definitionPointer: 0xe1a4,
            programStart: 0xc862,
            colorByteIndex: 0x005c,
            frameCount: 2,
            colorsPerFrame: 2,
            frameDuration: 1),
    ];
    private static readonly IReadOnlyList<TitleScreenAmbientPaletteFxProgramDefinition>
        ReadOnlyDefinitions = Array.AsReadOnly(Definitions);

    /// <summary>The tube-light and display loops in definition order.</summary>
    public static IReadOnlyList<TitleScreenAmbientPaletteFxProgramDefinition> All =>
        ReadOnlyDefinitions;

    /// <summary>Resolves one compiled mechanics word across both loops.</summary>
    public static bool TryReadMechanicsWord(ushort pointer, out ushort value)
    {
        foreach (TitleScreenAmbientPaletteFxProgramDefinition definition in Definitions)
        {
            if (definition.TryReadMechanicsWord(pointer, out value))
                return true;
        }

        value = 0;
        return false;
    }
}

/// <summary>One complete looping title-screen ambient palette program.</summary>
public sealed class TitleScreenAmbientPaletteFxProgramDefinition
{
    internal TitleScreenAmbientPaletteFxProgramDefinition(
        TitleScreenAmbientPaletteFxProgramOwner owner,
        ushort definitionPointer,
        ushort programStart,
        ushort colorByteIndex,
        int frameCount,
        int colorsPerFrame,
        ushort frameDuration)
    {
        Owner = owner;
        DefinitionPointer = definitionPointer;
        ProgramStart = programStart;
        ColorByteIndex = colorByteIndex;
        FrameCount = frameCount;
        ColorsPerFrame = colorsPerFrame;
        FrameDuration = frameDuration;
    }

    /// <summary>The mutually exclusive title-screen palette owner.</summary>
    public TitleScreenAmbientPaletteFxProgramOwner Owner { get; }

    /// <summary>The palette-FX definition identity that installs this program.</summary>
    /// <remarks><c>$8D:E1A0</c> is tube light and <c>$8D:E1A4</c> is displays.</remarks>
    public ushort DefinitionPointer { get; }

    /// <summary>The native instruction-list entry.</summary>
    /// <remarks><c>$8D:C7FA</c> is tube light and <c>$8D:C862</c> is displays.</remarks>
    public ushort ProgramStart { get; }

    /// <summary>The first destination byte in CGRAM.</summary>
    public ushort ColorByteIndex { get; }

    /// <summary>The number of timed records in one loop.</summary>
    public int FrameCount { get; }

    /// <summary>The number of live BGR555 colors in each record.</summary>
    public int ColorsPerFrame { get; }

    /// <summary>The cartridge-authored duration of each timed record.</summary>
    public ushort FrameDuration { get; }

    /// <summary>Bytes from one duration through its terminal wait command.</summary>
    public int FrameByteCount => sizeof(ushort) + ColorsPerFrame * sizeof(ushort) +
        sizeof(ushort);

    /// <summary>The complete loop duration in frames.</summary>
    public int CycleFrames => FrameCount * FrameDuration;

    /// <summary>The first timed record after color-index setup.</summary>
    public ushort FirstFramePointer => unchecked((ushort)(ProgramStart + 4));

    /// <summary>The terminal <c>goto</c> command after all timed records.</summary>
    public ushort LoopInstructionPointer =>
        unchecked((ushort)(FirstFramePointer + FrameCount * FrameByteCount));

    /// <summary>Returns one timed-record pointer.</summary>
    public ushort FramePointer(int frame)
    {
        if ((uint)frame >= FrameCount)
            throw new ArgumentOutOfRangeException(nameof(frame));
        return unchecked((ushort)(FirstFramePointer + frame * FrameByteCount));
    }

    /// <summary>Resolves one compiled mechanics word while excluding live colors.</summary>
    public bool TryReadMechanicsWord(ushort pointer, out ushort value)
    {
        value = pointer switch
        {
            var item when item == ProgramStart => PaletteFxInstructionCodes.SetColorIndex,
            var item when item == ProgramStart + 2 => ColorByteIndex,
            var item when item == LoopInstructionPointer => PaletteFxInstructionCodes.Goto,
            var item when item == LoopInstructionPointer + 2 => FirstFramePointer,
            _ => 0,
        };
        if (value != 0)
            return true;

        for (int frame = 0; frame < FrameCount; frame++)
        {
            int offset = pointer - FramePointer(frame);
            value = offset switch
            {
                0 => FrameDuration,
                var item when item == FrameByteCount - sizeof(ushort) =>
                    PaletteFxInstructionCodes.Wait,
                _ => 0,
            };
            if (value != 0)
                return true;
        }

        return false;
    }
}
