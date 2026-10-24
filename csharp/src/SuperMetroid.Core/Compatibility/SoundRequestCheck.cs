namespace SuperMetroid.Core.Compatibility;

/// <summary>One sound library's request as a check of the <see cref="SoundAcceptanceClock{TSound}"/> acted on it.</summary>
/// <typeparam name="TSound">The sound a library is asked for.</typeparam>
/// <param name="Request">The request taken at the previous check, now acknowledged, or none.</param>
/// <param name="Changed">Whether it differs from the request acted on at the previous check; a sound starts only for a changed request.</param>
[CompatibilityService("one sound library's request as a check of the sound acceptance clock acted on it")]
public readonly record struct SoundRequestCheck<TSound>(TSound? Request, bool Changed)
    where TSound : struct, IEquatable<TSound>;
