namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>One Torizo-specific entry and six-frame belly palette loop.</summary>
public sealed class TorizoBellyPaletteFxColorDefinition
{
    /// <summary>Stores the native definition and derived instruction offsets for one Torizo palette loop.</summary>
    internal TorizoBellyPaletteFxColorDefinition(
        TorizoBellyPaletteOwner owner,
        ushort definitionPointer,
        ushort programStart,
        ushort firstFramePointer,
        ushort loopInstructionPointer)
    {
        Owner = owner;
        DefinitionPointer = definitionPointer;
        ProgramStart = programStart;
        FirstFramePointer = firstFramePointer;
        LoopInstructionPointer = loopInstructionPointer;
    }

    /// <summary>The Torizo variant represented by this program.</summary>
    public TorizoBellyPaletteOwner Owner { get; }

    /// <summary>The palette-FX definition identity that installs this program.</summary>
    public ushort DefinitionPointer { get; }

    /// <summary>The setup entry for this program.</summary>
    public ushort ProgramStart { get; }

    /// <summary>The first timed color record.</summary>
    public ushort FirstFramePointer { get; }

    /// <summary>The terminal <c>goto</c> command after the sixth record.</summary>
    public ushort LoopInstructionPointer { get; }

    /// <summary>Returns one timed-record pointer.</summary>
    public ushort FramePointer(int frame)
    {
        if ((uint)frame >= TorizoBellyPaletteFxColorDefinitions.FrameCount)
            throw new ArgumentOutOfRangeException(nameof(frame));
        return unchecked((ushort)(FirstFramePointer +
            frame * TorizoBellyPaletteFxColorDefinitions.FrameByteCount));
    }

    /// <summary>Returns one contiguous presentation-color address within a frame.</summary>
    /// <remarks>Color payload derivation remains pending under issue #1165.</remarks>
    public ushort ColorPointer(int frame, int color)
    {
        if ((uint)color >= TorizoBellyPaletteFxColorDefinitions.ColorsPerFrame)
            throw new ArgumentOutOfRangeException(nameof(color));
        return unchecked((ushort)(FramePointer(frame) + sizeof(ushort) +
            color * sizeof(ushort)));
    }
}
