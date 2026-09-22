namespace SuperMetroid.Core.Game;

/// <summary>The mutually exclusive cinematic glow palette program.</summary>
public enum CinematicGlowPaletteFxProgramOwner
{
    /// <summary>The old Mother Brain fight's background-light pulse.</summary>
    OldMotherBrainBackgroundLights,
    /// <summary>The cinematic gunship glow.</summary>
    GunshipGlow,
}

/// <summary>Immutable mechanics for the old Mother Brain lights and gunship glow.</summary>
/// <remarks>
/// Definitions <c>$E1BC</c> and <c>$E1C0</c> both use fourteen timed records but
/// retain distinct widths and durations. Their 56 BGR555 words remain live presentation
/// data; this catalog owns palette placement, timing, waits, and loop control.
/// </remarks>
public static class CinematicGlowPaletteFxProgramMechanicsDefinitions
{
    /// <summary>Both glow loops contain fourteen timed records.</summary>
    public const int FrameCount = 14;

    private static readonly CinematicGlowPaletteFxProgramDefinition[] Definitions =
    [
        new(
            CinematicGlowPaletteFxProgramOwner.OldMotherBrainBackgroundLights,
            definitionPointer: 0xe1bc,
            programStart: 0xc9ba,
            colorByteIndex: 0x0028,
            colorsPerFrame: 3,
            frameDuration: 6),
        new(
            CinematicGlowPaletteFxProgramOwner.GunshipGlow,
            definitionPointer: 0xe1c0,
            programStart: 0xca4e,
            colorByteIndex: 0x01fe,
            colorsPerFrame: 1,
            frameDuration: 5),
    ];
    private static readonly IReadOnlyList<CinematicGlowPaletteFxProgramDefinition>
        ReadOnlyDefinitions = Array.AsReadOnly(Definitions);

    /// <summary>The old-Mother-Brain and gunship glow programs in definition order.</summary>
    public static IReadOnlyList<CinematicGlowPaletteFxProgramDefinition> All =>
        ReadOnlyDefinitions;

    /// <summary>Resolves one compiled mechanics word across both programs.</summary>
    public static bool TryReadMechanicsWord(ushort pointer, out ushort value)
    {
        foreach (CinematicGlowPaletteFxProgramDefinition definition in Definitions)
        {
            if (definition.TryReadMechanicsWord(pointer, out value))
                return true;
        }

        value = 0;
        return false;
    }
}

/// <summary>One complete cinematic glow control program.</summary>
public sealed class CinematicGlowPaletteFxProgramDefinition
{
    internal CinematicGlowPaletteFxProgramDefinition(
        CinematicGlowPaletteFxProgramOwner owner,
        ushort definitionPointer,
        ushort programStart,
        ushort colorByteIndex,
        int colorsPerFrame,
        ushort frameDuration)
    {
        Owner = owner;
        DefinitionPointer = definitionPointer;
        ProgramStart = programStart;
        ColorByteIndex = colorByteIndex;
        ColorsPerFrame = colorsPerFrame;
        FrameDuration = frameDuration;
    }

    /// <summary>The mutually exclusive glow owner.</summary>
    public CinematicGlowPaletteFxProgramOwner Owner { get; }

    /// <summary>The palette-FX definition identity that installs this program.</summary>
    /// <remarks>
    /// <c>$8D:E1BC</c> owns the old Mother Brain lights and <c>$8D:E1C0</c> the gunship.
    /// </remarks>
    public ushort DefinitionPointer { get; }

    /// <summary>The native instruction-list entry.</summary>
    /// <remarks>
    /// <c>$8D:C9BA</c> is the old Mother Brain loop and <c>$8D:CA4E</c> the gunship loop.
    /// </remarks>
    public ushort ProgramStart { get; }

    /// <summary>The first destination byte in CGRAM.</summary>
    public ushort ColorByteIndex { get; }

    /// <summary>The number of live BGR555 colors written by each timed record.</summary>
    public int ColorsPerFrame { get; }

    /// <summary>The cartridge-authored duration of each timed record.</summary>
    public ushort FrameDuration { get; }

    /// <summary>Bytes from one duration through its terminal wait command.</summary>
    public int FrameByteCount => (ColorsPerFrame + 2) * sizeof(ushort);

    /// <summary>The complete loop duration in frames.</summary>
    public int CycleFrames =>
        CinematicGlowPaletteFxProgramMechanicsDefinitions.FrameCount * FrameDuration;

    /// <summary>The first timed record after color-index setup.</summary>
    public ushort FirstFramePointer => unchecked((ushort)(ProgramStart + 4));

    /// <summary>The terminal <c>goto</c> command after all timed records.</summary>
    public ushort LoopInstructionPointer => unchecked((ushort)(FirstFramePointer +
        CinematicGlowPaletteFxProgramMechanicsDefinitions.FrameCount * FrameByteCount));

    /// <summary>Returns one timed-record pointer.</summary>
    public ushort FramePointer(int frame)
    {
        if ((uint)frame >= CinematicGlowPaletteFxProgramMechanicsDefinitions.FrameCount)
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

        for (int frame = 0;
             frame < CinematicGlowPaletteFxProgramMechanicsDefinitions.FrameCount;
             frame++)
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
