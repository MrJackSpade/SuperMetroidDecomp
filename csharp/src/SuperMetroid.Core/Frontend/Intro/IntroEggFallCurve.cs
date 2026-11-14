namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// How a piece of the hatching egg flies (#1825): its vertical speed for each update it has flown. It rises first, for
/// <see cref="RisingSpells"/> spells of seven updates, each spell a pixel an update slower than the one before and the
/// last a pixel an update upward. Within a spell the first update rises at the spell's whole speed, the next at three
/// quarters of a pixel less, and each update after an eighth of a pixel faster than the one before. Then it falls,
/// from rest, an eighth of a pixel an update faster each update, until <see cref="Updates"/> updates have passed.
/// </summary>
/// <param name="RisingSpells">The seven-update spells the piece rises for; the first rises this many pixels an update.</param>
/// <param name="Updates">The updates the curve covers; every piece has landed before its curve ends.</param>
internal sealed record IntroEggFallCurve(int RisingSpells, int Updates)
{
    /// <summary>The updates in one rising spell.</summary>
    private const int SpellUpdates = 7;

    /// <summary>How much faster the piece falls each update once it has stopped rising: an eighth of a pixel an update.</summary>
    private static readonly SignedSixteenSixteen Gravity = SignedSixteenSixteen.FromSixteenthsOfPixel(2);

    /// <summary>The piece's vertical speed on the update after it has flown <paramref name="updatesFlown"/> updates.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="updatesFlown"/> is negative or past the curve.</exception>
    internal SignedSixteenSixteen Speed(int updatesFlown)
    {
        if ((uint)updatesFlown >= Updates)
            throw new ArgumentOutOfRangeException(nameof(updatesFlown), updatesFlown, $"The curve covers {Updates} updates.");
        int risingUpdates = RisingSpells * SpellUpdates;
        if (updatesFlown >= risingUpdates)
            return Gravity * (updatesFlown - risingUpdates);
        int intoSpell = updatesFlown % SpellUpdates;
        SignedSixteenSixteen spellSpeed = SignedSixteenSixteen.FromWholePixels(updatesFlown / SpellUpdates - RisingSpells);
        return spellSpeed + Gravity * (intoSpell == 0 ? 0 : SpellUpdates - intoSpell);
    }
}
