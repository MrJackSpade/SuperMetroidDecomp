namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Immutable control words for the Bomb and Golden Torizo belly palette loops.</summary>
/// <remarks>
/// Palette-FX definitions <c>$F759</c> and <c>$F75D</c> install matching six-frame
/// programs with different live BGR555 colors. This catalog owns only color-index setup,
/// enemy-death pre-instruction setup, durations, waits, and loop control.
/// Bomb and Golden programs start at $8D:E2E9 and $8D:E331, each selecting
/// CGRAM byte $0132 and installing enemy-zero-death pre-instruction $E2E0.
/// Records f=0..5 begin at $E2F1 or $E339 plus 10*f, write three live
/// colors, and end in $C595 wait. Gotos at $E32D and $E375 return to
/// the respective first records after a 52-frame cycle; f=6 reaches
/// control. All 36 mechanics words match the pinned NTSC J/U v1.0 ROM.
/// </remarks>
public static class TorizoBellyPaletteFxProgramMechanicsDefinitions
{
    /// <summary>Bomb Torizo palette-FX identity at $8D:F759.</summary>
    private const ushort BombDefinition = 0xf759;
    /// <summary>Golden Torizo palette-FX identity at $8D:F75D.</summary>
    private const ushort GoldenDefinition = 0xf75d;
    /// <summary>Bomb Torizo belly setup program at $8D:E2E9.</summary>
    private const ushort BombProgram = 0xe2e9;
    /// <summary>Golden Torizo belly setup program at $8D:E331.</summary>
    private const ushort GoldenProgram = 0xe331;

    /// <summary>Enumerates the Bomb and Golden program layouts from their named native starts.</summary>
    private sealed class ProgramDefinitions : IReadOnlyList<TorizoBellyPaletteFxProgramDefinition>
    {
        /// <summary>Exactly one belly palette loop for each Torizo variant.</summary>
        public int Count => 2;
        /// <summary>Builds the requested variant's entry, first color record, and loop-command pointers.</summary>
        public TorizoBellyPaletteFxProgramDefinition this[int index] => index switch
        {
            (int)TorizoBellyPaletteOwner.BombTorizo => Create(TorizoBellyPaletteOwner.BombTorizo, BombDefinition, BombProgram),
            (int)TorizoBellyPaletteOwner.GoldenTorizo => Create(TorizoBellyPaletteOwner.GoldenTorizo, GoldenDefinition, GoldenProgram),
            _ => throw new ArgumentOutOfRangeException(nameof(index)),
        };

        /// <summary>Derives record and loop offsets from the native setup length and six fixed-size frames.</summary>
        private static TorizoBellyPaletteFxProgramDefinition Create(
            TorizoBellyPaletteOwner owner, ushort definition, ushort start) =>
            new(owner, definition, start, (ushort)(start + 8),
                (ushort)(start + 8 + FrameCount * FrameByteCount));

        /// <summary>Yields both variant definitions in palette-definition order.</summary>
        public IEnumerator<TorizoBellyPaletteFxProgramDefinition> GetEnumerator()
        {
            for (int index = 0; index < Count; index++)
                yield return this[index];
        }
        /// <summary>Exposes the same two definitions to non-generic collection consumers.</summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
    /// <summary>Three BGR555 colors are presentation-owned by each timed record.</summary>
    public const int ColorsPerFrame = 3;

    /// <summary>Six timed records form one complete belly-color cycle.</summary>
    public const int FrameCount = 6;

    /// <summary>Bytes from one duration word through its terminal wait command.</summary>
    public const int FrameByteCount = 10;

    /// <summary>The byte index of the first Torizo belly color in CGRAM.</summary>
    public const ushort ColorByteIndex = 0x0132;

    /// <summary>The Bomb and Golden Torizo programs in palette-definition order.</summary>
    public static IReadOnlyList<TorizoBellyPaletteFxProgramDefinition> All { get; } = new ProgramDefinitions();

    /// <summary>Resolves one compiled mechanics word across both Torizo programs.</summary>
    public static bool TryReadMechanicsWord(ushort pointer, out ushort value)
    {
        foreach (TorizoBellyPaletteFxProgramDefinition definition in All)
        {
            if (definition.TryReadMechanicsWord(pointer, out value))
                return true;
        }

        value = 0;
        return false;
    }

    /// <summary>Bounded duration shared by both Torizo belly programs.</summary>
    /// <remarks>
    /// For f=0..5, duration is 10 when f mod 3 is zero, otherwise 8:
    /// exactly 10,8,8,10,8,8 in the pinned ROM, totaling 52 frames.
    /// </remarks>
    internal static ushort Duration(int frame)
    {
        if ((uint)frame >= FrameCount)
            throw new ArgumentOutOfRangeException(nameof(frame));
        return (ushort)(frame % 3 == 0 ? 10 : 8);
    }
}
