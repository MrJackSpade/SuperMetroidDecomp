namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>One complete Samus-loading palette control program.</summary>
public sealed class SamusLoadingSuitPaletteFxColorDefinition
{
    /// <summary>Bank-local position of the one-frame record following four counted groups.</summary>
    private const ushort FinalFrameOffset = 0x0140;
    /// <summary>Bank-local position of deletion after the terminal palette record.</summary>
    private const ushort DeleteInstructionOffset = 0x0164;

    /// <summary>Pairs one suit's palette-FX header with its bank-$8D instruction-list entry.</summary>
    internal SamusLoadingSuitPaletteFxColorDefinition(
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
        ProgramStart + SamusLoadingSuitPaletteFxColorDefinitions
            .GroupStartOffset(group)));

    /// <summary>Returns the byte operand that initializes one group's replay timer.</summary>
    public ushort GroupTimerBytePointer(int group) =>
        unchecked((ushort)(GroupStartPointer(group) - 1));

    /// <summary>Returns one of the nine timed-record pointers.</summary>
    public ushort FramePointer(int frame)
    {
        if ((uint)frame >= SamusLoadingSuitPaletteFxColorDefinitions.FrameCount)
            throw new ArgumentOutOfRangeException(nameof(frame));
        if (frame == SamusLoadingSuitPaletteFxColorDefinitions.FrameCount - 1)
            return FinalFramePointer;

        int group = frame / SamusLoadingSuitPaletteFxColorDefinitions.FramesPerGroup;
        int frameInGroup = frame %
            SamusLoadingSuitPaletteFxColorDefinitions.FramesPerGroup;
        return unchecked((ushort)(GroupStartPointer(group) + frameInGroup *
            SamusLoadingSuitPaletteFxColorDefinitions.FrameByteCount));
    }

    /// <summary>Returns one live BGR555 color word in a timed record.</summary>
    public ushort ColorPointer(int frame, int color)
    {
        if ((uint)color >= SamusLoadingSuitPaletteFxColorDefinitions.ColorsPerFrame)
            throw new ArgumentOutOfRangeException(nameof(color));
        return unchecked((ushort)(FramePointer(frame) + sizeof(ushort) +
            color * sizeof(ushort)));
    }
}
