namespace SuperMetroid.Core.Samus.Colours;

/// <summary>One glow sequence's segment in each of Samus's suits (#2387, #2557), read by the suit she wears.</summary>
/// <param name="Power">The segment shown in the Power Suit.</param>
/// <param name="Varia">The segment shown in the Varia Suit.</param>
/// <param name="Gravity">The segment shown in the Gravity Suit.</param>
internal sealed record SamusSuitGlows(SamusGlowSegment Power, SamusGlowSegment Varia, SamusGlowSegment Gravity)
{
    /// <summary>The segment shown in <paramref name="suit"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The suit is undefined.</exception>
    internal SamusGlowSegment this[SamusSuit suit] => suit switch
    {
        SamusSuit.Power => Power,
        SamusSuit.Varia => Varia,
        SamusSuit.Gravity => Gravity,
        _ => throw new ArgumentOutOfRangeException(nameof(suit), suit, null),
    };
}
