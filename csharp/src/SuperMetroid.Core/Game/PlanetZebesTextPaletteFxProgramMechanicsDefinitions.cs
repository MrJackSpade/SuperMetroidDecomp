namespace SuperMetroid.Core.Game;

/// <summary>The mutually exclusive PLANET ZEBES text-fade palette program.</summary>
public enum PlanetZebesTextPaletteFxProgramOwner
{
    /// <summary>The opening cinematic's text fade-in.</summary>
    FadeIn,
    /// <summary>The opening cinematic's text fade-out.</summary>
    FadeOut,
}

/// <summary>Immutable mechanics for the cinematic PLANET ZEBES text fades.</summary>
/// <remarks>
/// Issue #840 / #625: the pinned NTSC J/U v1.0 ROM encodes both definitions as
/// <c>SetColorIndex($0102)</c>, eight ten-byte records of
/// <c>3, colors[3], Wait</c>, then <c>Delete</c>. Fade-in starts at
/// <c>$8D:C90E</c> (first record <c>$C912</c>, delete <c>$C962</c>);
/// fade-out starts at <c>$8D:C964</c> (first record <c>$C968</c>, delete
/// <c>$C9B8</c>). Each one-shot lasts 24 frames. All 38 control words match
/// the ROM. For frame <c>i</c> and color <c>c</c>, the ROM's fade-out color
/// equals fade-in color <c>(7 - i, c)</c> at all 24 positions. The eight
/// color rows and their reverse form48 native presentation identities. The
/// presentation view calculates rounded RGB5 interpolation from three required
/// endpoints and preserves independent supplied edits. This catalog supplies
/// control words; the text endpoints, cadence and palette selection are authored inputs.
/// ROM SHA-256:
/// <c>12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72</c>.
/// </remarks>
public static class PlanetZebesTextPaletteFxProgramMechanicsDefinitions
{
    /// <summary>$8D:C912..C960: eight brightness records per fade.</summary>
    public const int FrameCount = 8;

    /// <summary>$8D:C914..C918: three text colors per record.</summary>
    public const int ColorsPerFrame = 3;

    /// <summary>Bytes from one duration through its terminal wait command.</summary>
    public const int FrameByteCount = sizeof(ushort) * (ColorsPerFrame + 2);

    /// <summary>$8D:C912/C968: the authored three-frame hold.</summary>
    public const ushort FrameDuration = 3;
    /// <summary>$8D:C90E: native fade-in setup; fade-out follows its terminal delete.</summary>
    internal const ushort FadeInProgram = 0xC90E;
    /// <summary>$8D:C910/C966: CGRAM palette 8 for the cinematic text.</summary>
    internal const int TextPalette = 8;
    /// <summary>$8D:C910/C966: first text color 1 within the selected palette.</summary>
    internal const int FirstTextColor = 1;

    /// <summary>The fade-in and fade-out programs in native order without cached definition records.</summary>
    public static IReadOnlyList<PlanetZebesTextPaletteFxProgramDefinition> All { get; } = new DefinitionSequence();

    /// <summary>Provides the fade-in and fade-out definitions by constructing each requested entry on demand.</summary>
    private sealed class DefinitionSequence : IReadOnlyList<PlanetZebesTextPaletteFxProgramDefinition>
    {
        /// <summary>Gets the two cinematic text-fade programs in native execution order.</summary>
        public int Count => 2;

        /// <summary>Gets a fade definition by its position in the native sequence.</summary>
        /// <param name="index">Zero for fade-in or one for fade-out.</param>
        /// <returns>The requested text-fade program definition.</returns>
        public PlanetZebesTextPaletteFxProgramDefinition this[int index] => index switch
        {
            0 => new(PlanetZebesTextPaletteFxProgramOwner.FadeIn),
            1 => new(PlanetZebesTextPaletteFxProgramOwner.FadeOut),
            _ => throw new ArgumentOutOfRangeException(nameof(index)),
        };
        /// <summary>Enumerates fade-in followed by fade-out.</summary>
        /// <returns>An enumerator that constructs each definition as it is requested.</returns>
        public IEnumerator<PlanetZebesTextPaletteFxProgramDefinition> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        /// <summary>Returns a non-generic enumerator over this sequence.</summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
    /// <summary>Resolves one compiled mechanics word across both programs.</summary>
    public static bool TryReadMechanicsWord(ushort pointer, out ushort value)
    {
        foreach (PlanetZebesTextPaletteFxProgramDefinition definition in All)
        {
            if (definition.TryReadMechanicsWord(pointer, out value))
                return true;
        }

        value = 0;
        return false;
    }
}

/// <summary>One complete cinematic PLANET ZEBES text-fade control program.</summary>
public sealed class PlanetZebesTextPaletteFxProgramDefinition
{
    /// <summary>Creates the control-program view for one cinematic text-fade owner.</summary>
    /// <param name="owner">Selects whether the definition describes the fade-in or fade-out program.</param>
    internal PlanetZebesTextPaletteFxProgramDefinition(PlanetZebesTextPaletteFxProgramOwner owner) => Owner = owner;

    /// <summary>The mutually exclusive text-fade owner.</summary>
    public PlanetZebesTextPaletteFxProgramOwner Owner { get; }

    /// <summary>Fade-out begins after fade-in's setup, timed records and final delete word.</summary>
    public ushort ProgramStart => (ushort)(PlanetZebesTextPaletteFxProgramMechanicsDefinitions.FadeInProgram
        + (Owner == PlanetZebesTextPaletteFxProgramOwner.FadeOut
            ? 3 * sizeof(ushort) + PlanetZebesTextPaletteFxProgramMechanicsDefinitions.FrameCount
                * PlanetZebesTextPaletteFxProgramMechanicsDefinitions.FrameByteCount : 0));

    /// <summary>Byte destination from the independently required text palette and first color.</summary>
    public ushort ColorByteIndex => Owner switch
    {
        PlanetZebesTextPaletteFxProgramOwner.FadeIn or PlanetZebesTextPaletteFxProgramOwner.FadeOut =>
            (ushort)(sizeof(ushort) * (16 * PlanetZebesTextPaletteFxProgramMechanicsDefinitions.TextPalette
                + PlanetZebesTextPaletteFxProgramMechanicsDefinitions.FirstTextColor)),
        _ => throw new InvalidOperationException("Unsupported PLANET ZEBES fade owner."),
    };
    /// <summary>The first timed record after color-index setup.</summary>
    public ushort FirstFramePointer => unchecked((ushort)(ProgramStart + 4));

    /// <summary>The terminal <c>delete</c> command after all timed records.</summary>
    public ushort DeleteInstructionPointer => unchecked((ushort)(FirstFramePointer +
        PlanetZebesTextPaletteFxProgramMechanicsDefinitions.FrameCount *
        PlanetZebesTextPaletteFxProgramMechanicsDefinitions.FrameByteCount));

    /// <summary>Returns one timed-record pointer.</summary>
    public ushort FramePointer(int frame)
    {
        if ((uint)frame >= PlanetZebesTextPaletteFxProgramMechanicsDefinitions.FrameCount)
            throw new ArgumentOutOfRangeException(nameof(frame));
        return unchecked((ushort)(FirstFramePointer + frame *
            PlanetZebesTextPaletteFxProgramMechanicsDefinitions.FrameByteCount));
    }

    /// <summary>Returns one presentation-owned BGR555 word in a timed record.</summary>
    public ushort ColorPointer(int frame, int color)
    {
        if ((uint)color >= PlanetZebesTextPaletteFxProgramMechanicsDefinitions.ColorsPerFrame)
            throw new ArgumentOutOfRangeException(nameof(color));
        return unchecked((ushort)(FramePointer(frame) + sizeof(ushort) +
            color * sizeof(ushort)));
    }

    /// <summary>Resolves one compiled mechanics word while excluding live colors.</summary>
    public bool TryReadMechanicsWord(ushort pointer, out ushort value)
    {
        value = pointer switch
        {
            var item when item == ProgramStart => PaletteFxInstructionCodes.SetColorIndex,
            var item when item == ProgramStart + 2 => ColorByteIndex,
            var item when item == DeleteInstructionPointer => PaletteFxInstructionCodes.Delete,
            _ => 0,
        };
        if (value != 0)
            return true;

        for (int frame = 0;
             frame < PlanetZebesTextPaletteFxProgramMechanicsDefinitions.FrameCount;
             frame++)
        {
            int offset = pointer - FramePointer(frame);
            value = offset switch
            {
                0 => PlanetZebesTextPaletteFxProgramMechanicsDefinitions.FrameDuration,
                PlanetZebesTextPaletteFxProgramMechanicsDefinitions.FrameByteCount -
                    sizeof(ushort) => PaletteFxInstructionCodes.Wait,
                _ => 0,
            };
            if (value != 0)
                return true;
        }

        return false;
    }
}
