namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>
/// Immutable control words for the three Samus-in-heat palette programs in bank $8D.
/// </summary>
/// <remarks>
/// Each timed record contains fifteen BGR555 colors between its duration and terminal
/// wait command. Those 720 color words remain live presentation data; this catalog owns
/// only program setup, timing, waits, and loop control.
/// Power, Varia, and Gravity enter at $8D:E45E, $E68A, and $E8B6;
/// each installs heat pre-instruction $E379 and CGRAM byte $0182.
/// Their sixteen records start at $E466, $E692, or $E8BE plus $22*p,
/// p=0..15, and each ends in $C595 wait. Gotos at $E686, $E8B2,
/// and $EADE return to phase zero; p=16 reaches control. All 114
/// mechanics words match the pinned NTSC J/U v1.0 ROM.
/// </remarks>
public static class PaletteFxHeatProgramMechanicsDefinitions
{
    /// <summary>Compiled heat palette control for the Power Suit.</summary>
    private static readonly PaletteFxHeatProgramDefinition Power = new(PaletteFxHeatSuit.Power);
    /// <summary>Compiled heat palette control for the Varia Suit.</summary>
    private static readonly PaletteFxHeatProgramDefinition Varia = new(PaletteFxHeatSuit.Varia);
    /// <summary>Compiled heat palette control for the Gravity Suit.</summary>
    private static readonly PaletteFxHeatProgramDefinition Gravity = new(PaletteFxHeatSuit.Gravity);

    /// <summary>The Power, Varia, and Gravity programs in native selection order.</summary>
    public static IReadOnlyList<PaletteFxHeatProgramDefinition> All { get; } = new ProgramList();

    /// <summary>Exposes the three fixed suit programs in native selection order without a mutable array.</summary>
    private sealed class ProgramList : IReadOnlyList<PaletteFxHeatProgramDefinition>
    {
        /// <summary>Gets the number of native Samus-in-heat suit programs.</summary>
        public int Count => 3;
        /// <summary>Gets the Power, Varia, or Gravity program at its native selection index.</summary>
        public PaletteFxHeatProgramDefinition this[int index] => index switch
        {
            0 => Power, 1 => Varia, 2 => Gravity,
            _ => throw new ArgumentOutOfRangeException(nameof(index)),
        };
        /// <summary>Yields Power, Varia, and Gravity heat programs in native order.</summary>
        public IEnumerator<PaletteFxHeatProgramDefinition> GetEnumerator()
        {
            yield return Power;
            yield return Varia;
            yield return Gravity;
        }
        /// <summary>Exposes the same suit order to non-generic collection consumers.</summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }

    /// <summary>Resolves one compiled mechanics word across the three named suit programs.</summary>
    public static bool TryReadMechanicsWord(ushort pointer, out ushort value) =>
        Power.TryReadMechanicsWord(pointer, out value) ||
        Varia.TryReadMechanicsWord(pointer, out value) ||
        Gravity.TryReadMechanicsWord(pointer, out value);
}
