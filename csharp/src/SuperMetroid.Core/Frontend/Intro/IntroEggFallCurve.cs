using System.Collections.Immutable;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// How a piece of the hatching egg flies (#1825, #2361): its authored vertical speed for each update it rises, then plain
/// gravity from rest, an eighth of a pixel an update faster each update, until <see cref="Updates"/> updates have passed.
/// </summary>
/// <param name="RisingSpeeds">The piece's vertical speed on each update it rises, in order; negative rises.</param>
/// <param name="Updates">The updates the curve covers; every piece has landed before its curve ends.</param>
internal sealed record IntroEggFallCurve(ImmutableArray<SignedSixteenSixteen> RisingSpeeds, int Updates)
{
    /// <summary>How much faster the piece falls each update once it has stopped rising: an eighth of a pixel an update.</summary>
    private static readonly SignedSixteenSixteen Gravity = SignedSixteenSixteen.FromSixteenthsOfPixel(2);

    /// <summary>The piece's vertical speed on the update after it has flown <paramref name="updatesFlown"/> updates.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="updatesFlown"/> is negative or past the curve.</exception>
    internal SignedSixteenSixteen Speed(int updatesFlown)
    {
        if ((uint)updatesFlown >= Updates)
            throw new ArgumentOutOfRangeException(nameof(updatesFlown), updatesFlown, $"The curve covers {Updates} updates.");
        return updatesFlown < RisingSpeeds.Length
            ? RisingSpeeds[updatesFlown]
            : Gravity * (updatesFlown - RisingSpeeds.Length);
    }
}
