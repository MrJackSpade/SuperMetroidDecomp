namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>One complete looping title-screen ambient palette program.</summary>
public sealed class TitleScreenAmbientPaletteFxProgramDefinition
{
    /// <summary>Binds one semantic title palette owner to its calculated instruction layout.</summary>
    internal TitleScreenAmbientPaletteFxProgramDefinition(TitleScreenAmbientPaletteFxProgramOwner owner) => Owner = owner;
    /// <summary>Semantic title-palette owner selecting native definition $8D:E1A0 (tube) or $8D:E1A4 (displays); not a runtime palette-object slot index.</summary>
    public TitleScreenAmbientPaletteFxProgramOwner Owner { get; }
    /// <summary>Distinguishes the eight-shade tube loop from the two-state displays.</summary>
    internal bool IsTubeLight => Owner == TitleScreenAmbientPaletteFxProgramOwner.BabyMetroidTubeLight;
    /// <summary>Native instruction entries $8D:C7FA (tube) / $C862 (displays).</summary>
    public ushort ProgramStart => IsTubeLight ? TitleScreenAmbientPaletteFxProgramMechanicsDefinitions.TubeProgram
        : (ushort)(TitleScreenAmbientPaletteFxProgramMechanicsDefinitions.TubeLight.LoopInstructionPointer + 2 * sizeof(ushort));
    /// <summary>Native color destinations: byte $54 (tube) / $5C (displays).</summary>
    public ushort ColorByteIndex => (ushort)(FirstColor * sizeof(ushort));
    /// <summary>First CGRAM color slot controlled by the selected ambient program.</summary>
    private int FirstColor => IsTubeLight ? TitleScreenAmbientPaletteFxProgramMechanicsDefinitions.TubeFirstColor
        : TitleScreenAmbientPaletteFxProgramMechanicsDefinitions.TubeLastColor + 1;
    /// <summary>Timed palette-image records per loop: eight tube shades or two display states, distinct from the number of palette updates spent in the loop.</summary>
    public int FrameCount => IsTubeLight ? 2 * TitleScreenAmbientPaletteFxProgramMechanicsDefinitions.TubeDimmingSteps
        : TitleScreenAmbientPaletteFxProgramMechanicsDefinitions.DisplayStates;
    /// <summary>Editable packed RGB5 words per timed record: four for tube CGRAM slots 42..45 or two for display slots 46..47, excluding duration and wait control words.</summary>
    public int ColorsPerFrame => (IsTubeLight ? TitleScreenAmbientPaletteFxProgramMechanicsDefinitions.TubeLastColor
        : TitleScreenAmbientPaletteFxProgramMechanicsDefinitions.DisplayLastColor) - FirstColor + 1;
    /// <summary>Native timed-record exposure in palette-FX updates: ten for each tube shade or one for each display state, producing 80-update and two-update loops respectively; rendering does not advance it.</summary>
    public ushort FrameDuration => IsTubeLight ? TitleScreenAmbientPaletteFxProgramMechanicsDefinitions.TubeShadeTicks : (ushort)1;
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

    /// <summary>Resolves one compiled mechanics word while excluding live colors.</summary>
    /// <param name="pointer">Native bank-$8D word address, not a byte offset from <see cref="ProgramStart"/>.</param>
    /// <param name="value">The owned control/duration word, or zero when this program does not provide mechanics at that address.</param>
    /// <returns>True for setup, timed-record duration/wait, or terminal loop control; color payload words remain presentation-owned and return false.</returns>
    public bool TryReadMechanicsWord(ushort pointer, out ushort value)
    {
        value = pointer switch
        {
            var item when item == ProgramStart => (ushort)PaletteFxInstruction.SetColorIndex,
            var item when item == ProgramStart + 2 => ColorByteIndex,
            var item when item == LoopInstructionPointer => (ushort)PaletteFxInstruction.Goto,
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
                    (ushort)PaletteFxInstruction.Wait,
                _ => 0,
            };
            if (value != 0)
                return true;
        }

        return false;
    }
}
