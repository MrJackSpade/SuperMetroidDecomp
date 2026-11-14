namespace SuperMetroid.Core.Samus.Poses;

/// <summary>
/// The cue that closes a run of delays in a Samus animation: where the animation goes next, or which
/// transitional pose it publishes. Each kind is its own record; the animation acts on the kind.
/// </summary>
internal abstract record SamusAnimationCue : SamusPoseAnimationStep;
