namespace SuperMetroid.Core.Compatibility;

/// <summary>One sound library's request as a check of the <see cref="SoundAcceptanceClock"/> acted on it.</summary>
/// <param name="Request">The request taken at the previous check, now acknowledged: zero for none, otherwise a sound number from 1 through 255.</param>
/// <param name="Changed">Whether it differs from the request acted on at the previous check; a sound starts only for a changed nonzero request.</param>
[CompatibilityService("one sound library's request as a check of the sound acceptance clock acted on it")]
public readonly record struct SoundRequestCheck(int Request, bool Changed)
{
    /// <summary>The taken request, validated to the range a sound library can carry.</summary>
    public int Request { get; } = Ensure.BetweenInclusive(0, byte.MaxValue, Request);
}
