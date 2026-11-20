namespace SuperMetroid.Core.Compatibility;

/// <summary>One sound library's request as a check of the <see cref="SoundAcceptanceClock{TLibrary, TSound}"/> acted on it.</summary>
/// <typeparam name="TSound">The sound a library is asked for.</typeparam>
/// <param name="Request">The request taken at the previous check, now acknowledged, or none.</param>
/// <param name="Changed">Whether it differs from the request acted on at the previous check; a sound starts only for a changed request.</param>
/// <remarks>
/// Made by each request check of the sound acceptance clock, once per library. Evidence: the original sound program acts
/// at each check on the request it read at the previous one, and starts a sound only when that request differs from the
/// one it last acted on; the sound acceptance clock check requires the session's sound starts to be exactly the changed
/// requests a model of the sound program's checks sees, and its acknowledgements to match a recording of the emulated
/// sound program.
/// </remarks>
[CompatibilityService("one sound library's request as a check of the sound acceptance clock acted on it")]
public readonly record struct SoundRequestCheck<TSound>(TSound? Request, bool Changed)
    where TSound : struct, IEquatable<TSound>;
