using System.Collections.Immutable;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// How a piece of the hatching egg flies (#1825, #2361): its authored vertical speed for each update it rises, then plain
/// gravity from rest (<see cref="IntroEggMotionDefinitions.FallGravity"/>), until <see cref="Updates"/> updates have passed.
/// </summary>
/// <param name="RisingSpeeds">The piece's vertical speed on each update it rises, in order; negative rises.</param>
/// <param name="Updates">The updates the curve covers; every piece has landed before its curve ends.</param>
internal sealed partial record IntroEggFallCurve(ImmutableArray<SignedSixteenSixteen> RisingSpeeds, int Updates)
{
    /// <summary>The piece's vertical speed on the update after it has flown <paramref name="updatesFlown"/> updates.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="updatesFlown"/> is negative or past the curve.</exception>
    internal SignedSixteenSixteen Speed(int updatesFlown)
    {
        if ((uint)updatesFlown >= Updates)
            throw new ArgumentOutOfRangeException(nameof(updatesFlown), updatesFlown, $"The curve covers {Updates} updates.");
        return updatesFlown < RisingSpeeds.Length
            ? RisingSpeeds[updatesFlown]
            : IntroEggMotionDefinitions.FallGravity * (updatesFlown - RisingSpeeds.Length);
    }
}
