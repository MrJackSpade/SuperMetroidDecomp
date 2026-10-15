namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>One complete looping title-screen ambient palette program.</summary>
public sealed class TitleScreenAmbientPaletteFxColorDefinition
{
    /// <summary>Binds one semantic title palette owner to its calculated instruction layout.</summary>
    internal TitleScreenAmbientPaletteFxColorDefinition(TitleScreenAmbientPaletteFxProgramOwner owner) => Owner = owner;
    /// <summary>Semantic title-palette owner selecting native definition $8D:E1A0 (tube) or $8D:E1A4 (displays); not a runtime palette-object slot index.</summary>
    public TitleScreenAmbientPaletteFxProgramOwner Owner { get; }
    /// <summary>Distinguishes the eight-shade tube loop from the two-state displays.</summary>
    internal bool IsTubeLight => Owner == TitleScreenAmbientPaletteFxProgramOwner.BabyMetroidTubeLight;
    /// <summary>Native instruction entries $8D:C7FA (tube) / $C862 (displays).</summary>
    public ushort ProgramStart => IsTubeLight ? TitleScreenAmbientPaletteFxColorDefinitions.TubeProgram
        : (ushort)(TitleScreenAmbientPaletteFxColorDefinitions.TubeLight.LoopInstructionPointer + 2 * sizeof(ushort));
    /// <summary>Native color destinations: byte $54 (tube) / $5C (displays).</summary>
    public ushort ColorByteIndex => (ushort)(FirstColor * sizeof(ushort));
    /// <summary>First CGRAM color slot controlled by the selected ambient program.</summary>
    private int FirstColor => IsTubeLight ? TitleScreenAmbientPaletteFxColorDefinitions.TubeFirstColor
        : TitleScreenAmbientPaletteFxColorDefinitions.TubeLastColor + 1;
    /// <summary>Timed palette-image records per loop: eight tube shades or two display states, distinct from the number of palette updates spent in the loop.</summary>
    public int FrameCount => IsTubeLight ? 2 * TitleScreenAmbientPaletteFxColorDefinitions.TubeDimmingSteps
        : TitleScreenAmbientPaletteFxColorDefinitions.DisplayStates;
    /// <summary>Editable packed RGB5 words per timed record: four for tube CGRAM slots 42..45 or two for display slots 46..47, excluding duration and wait control words.</summary>
    public int ColorsPerFrame => (IsTubeLight ? TitleScreenAmbientPaletteFxColorDefinitions.TubeLastColor
        : TitleScreenAmbientPaletteFxColorDefinitions.DisplayLastColor) - FirstColor + 1;
    /// <summary>Native timed-record exposure in palette-FX updates: ten for each tube shade or one for each display state, producing 80-update and two-update loops respectively; rendering does not advance it.</summary>
    public ushort FrameDuration => IsTubeLight ? TitleScreenAmbientPaletteFxColorDefinitions.TubeShadeTicks : (ushort)1;
    /// <summary>Bytes from one duration through its terminal wait command.</summary>
    public int FrameByteCount => sizeof(ushort) + ColorsPerFrame * sizeof(ushort) +
        sizeof(ushort);

    /// <summary>The first timed record after color-index setup.</summary>
    public ushort FirstFramePointer => unchecked((ushort)(ProgramStart + 4));

    /// <summary>The terminal <c>goto</c> command after all timed records.</summary>
    public ushort LoopInstructionPointer =>
        unchecked((ushort)(FirstFramePointer + FrameCount * FrameByteCount));

    /// <summary>Returns one timed-record pointer.</summary>
    /// <param name="frame">Zero-based record ordinal, 0..7 for the tube or 0..1 for displays; not an elapsed-update count.</param>
    /// <returns>The bank-$8D word pointer to the record's duration, before its color words.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The record ordinal is outside this owner's loop.</exception>
    public ushort FramePointer(int frame)
    {
        if ((uint)frame >= FrameCount)
            throw new ArgumentOutOfRangeException(nameof(frame));
        return unchecked((ushort)(FirstFramePointer + frame * FrameByteCount));
    }
}
