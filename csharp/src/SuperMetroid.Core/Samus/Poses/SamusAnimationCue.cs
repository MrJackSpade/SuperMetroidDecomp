namespace SuperMetroid.Core.Samus.Poses;

/// <summary>
/// The cue that closes a run of delays in a Samus animation: where the animation goes next, or which
/// transitional pose it publishes. Each kind is its own record; the animation acts on the kind.
/// </summary>
internal abstract record SamusAnimationCue : SamusPoseAnimationStep
{
    /// <summary>
    /// How many values the cue carries, which follow it in its segment: the poses it publishes, the items it tests and the
    /// frame it repeats from. A frame written onto one of them reads it as a step of its own
    /// (<see cref="Compatibility.FramesBeyondAnimation{TAnimation, TStep}"/>).
    /// </summary>
    internal abstract int CarriedValues { get; }
}
