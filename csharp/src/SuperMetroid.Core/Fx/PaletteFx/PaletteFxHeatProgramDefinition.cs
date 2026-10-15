namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>One suit-specific Samus-in-heat palette control program.</summary>
/// <remarks>The $8D:E45E/E68A/E8B6 setup occupies eight bytes before phase zero.
/// Sixteen 34-byte records end at the terminal goto. These native layout relationships
/// calculate all cursor fields; the descriptor stores only suit identity and a calculated
/// list view, never a generated frame cache. Independently reviewed for #1165.</remarks>
public sealed class PaletteFxHeatProgramDefinition
{
    /// <summary>Fifteen BGR555 colors follow every timed duration word.</summary>
    public const int ColorsPerFrame = 15;

    /// <summary>Binds a suit identity to its calculated native frame address sequence.</summary>
    internal PaletteFxHeatProgramDefinition(PaletteFxHeatSuit suit)
    {
        Suit = suit;
        Frames = new FrameList(suit);
    }

    /// <summary>The mutually exclusive suit palette represented by this program.</summary>
    public PaletteFxHeatSuit Suit { get; }

    /// <summary>The sixteen timed records, calculated on access without stored rows.</summary>
    public IReadOnlyList<PaletteFxHeatProgramFrameDefinition> Frames { get; }

    /// <summary>Calculates sixteen fixed-stride frame descriptors for a selected suit.</summary>
    private sealed class FrameList(PaletteFxHeatSuit suit) : IReadOnlyList<PaletteFxHeatProgramFrameDefinition>
    {
        /// <summary>Gets the number of timed heat palette records per suit.</summary>
        public int Count => 16;
        /// <summary>Gets a phase's native duration-word address as a frame descriptor.</summary>
        public PaletteFxHeatProgramFrameDefinition this[int index]
        {
            get
            {
                if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
                return new(PaletteFxHeatInstructionListDefinitions.Resolve(suit, (ushort)index));
            }
        }
        /// <summary>Yields all sixteen calculated phase descriptors in program order.</summary>
        public IEnumerator<PaletteFxHeatProgramFrameDefinition> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        /// <summary>Exposes the same phase order to non-generic collection consumers.</summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
