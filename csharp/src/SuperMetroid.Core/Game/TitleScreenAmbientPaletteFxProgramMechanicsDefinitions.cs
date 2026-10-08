using SuperMetroid.Core.Frontend;

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
/// Issue #853 / #625: pinned NTSC J/U v1.0 ROM definition <c>$8D:E1A0</c>
/// enters at <c>$8D:C7FA</c>: <c>SetColorIndex($0054)</c>, eight 12-byte
/// records of <c>10, colors[4], Wait</c>, then <c>Goto($C7FE)</c> at
/// <c>$C85E</c> (80 frames per loop). All 20 control words match. The
/// first row reuses the initial title palette. Tube channels interpolate to their
/// clamped endpoint using the minimum phase count at the selected three-unit
/// scale and nearest/even integer quantization; no green exception is needed.
/// Issue #854 / #625: definition <c>$8D:E1A4</c> enters at
/// <c>$8D:C862</c>, sets color index <c>$005C</c>, and alternates two
/// eight-byte records of <c>1, colors[2], Wait</c> from <c>$C866</c>;
/// <c>Goto($C866)</c> at <c>$C876</c> repeats the two-frame cycle. All
/// eight control words match. Even records write <c>[13FF,0BB1]</c> and
/// odd records write <c>[00AC,0145]</c>. These frames reuse the initial
/// title palette's material paint through named bindings and calculated shading.
/// The initial palette's independent paint values remain separately required
/// under TitlePalettePresentation.colors; each supplied initial or animated
/// edit preserves every other installed value. Missing inputs report an error
/// rather than falling back to a runtime cartridge read.
/// ROM SHA-256:
/// <c>12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72</c>.
/// </remarks>
public static class TitleScreenAmbientPaletteFxProgramMechanicsDefinitions
{
    /// <summary>$8D:C7FE-C85D: four dimming steps and their reflected return form the tube pulse. This selected pulse depth is authored title choreography.</summary>
    internal const int TubeDimmingSteps = 4;
    /// <summary>$8D:C7FE and each following tube record: authored ten-tick shade exposure in this title pulse.</summary>
    internal const ushort TubeShadeTicks = 10;
    /// <summary>$8D:C866/C86E: bright and dim display states alternate on consecutive palette updates.</summary>
    internal const int DisplayStates = 2;
    /// <summary>$8D:C7FC: first tube material color is CGRAM42; four material slots end at45.</summary>
    internal const int TubeFirstColor = 42, TubeLastColor = 45;
    /// <summary>$8D:C864: the adjacent display material slots end at CGRAM47.</summary>
    internal const int DisplayLastColor = 47;
    /// <summary>$8D:C7FA, InstList_PaletteFXObject_TitleScreenBabyMetroidTubeLight_0.</summary>
    internal const ushort TubeProgram = 0xc7fa;
    /// <summary>$8D:E1A0 / $C7FA: the eight-frame baby-Metroid tube light.</summary>
    public static TitleScreenAmbientPaletteFxProgramDefinition TubeLight { get; } =
        new(TitleScreenAmbientPaletteFxProgramOwner.BabyMetroidTubeLight);
    /// <summary>$8D:E1A4 / $C862: the two-frame flickering displays.</summary>
    public static TitleScreenAmbientPaletteFxProgramDefinition Displays { get; } =
        new(TitleScreenAmbientPaletteFxProgramOwner.FlickeringDisplays);

    /// <summary>The two semantic palette owners in native definition order.</summary>
    public static IReadOnlyList<TitleScreenAmbientPaletteFxProgramDefinition> All { get; } = new ProgramList();

    private sealed class ProgramList : IReadOnlyList<TitleScreenAmbientPaletteFxProgramDefinition>
    {
        public int Count => 2;
        public TitleScreenAmbientPaletteFxProgramDefinition this[int index] => index switch
        {
            0 => TubeLight, 1 => Displays, _ => throw new ArgumentOutOfRangeException(nameof(index)),
        };
        public IEnumerator<TitleScreenAmbientPaletteFxProgramDefinition> GetEnumerator()
        {
            yield return TubeLight;
            yield return Displays;
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }

    /// <summary>Resolves a control word through the two mutually exclusive palette programs.</summary>
    /// <param name="pointer">Same-bank $8D word address in the tube or display program, not a program-relative offset.</param>
    /// <param name="value">Resolved instruction, duration, or control operand; zero when neither program owns that mechanics address.</param>
    /// <returns>True only for a compiled mechanics word; editable color-word addresses and unknown addresses return false.</returns>
    public static bool TryReadMechanicsWord(ushort pointer, out ushort value) =>
        TubeLight.TryReadMechanicsWord(pointer, out value) || Displays.TryReadMechanicsWord(pointer, out value);
}
/// <summary>One complete looping title-screen ambient palette program.</summary>
public sealed class TitleScreenAmbientPaletteFxProgramDefinition
{
    internal TitleScreenAmbientPaletteFxProgramDefinition(TitleScreenAmbientPaletteFxProgramOwner owner) => Owner = owner;
    /// <summary>Semantic title-palette owner selecting native definition $8D:E1A0 (tube) or $8D:E1A4 (displays); not a runtime palette-object slot index.</summary>
    public TitleScreenAmbientPaletteFxProgramOwner Owner { get; }
    private bool IsTubeLight => Owner == TitleScreenAmbientPaletteFxProgramOwner.BabyMetroidTubeLight;
    /// <summary>Native instruction entries $8D:C7FA (tube) / $C862 (displays).</summary>
    public ushort ProgramStart => IsTubeLight ? TitleScreenAmbientPaletteFxProgramMechanicsDefinitions.TubeProgram
        : (ushort)(TitleScreenAmbientPaletteFxProgramMechanicsDefinitions.TubeLight.LoopInstructionPointer + 2 * sizeof(ushort));
    /// <summary>Native color destinations: byte $54 (tube) / $5C (displays).</summary>
    public ushort ColorByteIndex => (ushort)(FirstColor * sizeof(ushort));
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
