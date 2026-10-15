namespace SuperMetroid.Core.Fx.PaletteFx;

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
public static class TitleScreenAmbientPaletteFxColorDefinitions
{
    /// <summary>$8D:C7FE-C85D: four dimming steps and their reflected return form the tube pulse. This selected pulse depth is authored title choreography.</summary>
    internal const int TubeDimmingSteps = 4;
    /// <summary>$8D:C866/C86E: bright and dim display states alternate on consecutive palette updates.</summary>
    internal const int DisplayStates = 2;
    /// <summary>$8D:C7FC: first tube material color is CGRAM42; four material slots end at45.</summary>
    internal const int TubeFirstColor = 42, TubeLastColor = 45;
    /// <summary>$8D:C864: the adjacent display material slots end at CGRAM47.</summary>
    internal const int DisplayLastColor = 47;
    /// <summary>$8D:C7FA, InstList_PaletteFXObject_TitleScreenBabyMetroidTubeLight_0.</summary>
    internal const ushort TubeProgram = 0xc7fa;
    /// <summary>$8D:E1A0 / $C7FA: the eight-frame baby-Metroid tube light.</summary>
    public static TitleScreenAmbientPaletteFxColorDefinition TubeLight { get; } =
        new(TitleScreenAmbientPaletteFxProgramOwner.BabyMetroidTubeLight);
    /// <summary>$8D:E1A4 / $C862: the two-frame flickering displays.</summary>
    public static TitleScreenAmbientPaletteFxColorDefinition Displays { get; } =
        new(TitleScreenAmbientPaletteFxProgramOwner.FlickeringDisplays);

    /// <summary>The two semantic palette owners in native definition order.</summary>
    public static IReadOnlyList<TitleScreenAmbientPaletteFxColorDefinition> All { get; } = new ProgramList();

    /// <summary>Indexable two-program title palette set in native definition order.</summary>
    private sealed class ProgramList : IReadOnlyList<TitleScreenAmbientPaletteFxColorDefinition>
    {
        /// <summary>Tube light and display flicker are the two native ambient owners.</summary>
        public int Count => 2;
        /// <summary>Selects the tube or display program by its native definition ordinal.</summary>
        public TitleScreenAmbientPaletteFxColorDefinition this[int index] => index switch
        {
            0 => TubeLight, 1 => Displays, _ => throw new ArgumentOutOfRangeException(nameof(index)),
        };
        /// <summary>Enumerates tube light before display flicker.</summary>
        public IEnumerator<TitleScreenAmbientPaletteFxColorDefinition> GetEnumerator()
        {
            yield return TubeLight;
            yield return Displays;
        }
        /// <summary>Uses the typed program enumeration for nongeneric callers.</summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
