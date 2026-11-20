namespace SuperMetroid.Core.Samus.Colours;

/// <summary>
/// One of Samus's shade sequences (#2351, #2387): a glow (<see cref="SamusGlow"/>), and for a glow that differs by suit the
/// suit it is shown in (#2557), so each glow is named once and the suit is its own dimension.
/// </summary>
public readonly record struct SamusGlowSegment
{
    /// <summary>The sequence of <paramref name="glow"/> in <paramref name="suit"/>, or alike in every suit when none.</summary>
    private SamusGlowSegment(SamusGlow glow, SamusSuit? suit)
    {
        Glow = glow;
        Suit = suit;
    }

    /// <summary>The glow the sequence shows.</summary>
    public SamusGlow Glow { get; }

    /// <summary>The suit the sequence shows its glow in; none for a glow alike in every suit.</summary>
    public SamusSuit? Suit { get; }

    /// <summary>The sequence of <paramref name="glow"/>, a glow alike in every suit.</summary>
    public static SamusGlowSegment Of(SamusGlow glow) => new(glow, suit: null);

    /// <summary>The sequence of <paramref name="glow"/> in <paramref name="suit"/>, for a glow that differs by suit.</summary>
    public static SamusGlowSegment In(SamusGlow glow, SamusSuit suit) => new(glow, suit);

    /// <inheritdoc/>
    public override string ToString() => Suit is { } suit ? $"{Glow} in {suit}" : Glow.ToString();
}
